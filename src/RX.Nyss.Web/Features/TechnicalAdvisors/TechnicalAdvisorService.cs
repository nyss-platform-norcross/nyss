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
using RX.Nyss.Web.Features.TechnicalAdvisors.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.TechnicalAdvisors
{
    public interface ITechnicalAdvisorService
    {
        Task<Result> Create(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto);
        Task<Result<GetTechnicalAdvisorResponseDto>> Get(int technicalAdvisorId);
        Task<Result> Edit(int technicalAdvisorId, EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto);
        Task<Result> Delete(int nationalSocietyId, int technicalAdvisorId);
        Task DeleteIncludingHeadManagerFlag(int nationalSocietyId, int technicalAdvisorId);
    }

    public class TechnicalAdvisorService : ITechnicalAdvisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;

        public TechnicalAdvisorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _nationalSocietyUserService = nationalSocietyUserService;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
        }

        public async Task<Result> Create(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto)
        {
            try
            {
                string securityStamp;
                TechnicalAdvisorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createTechnicalAdvisorRequestDto.Email, Role.TechnicalAdvisor);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);
                    user = await CreateTechnicalAdvisorUser(identityUser, nationalSocietyId, createTechnicalAdvisorRequestDto);

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

        public async Task<Result<GetTechnicalAdvisorResponseDto>> Get(int nationalSocietyUserId)
        {
            var technicalAdvisor = await _dataContext.Users.FilterAvailable()
                .OfType<TechnicalAdvisorUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetTechnicalAdvisorResponseDto
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

            if (technicalAdvisor == null)
            {
                _loggerAdapter.Debug($"Technical advisor with id {nationalSocietyUserId} was not found");
                return Error<GetTechnicalAdvisorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetTechnicalAdvisorResponseDto>(technicalAdvisor, true);
        }

        public async Task<Result> Edit(int technicalAdvisorId, EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<TechnicalAdvisorUser>(technicalAdvisorId);

                user.Name = editTechnicalAdvisorRequestDto.Name;
                user.PhoneNumber = editTechnicalAdvisorRequestDto.PhoneNumber;
                user.Organization = editTechnicalAdvisorRequestDto.Organization;
                user.AdditionalPhoneNumber = editTechnicalAdvisorRequestDto.AdditionalPhoneNumber;

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Delete(int nationalSocietyId, int technicalAdvisorId)
        {
            try
            {
                await _deleteUserService.EnsureCanDeleteUser(technicalAdvisorId, Role.TechnicalAdvisor);

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await DeleteFromNationalSociety(nationalSocietyId, technicalAdvisorId);

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

        public async Task DeleteIncludingHeadManagerFlag(int nationalSocietyId, int technicalAdvisorId) =>
            await DeleteFromNationalSociety(nationalSocietyId, technicalAdvisorId, true);

        private async Task<TechnicalAdvisorUser> CreateTechnicalAdvisorUser(IdentityUser identityUser, int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto)
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

            var user = new TechnicalAdvisorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createTechnicalAdvisorRequestDto.Name,
                PhoneNumber = createTechnicalAdvisorRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createTechnicalAdvisorRequestDto.AdditionalPhoneNumber,
                Organization = createTechnicalAdvisorRequestDto.Organization,
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

        private async Task DeleteFromNationalSociety(int nationalSocietyId, int technicalAdvisorId, bool allowHeadManagerDeletion = false)
        {
            var technicalAdvisor = await _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<TechnicalAdvisorUser>(technicalAdvisorId);

            var userNationalSocieties = technicalAdvisor.UserNationalSocieties;

            var nationalSocietyReferenceToRemove = userNationalSocieties.SingleOrDefault(uns => uns.NationalSocietyId == nationalSocietyId);
            if (nationalSocietyReferenceToRemove == null)
            {
                throw new ResultException(ResultKey.User.Registration.UserIsNotAssignedToThisNationalSociety);
            }

            _dataContext.UserNationalSocieties.Remove(nationalSocietyReferenceToRemove);

            await HandleHeadManagerStatus(technicalAdvisor, nationalSocietyReferenceToRemove, allowHeadManagerDeletion);

            var isUsersLastNationalSociety = userNationalSocieties.Count == 1;
            if (isUsersLastNationalSociety)
            {
                _nationalSocietyUserService.DeleteNationalSocietyUser(technicalAdvisor);
                await _identityUserRegistrationService.DeleteIdentityUser(technicalAdvisor.IdentityUserId);
            }
        }

        private async Task HandleHeadManagerStatus(TechnicalAdvisorUser technicalAdvisor, UserNationalSociety nationalSocietyReferenceToRemove, bool allowHeadManagerDeletion)
        {
            var nationalSociety = await _dataContext.NationalSocieties.FindAsync(nationalSocietyReferenceToRemove.NationalSocietyId);
            if (nationalSociety.PendingHeadManager == technicalAdvisor)
            {
                nationalSociety.PendingHeadManager = null;
            }

            if (nationalSociety.HeadManager == technicalAdvisor)
            {
                if (!allowHeadManagerDeletion)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadManager);
                }

                nationalSociety.HeadManager = null;
            }
        }
    }
}
