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
using RX.Nyss.Web.Features.Coordinators.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Coordinators
{
    public interface ICoordinatorService
    {
        Task<Result> Create(int nationalSocietyId, CreateCoordinatorRequestDto createCoordinatorRequestDto);
        Task<Result<GetCoordinatorResponseDto>> Get(int coordinatorId);
        Task<Result> Edit(int coordinatorId, EditCoordinatorRequestDto editCoordinatorRequestDto);
        Task<Result> Delete(int coordinatorId);
        Task DeleteIncludingHeadCoordinatorFlag(int coordinatorId);
    }


    public class CoordinatorService : ICoordinatorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;

        public CoordinatorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
        }

        public async Task<Result> Create(int nationalSocietyId, CreateCoordinatorRequestDto createCoordinatorRequestDto)
        {
            try
            {
                string securityStamp;
                CoordinatorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createCoordinatorRequestDto.Email, Role.Coordinator);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    user = await CreateCoordinatorUser(identityUser, nationalSocietyId, createCoordinatorRequestDto);

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

        public async Task<Result<GetCoordinatorResponseDto>> Get(int nationalSocietyUserId)
        {
            var coordinator = await _dataContext.Users.FilterAvailable()
                .OfType<CoordinatorUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetCoordinatorResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = u.Role,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization
                })
                .SingleOrDefaultAsync();

            if (coordinator == null)
            {
                _loggerAdapter.Debug($"Data coordinator with id {nationalSocietyUserId} was not found");
                return Error<GetCoordinatorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetCoordinatorResponseDto>(coordinator, true);
        }

        public async Task<Result> Edit(int coordinatorId, EditCoordinatorRequestDto editCoordinatorRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<CoordinatorUser>(coordinatorId);

                user.Name = editCoordinatorRequestDto.Name;
                user.PhoneNumber = editCoordinatorRequestDto.PhoneNumber;
                user.Organization = editCoordinatorRequestDto.Organization;
                user.AdditionalPhoneNumber = editCoordinatorRequestDto.AdditionalPhoneNumber;

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Delete(int coordinatorId)
        {
            try
            {
                await _deleteUserService.EnsureCanDeleteUser(coordinatorId, Role.Coordinator);

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await DeleteFromDb(coordinatorId);

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

        public async Task DeleteIncludingHeadCoordinatorFlag(int coordinatorId) =>
            await DeleteFromDb(coordinatorId, true);

        private async Task<CoordinatorUser> CreateCoordinatorUser(IdentityUser identityUser, int nationalSocietyId, CreateCoordinatorRequestDto createCoordinatorRequestDto)
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

            var user = new CoordinatorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createCoordinatorRequestDto.Name,
                PhoneNumber = createCoordinatorRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createCoordinatorRequestDto.AdditionalPhoneNumber,
                Organization = createCoordinatorRequestDto.Organization,
                ApplicationLanguage = defaultUserApplicationLanguage
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

        private async Task DeleteFromDb(int coordinatorId, bool allowHeadCoordinatorDeletion = false)
        {
            var coordinator = await _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<CoordinatorUser>(coordinatorId);

            await HandleHeadCoordinatorStatus(coordinator, allowHeadCoordinatorDeletion);

            _nationalSocietyUserService.DeleteNationalSocietyUser(coordinator);
            await _identityUserRegistrationService.DeleteIdentityUser(coordinator.IdentityUserId);
        }

        private async Task HandleHeadCoordinatorStatus(CoordinatorUser coordinator, bool allowHeadCoordinatorDeletion)
        {
            var nationalSociety = await _dataContext.NationalSocieties.FindAsync(coordinator.UserNationalSocieties.Single().NationalSocietyId);
            if (nationalSociety.PendingHeadCoordinator == coordinator)
            {
                nationalSociety.PendingHeadCoordinator = null;
            }

            if (nationalSociety.HeadCoordinator == coordinator)
            {
                if (!allowHeadCoordinatorDeletion)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadCoordinator);
                }

                nationalSociety.HeadCoordinator = null;
            }
        }
    }
}
