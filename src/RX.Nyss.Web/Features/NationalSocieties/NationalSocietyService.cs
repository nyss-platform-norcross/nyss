using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocieties
{
    public interface INationalSocietyService
    {
        Task<Result<List<NationalSocietyListResponseDto>>> List();

        Task<Result> Create(CreateNationalSocietyRequestDto nationalSociety);

        Task<Result> Edit(int nationalSocietyId, EditNationalSocietyRequestDto nationalSociety);

        Task<IEnumerable<HealthRiskDto>> GetHealthRiskNames(int nationalSocietyId, bool excludeActivity);

        Task<Result> Reopen(int nationalSocietyId);
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;

        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        private readonly ILoggerAdapter _loggerAdapter;

        private readonly IAuthorizationService _authorizationService;

        public NationalSocietyService(
            INyssContext context,
            INationalSocietyAccessService nationalSocietyAccessService,
            ILoggerAdapter loggerAdapter,
            IAuthorizationService authorizationService)
        {
            _nyssContext = context;
            _nationalSocietyAccessService = nationalSocietyAccessService;
            _loggerAdapter = loggerAdapter;
            _authorizationService = authorizationService;
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
                    HeadManagers = string.Join(", ", n.Organizations
                        .Where(o => o.HeadManager != null)
                        .Select(o => o.HeadManager.Name)
                        .ToList()),
                    TechnicalAdvisor = string.Join(", ", n.NationalSocietyUsers
                        .Where(u => u.User.Role == Role.TechnicalAdvisor)
                        .Select(u => u.User.Name)
                        .ToList()),
                    Coordinators = string.Join(", ", n.NationalSocietyUsers
                        .Where(nsu => nsu.User.Role == Role.Coordinator)
                        .Select(nsu => nsu.User.Name)
                        .ToList()),
                    IsArchived = n.IsArchived
                })
                .OrderBy(n => n.Name)
                .ToListAsync();

            return Success(list);
        }

        public async Task<Result> Create(CreateNationalSocietyRequestDto dto)
        {
            var nationalSociety = new NationalSociety
            {
                Name = dto.Name,
                ContentLanguage = await GetLanguageById(dto.ContentLanguageId),
                Country = await GetCountryById(dto.CountryId),
                IsArchived = false,
                StartDate = DateTime.UtcNow
            };

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
            var currentUser = await _authorizationService.GetCurrentUser();

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
    }
}
