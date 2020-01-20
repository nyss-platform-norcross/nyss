using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Features.Projects.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    public interface IProjectDashboardService
    {
        Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId);
        
        Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto);
    }

    public class ProjectDashboardService : IProjectDashboardService
    {
        private readonly IProjectDashboardDataService _projectDashboardDataService;
        private readonly IProjectService _projectService;

        public ProjectDashboardService(
            IProjectService projectService,
            IProjectDashboardDataService projectDashboardDataService)
        {
            _projectService = projectService;
            _projectDashboardDataService = projectDashboardDataService;
        }

        public async Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId)
        {
            var healthRiskTypesWithoutActivity = new List<HealthRiskType> { HealthRiskType.Human, HealthRiskType.NonHuman, HealthRiskType.UnusualEvent };
            var projectHealthRisks = await _projectService.GetProjectHealthRiskNames(projectId, healthRiskTypesWithoutActivity);

            var dto = new ProjectDashboardFiltersResponseDto
            {
                HealthRisks = projectHealthRisks
            };

            return Success(dto);
        }

        public async Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto)
        {
            if (filtersDto.EndDate < filtersDto.StartDate)
            {
                return Success(new ProjectDashboardResponseDto());
            }

            var projectSummary = await _projectDashboardDataService.GetSummaryData(projectId, filtersDto);
            var reportsByDate = await _projectDashboardDataService.GetReportsGroupedByDate(projectId, filtersDto);
            var reportsByFeaturesAndDate = await _projectDashboardDataService.GetReportsGroupedByFeaturesAndDate(projectId, filtersDto);
            var reportsByFeatures = GetReportsGroupedByFeatures(reportsByFeaturesAndDate);
            var reportsGroupedByLocation = await _projectDashboardDataService.GetProjectSummaryMap(projectId, filtersDto);
            var dataCollectionPointReportsByDate = await _projectDashboardDataService.GetDataCollectionPointReports(projectId, filtersDto);
            var reportsByVillageAndDate = await _projectDashboardDataService.GetReportsGroupedByVillageAndDate(projectId, filtersDto);

            var dashboardDataDto = new ProjectDashboardResponseDto
            {
                Summary = projectSummary,
                ReportsGroupedByDate = reportsByDate,
                ReportsGroupedByFeaturesAndDate = reportsByFeaturesAndDate,
                ReportsGroupedByVillageAndDate = reportsByVillageAndDate,
                ReportsGroupedByLocation = reportsGroupedByLocation,
                ReportsGroupedByFeatures = reportsByFeatures,
                DataCollectionPointReportsGroupedByDate = dataCollectionPointReportsByDate
            };

            return Success(dashboardDataDto);
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
    }
}
