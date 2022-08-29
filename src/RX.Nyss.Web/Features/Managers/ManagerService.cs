using System;
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
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Managers
{
    public interface IManagerService
    {
        Task<Result> Create(int nationalSocietyId, CreateManagerRequestDto createManagerRequestDto);
        Task<Result<GetManagerResponseDto>> Get(int managerId, int nationalSocietyId);
        Task<Result> Edit(int managerId, EditManagerRequestDto editDto);
        Task<Result> Delete(int managerId);
        Task DeleteIncludingHeadManagerFlag(int managerId);
    }


    public class ManagerService : IManagerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationService _organizationService;

        public ManagerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext nyssContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService, IAuthorizationService authorizationService, IOrganizationService organizationService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
            _authorizationService = authorizationService;
            _organizationService = organizationService;
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

        public async Task<Result<GetManagerResponseDto>> Get(int nationalSocietyUserId, int nationalSocietyId)
        {
            var manager = await _nyssContext.UserNationalSocieties
                .FilterAvailable()
                .Where(u => u.User.Role == Role.Manager)
                .Where(u => u.UserId == nationalSocietyUserId && u.NationalSocietyId == nationalSocietyId)
                .Select(u => new GetManagerResponseDto
                {
                    Id = u.User.Id,
                    Name = u.User.Name,
                    Role = u.User.Role,
                    Email = u.User.EmailAddress,
                    OrganizationId = u.Organization.Id,
                    PhoneNumber = u.User.PhoneNumber,
                    AdditionalPhoneNumber = u.User.AdditionalPhoneNumber,
                    Organization = u.User.Organization,
                    ModemId = ((ManagerUser)u.User).ModemId
                })
                .SingleOrDefaultAsync();

            if (manager == null)
            {
                _loggerAdapter.Debug($"Data manager with id {nationalSocietyUserId} was not found");
                return Error<GetManagerResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetManagerResponseDto>(manager, true);
        }

        public async Task<Result> Edit(int managerId, EditManagerRequestDto editDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<ManagerUser>(managerId);
                var oldEmail = user.EmailAddress;
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                else
                {
                    user.Name = editDto.Name;
                    user.EmailAddress = editDto.Email;
                    user.PhoneNumber = editDto.PhoneNumber;
                    user.Organization = editDto.Organization;
                    user.AdditionalPhoneNumber = editDto.AdditionalPhoneNumber;
                    user.IsFirstLogin = oldEmail != editDto.Email ? true : false;
                    var organization = await _nyssContext.Organizations.FindAsync(editDto.OrganizationId);

                    if (editDto.OrganizationId.HasValue)
                    {
                        var userLink = await _nyssContext.UserNationalSocieties
                            .Where(un => un.UserId == managerId && un.NationalSociety.Id == editDto.NationalSocietyId)
                            .SingleOrDefaultAsync();

                        if (editDto.OrganizationId.Value != userLink.OrganizationId)
                        {
                            var validationResult = await _organizationService.CheckAccessForOrganizationEdition(userLink);

                            if (!validationResult.IsSuccess)
                            {
                                return validationResult;
                            }

                            userLink.Organization = await _nyssContext.Organizations.FindAsync(editDto.OrganizationId.Value);
                        }
                    }

                    await UpdateModem(user, editDto.ModemId, editDto.NationalSocietyId);

                    await _nyssContext.SaveChangesAsync();
                }

                if (oldEmail != editDto.Email)
                {
                    var identityUser = await _identityUserRegistrationService.EditIdentityUser(oldEmail, editDto.Email);
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

        public async Task<Result> Delete(int managerId)
        {
            try
            {
                await _deleteUserService.EnsureCanDeleteUser(managerId, Role.Manager);

                var currentUser = await _authorizationService.GetCurrentUser();
                var canDeleteHeadManager = currentUser.Role == Role.Administrator || currentUser.Role == Role.Coordinator;

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                await DeleteFromDb(managerId, canDeleteHeadManager);

                await AnonymizeManagerWithAlertReferences(managerId);

                await _nyssContext.SaveChangesAsync();

                transactionScope.Complete();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task DeleteIncludingHeadManagerFlag(int managerId) =>
            await DeleteFromDb(managerId, true);

        private async Task<ManagerUser> CreateManagerUser(IdentityUser identityUser, int nationalSocietyId, CreateManagerRequestDto createDto)
        {
            var nationalSociety = await _nyssContext.NationalSocieties
                .Include(ns => ns.ContentLanguage)
                .Include(ns => ns.DefaultOrganization)
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns  => new
                {
                    NationalSociety = ns,
                    HasCoordinator = ns.NationalSocietyUsers.Any(nsu => nsu.User.Role == Role.Coordinator)
                })
                .SingleOrDefaultAsync();

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            if (nationalSociety.NationalSociety.IsArchived)
            {
                throw new ResultException(ResultKey.User.Registration.CannotCreateUsersInArchivedNationalSociety);
            }

            var defaultUserApplicationLanguage = await _nyssContext.ApplicationLanguages
                .SingleOrDefaultAsync(al => al.LanguageCode == nationalSociety.NationalSociety.ContentLanguage.LanguageCode);

            var user = new ManagerUser
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
                NationalSociety = nationalSociety.NationalSociety,
                User = user
            };

            if (createDto.OrganizationId.HasValue)
            {
                userNationalSociety.Organization = await _nyssContext.Organizations
                    .Where(o => o.Id == createDto.OrganizationId.Value && o.NationalSocietyId == nationalSocietyId)
                    .SingleAsync();
            }
            else
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                userNationalSociety.Organization = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                    .Select(uns => uns.Organization)
                    .SingleOrDefaultAsync() ?? nationalSociety.NationalSociety.DefaultOrganization;
            }


            if (createDto.SetAsHeadManager == true)
            {
                if (userNationalSociety.Organization.HeadManagerId.HasValue || userNationalSociety.Organization.PendingHeadManagerId.HasValue)
                {
                    throw new ResultException(ResultKey.User.Registration.HeadManagerAlreadyExists);
                }

                if (_authorizationService.IsCurrentUserInRole(Role.GlobalCoordinator) && nationalSociety.HasCoordinator)
                {
                    throw new ResultException(ResultKey.User.Registration.CoordinatorExists);
                }

                if (nationalSociety.NationalSociety.DefaultOrganization == userNationalSociety.Organization)
                {
                    userNationalSociety.Organization.PendingHeadManager = user;
                }
                else
                {
                    userNationalSociety.Organization.HeadManager = user;
                }
            }

            await AttachManagerToModem(user, createDto.ModemId, nationalSocietyId);

            await _nyssContext.AddAsync(userNationalSociety);
            await _nyssContext.SaveChangesAsync();
            return user;
        }

        private async Task AttachManagerToModem(ManagerUser user, int? modemId, int nationalSocietyId)
        {
            if (modemId.HasValue)
            {
                var modem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId);
                if (modem == null)
                {
                    throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
                }

                user.Modem = modem;
            }
        }

        private async Task UpdateModem(ManagerUser user, int? modemId, int nationalSocietyId)
        {
            var modem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId);
            if (modemId.HasValue && modem == null)
            {
                throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
            }

            user.Modem = modem;
        }

        private async Task DeleteFromDb(int managerId, bool allowHeadManagerDeletion = false)
        {
            var manager = await _nationalSocietyUserService.GetNationalSocietyUserIncludingOrganizations<ManagerUser>(managerId);

            await HandleHeadManagerStatus(manager, allowHeadManagerDeletion);

            await _identityUserRegistrationService.DeleteIdentityUser(manager.IdentityUserId);
        }

        private async Task HandleHeadManagerStatus(ManagerUser manager, bool allowHeadManagerDeletion)
        {
            var organization = await _nyssContext.Organizations
                .Include(o => o.PendingHeadManager)
                .Include(o => o.HeadManager)
                .Where(o => o.HeadManager == manager || o.PendingHeadManager == manager)
                .SingleOrDefaultAsync();

            if (organization == null)
            {
                return;
            }

            if (organization.PendingHeadManager == manager)
            {
                organization.PendingHeadManager = null;
            }

            if (organization.HeadManager == manager)
            {
                if (!allowHeadManagerDeletion)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadManager);
                }

                var currentUser = await _authorizationService.GetCurrentUser();
                var organizationHasUsers = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.NationalSocietyId == organization.NationalSocietyId
                        && uns.OrganizationId == organization.Id
                        && uns.UserId != manager.Id && uns.UserId != currentUser.Id)
                    .AnyAsync();

                if (organizationHasUsers)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadManagerWithUsers);
                }

                organization.HeadManager = null;
            }
        }

        private async Task AnonymizeManagerWithAlertReferences(int managerId)
        {
            var managerUser = await _nyssContext.Users
                .Where(u => u.Id == managerId)
                .SingleOrDefaultAsync();

            if (managerUser == null)
            {
                throw new ResultException(ResultKey.User.Deletion.UserNotFound);
            }

            managerUser.IdentityUserId = null;
            managerUser.EmailAddress = Anonymization.Text;
            managerUser.PhoneNumber = Anonymization.Text;
            managerUser.AdditionalPhoneNumber = Anonymization.Text;
            managerUser.Name = Anonymization.Text;
            managerUser.DeletedAt = DateTime.UtcNow;
        }
    }
}
