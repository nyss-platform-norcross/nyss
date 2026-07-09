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
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Coordinators
{
    public interface ICoordinatorService
    {
        Task<Result> Create(int nationalSocietyId, CreateCoordinatorRequestDto createCoordinatorRequestDto);
        Task<Result<GetCoordinatorResponseDto>> Get(int coordinatorId);
        Task<Result> Edit(int coordinatorId, CreateCoordinatorRequestDto editCoordinatorRequestDto);
        Task<Result> Delete(int coordinatorId);
    }


    public class CoordinatorService : ICoordinatorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;
        private readonly IAuthorizationService _authorizationService;

        public CoordinatorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService, IAuthorizationService authorizationService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
            _authorizationService = authorizationService;
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
            var coordinator = await _dataContext.UserNationalSocieties
                .FilterAvailable()
                .Where(u => u.User.Role == Role.Coordinator)
                .Where(u => u.UserId == nationalSocietyUserId)
                .Select(u => new GetCoordinatorResponseDto
                {
                    Id = u.User.Id,
                    Name = u.User.Name,
                    Role = u.User.Role,
                    Email = u.User.EmailAddress,
                    PhoneNumber = u.User.PhoneNumber,
                    OrganizationId = u.Organization.Id,
                    AdditionalPhoneNumber = u.User.AdditionalPhoneNumber,
                    Organization = u.User.Organization
                })
                .SingleOrDefaultAsync();

            if (coordinator == null)
            {
                _loggerAdapter.Debug($"Data coordinator with id {nationalSocietyUserId} was not found");
                return Error<GetCoordinatorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetCoordinatorResponseDto>(coordinator, true);
        }

        public async Task<Result> Edit(int coordinatorId, CreateCoordinatorRequestDto editCoordinatorRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<CoordinatorUser>(coordinatorId);
                var oldEmail = user.EmailAddress;
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                else
                {
                    user.Name = editCoordinatorRequestDto.Name;
                    user.EmailAddress = editCoordinatorRequestDto.Email;
                    user.PhoneNumber = editCoordinatorRequestDto.PhoneNumber;
                    user.Organization = editCoordinatorRequestDto.Organization;
                    user.AdditionalPhoneNumber = editCoordinatorRequestDto.AdditionalPhoneNumber;
                    user.IsFirstLogin = oldEmail != editCoordinatorRequestDto.Email ? true : false;
                    var organization = await _dataContext.Organizations.FindAsync(editCoordinatorRequestDto.OrganizationId);

                    var userLink = await _dataContext.UserNationalSocieties
                        .Where(un => un.UserId == coordinatorId && un.NationalSociety.Id == organization.NationalSocietyId)
                        .SingleOrDefaultAsync();

                    userLink.Organization = await _dataContext.Organizations.FindAsync(editCoordinatorRequestDto.OrganizationId);
                    await _dataContext.SaveChangesAsync();

                }

                if (oldEmail != editCoordinatorRequestDto.Email)
                {
                    var identityUser = await _identityUserRegistrationService.EditIdentityUser(oldEmail, editCoordinatorRequestDto.Email);
                    string securityStamp;
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);
                    await _verificationEmailService.SendVerificationEmail(user, securityStamp);
                }

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

        private async Task<CoordinatorUser> CreateCoordinatorUser(IdentityUser identityUser, int nationalSocietyId, CreateCoordinatorRequestDto createDto)
        {
            if (nationalSocietyId != createDto.NationalSocietyId)
            {
                throw new ResultException(ResultKey.UnexpectedError);
            }

            var nationalSociety = await _dataContext.NationalSocieties
                .Include(ns => ns.NationalSocietyUsers)
                .ThenInclude(nsu => nsu.User)
                .Include(ns => ns.ContentLanguage)
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
                Name = createDto.Name,
                PhoneNumber = createDto.PhoneNumber,
                AdditionalPhoneNumber = createDto.AdditionalPhoneNumber,
                Organization = createDto.Organization,
                ApplicationLanguage = defaultUserApplicationLanguage
            };

            var userNationalSociety = new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

            if (createDto.OrganizationId.HasValue)
            {
                userNationalSociety.Organization = await _dataContext.Organizations
                    .Where(o => o.Id == createDto.OrganizationId.Value && o.NationalSocietyId == nationalSocietyId)
                    .SingleAsync();
            }
            else
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                userNationalSociety.Organization = await _dataContext.UserNationalSocieties
                        .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                        .Select(uns => uns.Organization)
                        .SingleOrDefaultAsync() ??
                    await _dataContext.Organizations.Where(o => o.NationalSocietyId == nationalSocietyId).FirstOrDefaultAsync();
            }


            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            return user;
        }

        private async Task DeleteFromDb(int coordinatorId)
        {
            var coordinator = await _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<CoordinatorUser>(coordinatorId);
            var usersInNs = await _dataContext.UserNationalSocieties
                .Where(uns => uns.NationalSocietyId == coordinator.UserNationalSocieties.Single().NationalSocietyId)
                .CountAsync();

            if (usersInNs > 1 && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
            {
                throw new ResultException(ResultKey.User.Deletion.MoreUsersExists);
            }

            _nationalSocietyUserService.DeleteNationalSocietyUser(coordinator);
            await _identityUserRegistrationService.DeleteIdentityUser(coordinator.IdentityUserId);
        }
    }
}
