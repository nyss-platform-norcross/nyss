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
        Task<Result> SetPendingHeadManager(int nationalSocietyId, int userId);
        Task<Result> SetAsHeadManager();
        Task<Result<List<PendingHeadManagerConsentDto>>> GetPendingHeadManagerConsents();
        Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int nationalSocietyId, bool excludeActivity);
        Task<Result> Reopen(int nationalSocietyId);
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
        private readonly INyssBlobProvider _nyssBlobProvider;

        public NationalSocietyService(
            INyssContext context,
            INationalSocietyAccessService nationalSocietyAccessService,
            ILoggerAdapter loggerAdapter, IAuthorizationService authorizationService,
            IManagerService managerService, ITechnicalAdvisorService technicalAdvisorService,
            ISmsGatewayService smsGatewayService, INyssBlobProvider nyssBlobProvider)
        {
            _nyssContext = context;
            _nationalSocietyAccessService = nationalSocietyAccessService;
            _loggerAdapter = loggerAdapter;
            _authorizationService = authorizationService;
            _managerService = managerService;
            _technicalAdvisorService = technicalAdvisorService;
            _smsGatewayService = smsGatewayService;
            _nyssBlobProvider = nyssBlobProvider;
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> List()
        {
            var nationalSocietiesQuery = await GetNationalSocietiesQuery();

            var list = await nationalSocietiesQuery
                .Include(x => x.HeadManager)
                .Include(x => x.PendingHeadManager)
                .Select(n => new NationalSocietyListResponseDto
                {
                    Id = n.Id,
                    ContentLanguage = n.ContentLanguage.DisplayName,
                    Name = n.Name,
                    Country = n.Country.Name,
                    StartDate = n.StartDate,
                    HeadManagerName = n.HeadManager.Name,
                    PendingHeadManagerName = n.PendingHeadManager.Name,
                    TechnicalAdvisor = string.Join(", ", n.NationalSocietyUsers.AsQueryable()
                        .Where(UserNationalSocietyQueries.IsNotDeletedUser)
                        .Where(u => u.User.Role == Role.TechnicalAdvisor)
                        .Select(u => u.User.Name)
                        .ToList()),
                    IsArchived = n.IsArchived
                })
                .OrderBy(n => n.Name)
                .ToListAsync();

            return Success(list);
        }

        public async Task<Result<NationalSocietyResponseDto>> Get(int id)
        {
            var nationalSociety = await _nyssContext.NationalSocieties
                .Select(n => new NationalSocietyResponseDto
                {
                    Id = n.Id,
                    ContentLanguageId = n.ContentLanguage.Id,
                    ContentLanguageName = n.ContentLanguage.DisplayName,
                    Name = n.Name,
                    CountryId = n.Country.Id,
                    CountryName = n.Country.Name,
                    HeadManagerId = n.HeadManager.Id,
                    IsArchived = n.IsArchived
                })
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nationalSociety == null)
            {
                return Error<NationalSocietyResponseDto>(ResultKey.NationalSociety.NotFound);
            }

            return Success(nationalSociety);
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
            _loggerAdapter.Info($"A national society {nationalSociety} was created");
            return Success(nationalSociety.Id);
        }

        public async Task<Result> Edit(int nationalSocietyId, EditNationalSocietyRequestDto dto)
        {
            if (_nyssContext.NationalSocieties.Any(ns => ns.Id != nationalSocietyId && ns.Name.ToLower() == dto.Name.ToLower()))
            {
                return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
            }

            var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyId);

            if (nationalSociety.IsArchived)
            {
                return Error(ResultKey.NationalSociety.Edit.CannotEditArchivedNationalSociety);
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

        public async Task<Result> SetPendingHeadManager(int nationalSocietyId, int userId)
        {
            var userNationalSocieties = await _nyssContext.UserNationalSocieties.FilterAvailableUsers()
                .Where(uns => uns.NationalSocietyId == nationalSocietyId).ToListAsync();
            if (userNationalSocieties.Count == 0 || userNationalSocieties.All(x => x.UserId != userId))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotAMemberOfSociety);
            }

            var user = await _nyssContext.Users.FilterAvailable().Include(u => u.UserNationalSocieties).FirstOrDefaultAsync(u => u.Id == userId);
            if (!(user is ManagerUser || user is TechnicalAdvisorUser))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotApplicableUserRole);
            }

            var ns = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyId);
            ns.PendingHeadManager = user;
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result> SetAsHeadManager()
        {
            var identityUserName = _authorizationService.GetCurrentUserName();

            var user = await _nyssContext.Users.FilterAvailable()
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (user == null)
            {
                return Error(ResultKey.User.Common.UserNotFound);
            }

            if (!(user is ManagerUser || user is TechnicalAdvisorUser))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotApplicableUserRole);
            }

            var pendingSocieties = _nyssContext.NationalSocieties.Where(x => x.PendingHeadManager.Id == user.Id);
            var utcNow = DateTime.UtcNow;

            // Set until date for the previous consent
            await _nyssContext.HeadManagerConsents
                .Where(x => pendingSocieties.Select(y => y.Id).Contains(x.NationalSocietyId) && x.ConsentedUntil == null)
                .ForEachAsync(x => x.ConsentedUntil = utcNow);

            foreach (var nationalSociety in pendingSocieties)
            {
                nationalSociety.PendingHeadManager = null;
                nationalSociety.HeadManager = user;

                await _nyssContext.HeadManagerConsents.AddAsync(new HeadManagerConsent
                {
                    ConsentedFrom = utcNow,
                    NationalSocietyId = nationalSociety.Id,
                    UserEmailAddress = user.EmailAddress,
                    UserPhoneNumber = user.PhoneNumber
                });
            }

            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result<List<PendingHeadManagerConsentDto>>> GetPendingHeadManagerConsents()
        {
            var identityUserName = _authorizationService.GetCurrentUserName();

            var userEntity = await _nyssContext.Users.FilterAvailable()
                .Include(x => x.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (userEntity == null)
            {
                return Error<List<PendingHeadManagerConsentDto>>(ResultKey.User.Common.UserNotFound);
            }
            var blobUrl = _nyssBlobProvider.GetAgreementPdf("en");
            var pendingSocieties = await _nyssContext.NationalSocieties
                .Where(ns => ns.PendingHeadManager.IdentityUserId == userEntity.IdentityUserId)
                .Select(ns => new PendingHeadManagerConsentDto
                {
                    NationalSocietyName = ns.Name,
                    NationalSocietyCountryName = ns.Country.Name,
                    NationalSocietyId = ns.Id,
                    AgreementPdf = blobUrl
                })
                .ToListAsync();

            return Success(pendingSocieties);
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
                        .Any(uns => uns.UserId != ns.HeadManager.Id),
                    HasOpenedProjects = openedProjectsQuery.Any(p => p.NationalSocietyId == ns.Id),
                    HeadManagerId = ns.HeadManager != null
                        ? (int?)ns.HeadManager.Id
                        : null,
                    HeadManagerRole = ns.HeadManager != null
                        ? (Role?)ns.HeadManager.Role
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

        public async Task<ContentLanguage> GetLanguageById(int id) =>
            await _nyssContext.ContentLanguages.FindAsync(id);

        public async Task<Country> GetCountryById(int id) =>
            await _nyssContext.Countries.FindAsync(id);

        private async Task<IQueryable<NationalSociety>> GetNationalSocietiesQuery()
        {
            if (_nationalSocietyAccessService.HasCurrentUserAccessToAllNationalSocieties())
            {
                return _nyssContext.NationalSocieties;
            }

            var availableNationalSocieties = await _nationalSocietyAccessService.GetCurrentUserNationalSocietyIds();
            return _nyssContext.NationalSocieties.Where(ns => availableNationalSocieties.Contains(ns.Id));
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
