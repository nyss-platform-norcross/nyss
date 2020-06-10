using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.TechnicalAdvisors;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocieties
{
    public interface INationalSocietyService
    {
        Task<Result<List<NationalSocietyListResponseDto>>> List();
        Task<Result<NationalSocietyResponseDto>> Get(int id);
        Task<Result> Create(CreateNationalSocietyRequestDto nationalSociety);
        Task<Result> Edit(int nationalSocietyId, EditNationalSocietyRequestDto nationalSociety);
        Task<Result> Delete(int id);
        Task<Result> Archive(int nationalSocietyId);
        Task<Result<PendingConsentDto>> GetPendingNationalSocietyConsents();
        Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int nationalSocietyId, bool excludeActivity);
        Task<Result> Reopen(int nationalSocietyId);
        Task<Result> ConsentToNationalSocietyAgreement(string languageCode);
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IAuthorizationService _authorizationService;
        private readonly IManagerService _managerService;
        private readonly ITechnicalAdvisorService _technicalAdvisorService;
        private readonly ISmsGatewayService _smsGatewayService;
        private readonly IGeneralBlobProvider _generalBlobProvider;
        private readonly IOrganizationService _organizationService;
        private readonly IDataBlobService _dataBlobService;

        public NationalSocietyService(
            INyssContext context,
            INationalSocietyAccessService nationalSocietyAccessService,
            ILoggerAdapter loggerAdapter, IAuthorizationService authorizationService,
            IManagerService managerService, ITechnicalAdvisorService technicalAdvisorService,
            ISmsGatewayService smsGatewayService, IGeneralBlobProvider generalBlobProvider,
            IOrganizationService organizationService, IDataBlobService dataBlobService)
        {
            _nyssContext = context;
            _nationalSocietyAccessService = nationalSocietyAccessService;
            _loggerAdapter = loggerAdapter;
            _authorizationService = authorizationService;
            _managerService = managerService;
            _technicalAdvisorService = technicalAdvisorService;
            _smsGatewayService = smsGatewayService;
            _generalBlobProvider = generalBlobProvider;
            _organizationService = organizationService;
            _dataBlobService = dataBlobService;
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> List()
        {
            var list = await GetNationalSocietiesQuery()
                .Include(x => x.DefaultOrganization.HeadManager)
                .Include(x => x.DefaultOrganization.PendingHeadManager)
                .Select(n => new NationalSocietyListResponseDto
                {
                    Id = n.Id,
                    ContentLanguage = n.ContentLanguage.DisplayName,
                    Name = n.Name,
                    Country = n.Country.Name,
                    StartDate = n.StartDate,
                    HeadManagers = string.Join(", ", n.Organizations.AsQueryable()
                        .Where(o => o.HeadManager != null)
                        .Select(o => o.HeadManager.Name)
                        .ToList()),
                    TechnicalAdvisor = string.Join(", ", n.NationalSocietyUsers.AsQueryable()
                        .Where(UserNationalSocietyQueries.IsNotDeletedUser)
                        .Where(u => u.User.Role == Role.TechnicalAdvisor)
                        .Select(u => u.User.Name)
                        .ToList()),
                    Coordinators = string.Join(", ", n.NationalSocietyUsers.AsQueryable()
                        .Where(UserNationalSocietyQueries.IsNotDeletedUser)
                        .Where(nsu => nsu.User.Role == Role.Coordinator)
                        .Select(nsu => nsu.User.Name)
                        .ToList()),
                    IsArchived = n.IsArchived
                })
                .OrderBy(n => n.Name)
                .ToListAsync();

            return Success(list);
        }

        public async Task<Result<NationalSocietyResponseDto>> Get(int id)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();

            var nationalSociety = await _nyssContext.NationalSocieties
                .Select(n => new NationalSocietyResponseDto
                {
                    Id = n.Id,
                    ContentLanguageId = n.ContentLanguage.Id,
                    ContentLanguageName = n.ContentLanguage.DisplayName,
                    Name = n.Name,
                    CountryId = n.Country.Id,
                    CountryName = n.Country.Name,
                    IsCurrentUserHeadManager = n.Organizations.Any(o => o.HeadManager.EmailAddress == currentUserName),
                    IsArchived = n.IsArchived,
                    HasCoordinator = n.NationalSocietyUsers.Any(nsu => nsu.User.Role == Role.Coordinator)
                })
                .FirstOrDefaultAsync(n => n.Id == id);

            return nationalSociety != null
                ? Success(nationalSociety)
                : Error<NationalSocietyResponseDto>(ResultKey.NationalSociety.NotFound);
        }

        public async Task<Result> Create(CreateNationalSocietyRequestDto dto)
        {
            if (_nyssContext.NationalSocieties.Any(ns => ns.Name.ToLower() == dto.Name.ToLower()))
            {
                return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
            }

            var nationalSociety = new NationalSociety
            {
                Name = dto.Name,
                ContentLanguage = await GetLanguageById(dto.ContentLanguageId),
                Country = await GetCountryById(dto.CountryId),
                IsArchived = false,
                StartDate = DateTime.UtcNow
            };

            if (nationalSociety.ContentLanguage == null)
            {
                return Error<int>(ResultKey.NationalSociety.Creation.LanguageNotFound);
            }

            if (nationalSociety.Country == null)
            {
                return Error<int>(ResultKey.NationalSociety.Creation.CountryNotFound);
            }

            await _nyssContext.AddAsync(nationalSociety);
            await _nyssContext.SaveChangesAsync();

            nationalSociety.DefaultOrganization = new Organization
            {
                Name = dto.InitialOrganizationName,
                NationalSociety = nationalSociety
            };

            await _nyssContext.SaveChangesAsync();

            _loggerAdapter.Info($"A national society {nationalSociety} was created");
            return Success(nationalSociety.Id);
        }

        public async Task<Result> Edit(int nationalSocietyId, EditNationalSocietyRequestDto dto)
        {
            if (_nyssContext.NationalSocieties.Any(ns => ns.Id != nationalSocietyId && ns.Name.ToLower() == dto.Name.ToLower()))
            {
                return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
            }

            var currentUser = _authorizationService.GetCurrentUser();

            var nationalSocietyData = await _nyssContext.NationalSocieties
                .Where(n => n.Id == nationalSocietyId)
                .Select(ns => new
                {
                    NationalSociety = ns,
                    CurrentUserOrganizationId = ns.NationalSocietyUsers
                        .Where(uns => uns.User == currentUser)
                        .Select(uns => uns.OrganizationId)
                        .SingleOrDefault(),
                    HasCoordinator = ns.NationalSocietyUsers
                        .Any(uns => uns.User.Role == Role.Coordinator)
                })
                .SingleAsync();

            var nationalSociety = nationalSocietyData.NationalSociety;

            if (nationalSociety.IsArchived)
            {
                return Error(ResultKey.NationalSociety.Edit.CannotEditArchivedNationalSociety);
            }

            if (nationalSocietyData.HasCoordinator && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
            {
                return Error(ResultKey.UnexpectedError);
            }

            nationalSociety.Name = dto.Name;
            nationalSociety.ContentLanguage = await GetLanguageById(dto.ContentLanguageId);
            nationalSociety.Country = await GetCountryById(dto.CountryId);

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.NationalSociety.Edit.Success);
        }

        public async Task<Result> Delete(int id)
        {
            var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(id);
            _nyssContext.NationalSocieties.Remove(nationalSociety);
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.NationalSociety.Remove.Success);
        }

        public async Task<Result<PendingConsentDto>> GetPendingNationalSocietyConsents()
        {
            var identityUserName = _authorizationService.GetCurrentUserName();

            var userEntity = await _nyssContext.Users.FilterAvailable()
                .Include(x => x.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (userEntity == null)
            {
                return Error<PendingConsentDto>(ResultKey.User.Common.UserNotFound);
            }

            var pendingSocieties = await _organizationService.GetNationalSocietiesWithPendingAgreementsForUserQuery(userEntity)
                .Select(ns => new PendingNationalSocietyConsentDto
                {
                    NationalSocietyName = ns.Name,
                    NationalSocietyCountryName = ns.Country.Name,
                    NationalSocietyId = ns.Id
                })
                .ToListAsync();

            if (!pendingSocieties.Any())
            {
                return Error<PendingConsentDto>(ResultKey.Consent.NoPendingConsent);
            }

            var applicationLanguages = await _nyssContext.ApplicationLanguages.ToListAsync();
            var docs = applicationLanguages.Select(apl => new AgreementDocument
            {
                Language = apl.DisplayName,
                LanguageCode = apl.LanguageCode,
                AgreementDocumentUrl = _generalBlobProvider.GetPlatformAgreementUrl(apl.LanguageCode.ToLower())
            }).Where(d => d.AgreementDocumentUrl != null);

            var pendingSociety = new PendingConsentDto
            {
                AgreementDocuments = docs,
                NationalSocieties = pendingSocieties
            };

            return Success(pendingSociety);
        }

        public async Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int nationalSocietyId, bool excludeActivity) =>
            await _nyssContext.ProjectHealthRisks
                .Where(ph => ph.Project.NationalSocietyId == nationalSocietyId)
                .Where(ph => !excludeActivity || ph.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                .Select(ph => new HealthRiskDto
                {
                    Id = ph.HealthRiskId,
                    Name = ph.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == ph.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .FirstOrDefault()
                })
                .Distinct()
                .OrderBy(x => x.Name)
                .ToListAsync();

        public async Task<Result> Archive(int nationalSocietyId)
        {
            var openedProjectsQuery = _nyssContext.Projects.Where(p => p.State == ProjectState.Open);
            var nationalSocietyData = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Where(ns => !ns.IsArchived)
                .Select(ns => new
                {
                    NationalSociety = ns,
                    HasRegisteredUsers = ns.NationalSocietyUsers.AsQueryable()
                        .Where(UserNationalSocietyQueries.IsNotDeletedUser)
                        .Any(uns => uns.UserId != ns.DefaultOrganization.HeadManager.Id),
                    HasOpenedProjects = openedProjectsQuery.Any(p => p.NationalSocietyId == ns.Id),
                    HeadManagerId = ns.DefaultOrganization.HeadManager != null
                        ? (int?)ns.DefaultOrganization.HeadManager.Id
                        : null,
                    HeadManagerRole = ns.DefaultOrganization.HeadManager != null
                        ? (Role?)ns.DefaultOrganization.HeadManager.Role
                        : null
                })
                .SingleAsync();

            if (nationalSocietyData.HasOpenedProjects)
            {
                return Error(ResultKey.NationalSociety.Archive.ErrorHasOpenedProjects);
            }

            if (nationalSocietyData.HasRegisteredUsers)
            {
                return Error(ResultKey.NationalSociety.Archive.ErrorHasRegisteredUsers);
            }

            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (nationalSocietyData.HeadManagerId.HasValue)
                {
                    await DeleteHeadManager(nationalSocietyData.NationalSociety.Id, nationalSocietyData.HeadManagerId.Value, nationalSocietyData.HeadManagerRole.Value);
                }

                await RemoveApiKeys(nationalSocietyData.NationalSociety.Id);
                nationalSocietyData.NationalSociety.IsArchived = true;

                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();

                return SuccessMessage(ResultKey.NationalSociety.Archive.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Reopen(int nationalSocietyId)
        {
            var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyId);
            if (nationalSociety == null)
            {
                return Error(ResultKey.NationalSociety.NotFound);
            }

            nationalSociety.IsArchived = false;
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.NationalSociety.Archive.ReopenSuccess);
        }

        public async Task<Result> ConsentToNationalSocietyAgreement(string languageCode)
        {
            var identityUserName = _authorizationService.GetCurrentUserName();

            var user = await _nyssContext.Users.FilterAvailable()
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (user == null)
            {
                return Error(ResultKey.User.Common.UserNotFound);
            }

            var pendingSocieties = await _organizationService.GetNationalSocietiesWithPendingAgreementsForUserQuery(user)
                .Include(x => x.DefaultOrganization).ToListAsync();
            var utcNow = DateTime.UtcNow;

            var consentDocumentFileName = Guid.NewGuid() + ".pdf";
            var sourceUri = _generalBlobProvider.GetPlatformAgreementUrl(languageCode);
            await _dataBlobService.StorePlatformAgreement(sourceUri, consentDocumentFileName);

            foreach (var nationalSociety in pendingSocieties)
            {
                if (user.Role == Role.Manager || user.Role == Role.TechnicalAdvisor)
                {
                    nationalSociety.DefaultOrganization.PendingHeadManager = null;
                    nationalSociety.DefaultOrganization.HeadManager = user;
                }

                await _nyssContext.NationalSocietyConsents.AddAsync(new NationalSocietyConsent
                {
                    ConsentedFrom = utcNow,
                    NationalSocietyId = nationalSociety.Id,
                    UserEmailAddress = user.EmailAddress,
                    UserPhoneNumber = user.PhoneNumber,
                    ConsentDocument = consentDocumentFileName
                });
            }

            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<ContentLanguage> GetLanguageById(int id) =>
            await _nyssContext.ContentLanguages.FindAsync(id);

        public async Task<Country> GetCountryById(int id) =>
            await _nyssContext.Countries.FindAsync(id);

        private IQueryable<NationalSociety> GetNationalSocietiesQuery()
        {
            if (_nationalSocietyAccessService.HasCurrentUserAccessToAllNationalSocieties())
            {
                return _nyssContext.NationalSocieties;
            }

            var userName = _authorizationService.GetCurrentUserName();

            return _nyssContext.NationalSocieties
                .Where(ns => ns.NationalSocietyUsers.Any(u => u.User.EmailAddress == userName));
        }

        private async Task RemoveApiKeys(int nationalSocietyId)
        {
            var gatewaysResult = await _smsGatewayService.List(nationalSocietyId);
            ThrowIfErrorResult(gatewaysResult);

            foreach (var gateway in gatewaysResult.Value)
            {
                var deleteResult = await _smsGatewayService.Delete(gateway.Id);
                ThrowIfErrorResult(deleteResult);
            }
        }

        private void ThrowIfErrorResult(Result result)
        {
            if (!result.IsSuccess)
            {
                throw new ResultException(result.Message.Key, result.Message.Data);
            }
        }

        private async Task DeleteHeadManager(int nationalSocietyId, int headManagerId, Role headManagerRole)
        {
            if (headManagerRole == Role.Manager)
            {
                await _managerService.DeleteIncludingHeadManagerFlag(headManagerId);
            }
            else if (headManagerRole == Role.TechnicalAdvisor)
            {
                await _technicalAdvisorService.DeleteIncludingHeadManagerFlag(nationalSocietyId, headManagerId);
            }
        }
    }
}
