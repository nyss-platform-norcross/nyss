using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.Geolocation;
using RX.Nyss.Web.Services.ReportsDashboard;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    public interface IProjectDashboardService
    {
        Task<Result<ProjectDashboardFiltersResponseDto>> GetFiltersData(int projectId);
        Task<Result<ProjectDashboardResponseDto>> GetData(int projectId, FiltersRequestDto filtersDto);

        Task<Result<IEnumerable<ReportsSummaryHealthRiskResponseDto>>> GetProjectReportHealthRisks(int projectId, double latitude, double longitude,
            FiltersRequestDto filtersDto);
    }

    public class ProjectDashboardService : IProjectDashboardService
    {
        private readonly IProjectService _projectService;
        private readonly IReportService _reportService;
        private readonly IReportsDashboardMapService _reportsDashboardMapService;
        private readonly IReportsDashboardByFeatureService _reportsDashboardByFeatureService;
        private readonly IReportsDashboardByHealthRiskService _reportsDashboardByHealthRiskService;
        private readonly IReportsDashboardByVillageService _reportsDashboardByVillageService;
        private readonly IReportsDashboardByDataCollectionPointService _reportsDashboardByDataCollectionPointService;
        private readonly IProjectDashboardSummaryService _projectDashboardSummaryService;
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public ProjectDashboardService(
            IProjectService projectService,
            IReportService reportService,
            IReportsDashboardMapService reportsDashboardMapService,
            IReportsDashboardByFeatureService reportsDashboardByFeatureService,
            IReportsDashboardByHealthRiskService reportsDashboardByHealthRiskService,
            IReportsDashboardByVillageService reportsDashboardByVillageService,
            IReportsDashboardByDataCollectionPointService reportsDashboardByDataCollectionPointService,
            IProjectDashboardSummaryService projectDashboardSummaryService,
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _projectService = projectService;
            _reportService = reportService;
            _reportsDashboardMapService = reportsDashboardMapService;
            _reportsDashboardByFeatureService = reportsDashboardByFeatureService;
            _reportsDashboardByHealthRiskService = reportsDashboardByHealthRiskService;
            _reportsDashboardByVillageService = reportsDashboardByVillageService;
            _reportsDashboardByDataCollectionPointService = reportsDashboardByDataCollectionPointService;
            _projectDashboardSummaryService = projectDashboardSummaryService;
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<ProjectDashboardFiltersResponseDto>> GetFiltersData(int projectId)
        {
            var healthRiskTypesWithoutActivity = new List<HealthRiskType>
            {
                HealthRiskType.Human,
                HealthRiskType.NonHuman,
                HealthRiskType.UnusualEvent
            };

            var organizations = await GetOrganizations(projectId);
            var projectHealthRisks = await _projectService.GetHealthRiskNames(projectId, healthRiskTypesWithoutActivity);

            var dto = new ProjectDashboardFiltersResponseDto
            {
                HealthRisks = projectHealthRisks,
                Organizations = organizations
            };

            return Success(dto);
        }

        private async Task<List<ProjectDashboardFiltersResponseDto.ProjectOrganizationDto>> GetOrganizations(int projectId)
        {
            if (_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
            {
                return await _nyssContext.Projects
                    .Where(p => p.Id == projectId)
                    .SelectMany(p => p.ProjectOrganizations)
                    .Select(o => new ProjectDashboardFiltersResponseDto.ProjectOrganizationDto
                    {
                        Id = o.Organization.Id,
                        Name = o.Organization.Name
                    }).ToListAsync();
            }

            var currentUserName = _authorizationService.GetCurrentUserName();

            return await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Role != Role.DataConsumer && uns.User.EmailAddress == currentUserName)
                .Select(o => new ProjectDashboardFiltersResponseDto.ProjectOrganizationDto
                {
                    Id = o.Organization.Id,
                    Name = o.Organization.Name
                }).ToListAsync();
        }

        public async Task<Result<ProjectDashboardResponseDto>> GetData(int projectId, FiltersRequestDto filtersDto)
        {
            if (filtersDto.EndDate < filtersDto.StartDate)
            {
                return Success(new ProjectDashboardResponseDto());
            }

            var filters = MapToReportFilters(projectId, filtersDto);
            var reportsByFeaturesAndDate = await _reportsDashboardByFeatureService.GetReportsGroupedByFeaturesAndDate(filters, filtersDto.GroupingType);

            var dashboardDataDto = new ProjectDashboardResponseDto
            {
                Summary = await _projectDashboardSummaryService.GetData(filters),
                ReportsGroupedByHealthRiskAndDate = await _reportsDashboardByHealthRiskService.GetReportsGroupedByHealthRiskAndDate(filters, filtersDto.GroupingType),
                ReportsGroupedByFeaturesAndDate = reportsByFeaturesAndDate,
                ReportsGroupedByVillageAndDate = await _reportsDashboardByVillageService.GetReportsGroupedByVillageAndDate(filters, filtersDto.GroupingType),
                ReportsGroupedByLocation = await _reportsDashboardMapService.GetProjectSummaryMap(filters),
                ReportsGroupedByFeatures = GetReportsGroupedByFeatures(reportsByFeaturesAndDate),
                DataCollectionPointReportsGroupedByDate = filtersDto.ReportsType == FiltersRequestDto.ReportsTypeDto.DataCollectionPoint
                    ? await _reportsDashboardByDataCollectionPointService.GetDataCollectionPointReports(filters, filtersDto.GroupingType)
                    : Enumerable.Empty<DataCollectionPointsReportsByDateDto>()
            };

            return Success(dashboardDataDto);
        }

        public async Task<Result<IEnumerable<ReportsSummaryHealthRiskResponseDto>>> GetProjectReportHealthRisks(int projectId, double latitude, double longitude, FiltersRequestDto filtersDto)
        {
            var filters = MapToReportFilters(projectId, filtersDto);
            var data = await _reportsDashboardMapService.GetProjectReportHealthRisks(filters, latitude, longitude);
            return Success(data);
        }

        private static ReportByFeaturesAndDateResponseDto GetReportsGroupedByFeatures(IList<ReportByFeaturesAndDateResponseDto> reportByFeaturesAndDate) =>
            new ReportByFeaturesAndDateResponseDto
            {
                Period = "all",
                CountFemalesAtLeastFive = reportByFeaturesAndDate.Sum(r => r.CountFemalesAtLeastFive),
                CountFemalesBelowFive = reportByFeaturesAndDate.Sum(r => r.CountFemalesBelowFive),
                CountMalesAtLeastFive = reportByFeaturesAndDate.Sum(r => r.CountMalesAtLeastFive),
                CountMalesBelowFive = reportByFeaturesAndDate.Sum(r => r.CountMalesBelowFive)
            };

        private ReportsFilter MapToReportFilters(int projectId, FiltersRequestDto filtersDto) =>
            new ReportsFilter
            {
                StartDate = filtersDto.StartDate,
                EndDate = filtersDto.EndDate.AddDays(1),
                HealthRiskId = filtersDto.HealthRiskId,
                OrganizationId = filtersDto.OrganizationId,
                Area = filtersDto.Area == null
                    ? null
                    : new Area
                    {
                        AreaType = filtersDto.Area.Type,
                        AreaId = filtersDto.Area.Id
                    },
                ProjectId = projectId,
                DataCollectorType = MapToDataCollectorType(filtersDto.ReportsType),
                IsTraining = filtersDto.IsTraining,
                TimezoneOffset = filtersDto.TimezoneOffset
            };

        private DataCollectorType? MapToDataCollectorType(FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector => DataCollectorType.Human,
                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint => DataCollectorType.CollectionPoint,
                _ => null as DataCollectorType?
            };
    }
}
