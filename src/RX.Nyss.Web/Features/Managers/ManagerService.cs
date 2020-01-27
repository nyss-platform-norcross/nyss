using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Managers.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Managers
{
    public interface IManagerService
    {
        Task<Result> Create(int nationalSocietyId, CreateManagerRequestDto createManagerRequestDto);
        Task<Result<GetManagerResponseDto>> Get(int managerId);
        Task<Result> Edit(int managerId, EditManagerRequestDto editManagerRequestDto);
        Task<Result> Delete(int managerId);
        Task DeleteIncludingHeadManagerFlag(int managerId);
    }


    public class ManagerService : IManagerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;

        public ManagerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
        }

        public async Task<Result> Create(int nationalSocietyId, CreateManagerRequestDto createManagerRequestDto)
        {
            try
            {
                string securityStamp;
                ManagerUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createManagerRequestDto.Email, Role.Manager);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    user = await CreateManagerUser(identityUser, nationalSocietyId, createManagerRequestDto);

                    transactionScope.Complete();
                }
                await _verificationEmailService.SendVerificationEmail(user, securityStamp);
                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<ManagerUser> CreateManagerUser(IdentityUser identityUser, int nationalSocietyId, CreateManagerRequestDto createManagerRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties.Include(ns => ns.ContentLanguage)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }
            if (nationalSociety.IsArchived)
            {
                throw new ResultException(ResultKey.User.Registration.CannotCreateUsersInArchivedNationalSociety);
            }

            var defaultUserApplicationLanguage = await _dataContext.ApplicationLanguages
                .SingleOrDefaultAsync(al => al.LanguageCode == nationalSociety.ContentLanguage.LanguageCode);

            var user = new ManagerUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createManagerRequestDto.Name,
                PhoneNumber = createManagerRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createManagerRequestDto.AdditionalPhoneNumber,
                Organization = createManagerRequestDto.Organization,
                ApplicationLanguage = defaultUserApplicationLanguage,
            };

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            return user;
        }

        private UserNationalSociety CreateUserNationalSocietyReference(NationalSociety nationalSociety, User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        public async Task<Result<GetManagerResponseDto>> Get(int nationalSocietyUserId)
        {
            var manager = await _dataContext.Users.FilterAvailable()
                .OfType<ManagerUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetManagerResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = u.Role,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization,
                })
                .SingleOrDefaultAsync();

            if (manager == null)
            {
                _loggerAdapter.Debug($"Data manager with id {nationalSocietyUserId} was not found");
                return Error<GetManagerResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetManagerResponseDto>(manager, true);
        }

        public async Task<Result> Edit(int managerId, EditManagerRequestDto editManagerRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<ManagerUser>(managerId);

                user.Name = editManagerRequestDto.Name;
                user.PhoneNumber = editManagerRequestDto.PhoneNumber;
                user.Organization = editManagerRequestDto.Organization;
                user.AdditionalPhoneNumber = editManagerRequestDto.AdditionalPhoneNumber;

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Delete(int managerId)
        {
            try
            {
                await _deleteUserService.EnsureCanDeleteUser(managerId, Role.Manager);

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await DeleteFromDb(managerId);

                await _dataContext.SaveChangesAsync();

                transactionScope.Complete();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task DeleteFromDb(int managerId, bool allowHeadManagerDeletion = false)
        {
            var manager = await _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<ManagerUser>(managerId);

            await HandleHeadManagerStatus(manager, allowHeadManagerDeletion);

            _nationalSocietyUserService.DeleteNationalSocietyUser<ManagerUser>(manager);
            await _identityUserRegistrationService.DeleteIdentityUser(manager.IdentityUserId);
        }

        private async Task HandleHeadManagerStatus(ManagerUser manager, bool allowHeadManagerDeletion)
        {
            var nationalSociety = await _dataContext.NationalSocieties.FindAsync(manager.UserNationalSocieties.Single().NationalSocietyId);
            if (nationalSociety.PendingHeadManager == manager)
            {
                nationalSociety.PendingHeadManager = null;
            }
            if (nationalSociety.HeadManager == manager)
            {
                if (!allowHeadManagerDeletion)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadManager);
                }
                nationalSociety.HeadManager = null;
            }
        }

        public async Task DeleteIncludingHeadManagerFlag(int managerId) =>
            await DeleteFromDb(managerId, true);
    }
}
