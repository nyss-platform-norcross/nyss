using System;
using System.Collections.Generic;
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
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.TechnicalAdvisors.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.TechnicalAdvisors
{
    public interface ITechnicalAdvisorService
    {
        Task<Result> Create(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto);
        Task<Result<GetTechnicalAdvisorResponseDto>> Get(int technicalAdvisorId, int nationalSocietyId);
        Task<Result> Edit(int technicalAdvisorId, EditTechnicalAdvisorRequestDto editDto);
        Task<Result> Delete(int nationalSocietyId, int technicalAdvisorId);
        Task DeleteIncludingHeadManagerFlag(int nationalSocietyId, int technicalAdvisorId);
    }

    public class TechnicalAdvisorService : ITechnicalAdvisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationService _organizationService;

        public TechnicalAdvisorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext nyssContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService, IAuthorizationService authorizationService, IOrganizationService organizationService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _nationalSocietyUserService = nationalSocietyUserService;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
            _authorizationService = authorizationService;
            _organizationService = organizationService;
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

        public async Task<Result<GetTechnicalAdvisorResponseDto>> Get(int nationalSocietyUserId, int nationalSocietyId)
        {
            var technicalAdvisor = await _nyssContext.UserNationalSocieties
                .FilterAvailable()
                .Where(u => u.User.Role == Role.TechnicalAdvisor)
                .Where(u => u.UserId == nationalSocietyUserId && u.NationalSocietyId == nationalSocietyId)
                .Select(u => new GetTechnicalAdvisorResponseDto
                {
                    Id = u.User.Id,
                    Name = u.User.Name,
                    Role = u.User.Role,
                    Email = u.User.EmailAddress,
                    OrganizationId = u.Organization.Id,
                    PhoneNumber = u.User.PhoneNumber,
                    AdditionalPhoneNumber = u.User.AdditionalPhoneNumber,
                    Organization = u.User.Organization,
                    ModemId = ((TechnicalAdvisorUser)u.User).TechnicalAdvisorUserGatewayModems
                        .FirstOrDefault(x => x.GatewayModem.GatewaySetting.NationalSocietyId == nationalSocietyId).GatewayModemId
                })
                .SingleOrDefaultAsync();

            if (technicalAdvisor == null)
            {
                _loggerAdapter.Debug($"Technical advisor with id {nationalSocietyUserId} was not found");
                return Error<GetTechnicalAdvisorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetTechnicalAdvisorResponseDto>(technicalAdvisor, true);
        }

        public async Task<Result> Edit(int technicalAdvisorId, EditTechnicalAdvisorRequestDto editDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<TechnicalAdvisorUser>(technicalAdvisorId);

                user.Name = editDto.Name;
                user.PhoneNumber = editDto.PhoneNumber;
                user.Organization = editDto.Organization;
                user.AdditionalPhoneNumber = editDto.AdditionalPhoneNumber;

                if (editDto.OrganizationId.HasValue)
                {
                    var userLink = await _nyssContext.UserNationalSocieties
                        .Where(un => un.UserId == technicalAdvisorId && un.NationalSociety.Id == editDto.NationalSocietyId)
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

        public async Task DeleteIncludingHeadManagerFlag(int nationalSocietyId, int technicalAdvisorId) =>
            await DeleteFromNationalSociety(nationalSocietyId, technicalAdvisorId, true);

        private async Task<TechnicalAdvisorUser> CreateTechnicalAdvisorUser(IdentityUser identityUser, int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto)
        {
            var nationalSociety = await _nyssContext.NationalSocieties.Include(ns => ns.ContentLanguage)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            if (nationalSociety.IsArchived)
            {
                throw new ResultException(ResultKey.User.Registration.CannotCreateUsersInArchivedNationalSociety);
            }

            var defaultUserApplicationLanguage = await _nyssContext.ApplicationLanguages
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

            if (createTechnicalAdvisorRequestDto.OrganizationId.HasValue)
            {
                userNationalSociety.Organization = await _nyssContext.Organizations
                    .Where(o => o.Id == createTechnicalAdvisorRequestDto.OrganizationId.Value && o.NationalSocietyId == nationalSocietyId)
                    .SingleAsync();
            }
            else
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                userNationalSociety.Organization = await _nyssContext.UserNationalSocieties
                        .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                        .Select(uns => uns.Organization)
                        .SingleOrDefaultAsync() ??
                    await _nyssContext.Organizations.Where(o => o.NationalSocietyId == nationalSocietyId).FirstOrDefaultAsync();
            }

            await AttachTechnicalAdvisorToModem(user, createTechnicalAdvisorRequestDto.ModemId, nationalSocietyId);

            await _nyssContext.AddAsync(userNationalSociety);
            await _nyssContext.SaveChangesAsync();
            return user;
        }

        private async Task AttachTechnicalAdvisorToModem(TechnicalAdvisorUser user, int? modemId, int nationalSocietyId)
        {
            if (modemId.HasValue)
            {
                var modem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId);
                if (modem == null)
                {
                    throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
                }

                var technicalAdvisorModem = new TechnicalAdvisorUserGatewayModem
                {
                    TechnicalAdvisorUser = user,
                    GatewayModem = modem
                };

                user.TechnicalAdvisorUserGatewayModems = new List<TechnicalAdvisorUserGatewayModem> { technicalAdvisorModem };
            }
        }

        private async Task UpdateModem(TechnicalAdvisorUser user, int? modemId, int nationalSocietyId)
        {
            var modem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId);

            if (modemId.HasValue && modem == null)
            {
                throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
            }

            var technicalAdvisorModem = new TechnicalAdvisorUserGatewayModem
            {
                TechnicalAdvisorUser = user,
                GatewayModem = modem
            };

            if (user.TechnicalAdvisorUserGatewayModems == null)
            {
                user.TechnicalAdvisorUserGatewayModems = new List<TechnicalAdvisorUserGatewayModem> { technicalAdvisorModem };
            }
            else
            {
                user.TechnicalAdvisorUserGatewayModems.Add(technicalAdvisorModem);
            }
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

            _nyssContext.UserNationalSocieties.Remove(nationalSocietyReferenceToRemove);

            await HandleHeadManagerStatus(technicalAdvisor, nationalSocietyReferenceToRemove, allowHeadManagerDeletion);

            var isUsersLastNationalSociety = userNationalSocieties.Count == 1;
            if (isUsersLastNationalSociety)
            {
                await _identityUserRegistrationService.DeleteIdentityUser(technicalAdvisor.IdentityUserId);
                await AnonymizeTechnicalAdvisorWithAlertReferences(technicalAdvisorId);
            }
        }

        private async Task HandleHeadManagerStatus(TechnicalAdvisorUser technicalAdvisor, UserNationalSociety nationalSocietyReferenceToRemove, bool allowHeadManagerDeletion)
        {
            var organization = await _nyssContext.Organizations
                .Include(o => o.PendingHeadManager)
                .Include(o => o.HeadManager)
                .Where(o => o.NationalSocietyId == nationalSocietyReferenceToRemove.NationalSocietyId && o.Id == nationalSocietyReferenceToRemove.OrganizationId)
                .SingleOrDefaultAsync();

            if (organization == null)
            {
                return;
            }

            if (organization.PendingHeadManager == technicalAdvisor)
            {
                organization.PendingHeadManager = null;
            }

            if (organization.HeadManager == technicalAdvisor)
            {
                if (!allowHeadManagerDeletion)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadManager);
                }

                organization.HeadManager = null;
            }
        }

        private async Task AnonymizeTechnicalAdvisorWithAlertReferences(int technicalAdvisorId)
        {
            var technicalAdvisorUser = await _nyssContext.Users
                .Where(u => u.Id == technicalAdvisorId)
                .SingleOrDefaultAsync();

            if (technicalAdvisorUser == null)
            {
                throw new ResultException(ResultKey.User.Deletion.UserNotFound);
            }

            technicalAdvisorUser.IdentityUserId = null;
            technicalAdvisorUser.EmailAddress = Anonymization.Text;
            technicalAdvisorUser.PhoneNumber = Anonymization.Text;
            technicalAdvisorUser.AdditionalPhoneNumber = Anonymization.Text;
            technicalAdvisorUser.Name = Anonymization.Text;
            technicalAdvisorUser.DeletedAt = DateTime.UtcNow;
        }
    }
}
