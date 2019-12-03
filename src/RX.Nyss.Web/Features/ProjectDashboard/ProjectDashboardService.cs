using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    public interface IProjectDashboardService
    {
        Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId);
        
        Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto);
    }

    public class ProjectDashboardService : IProjectDashboardService
    {
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly INyssContext _nyssContext;
        private readonly IProjectDashboardDataService _projectDashboardDataService;
        private readonly IProjectService _projectService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ProjectDashboardService(
            INyssContext nyssContext,
            INationalSocietyStructureService nationalSocietyStructureService,
            IProjectService projectService,
            IProjectDashboardDataService projectDashboardDataService, IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _nationalSocietyStructureService = nationalSocietyStructureService;
            _projectService = projectService;
            _projectDashboardDataService = projectDashboardDataService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId)
        {
            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleAsync();

            var structure = await _nationalSocietyStructureService.GetStructure(nationalSocietyId);

            if (!structure.IsSuccess)
            {
                return Error<ProjectDashboardFiltersResponseDto>(structure.Message.Key);
            }

            var projectHealthRiskNames = await _projectService.GetProjectHealthRiskNames(projectId);

            var dto = new ProjectDashboardFiltersResponseDto
            {
                Regions = structure.Value.Regions,
                HealthRisks = projectHealthRiskNames
                    .Select(p => new ProjectDashboardFiltersResponseDto.HealthRiskDto { Id = p.Id, Name = p.Name })
            };

            return Success(dto);
        }

        public async Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto)
        {
            var projectSummary = await _projectDashboardDataService.GetSummaryData(projectId, filtersDto);
            var reportsByDate = await _projectDashboardDataService.GetReportsGroupedByDate(projectId, filtersDto);
            var reportsByFeaturesAndDate = await _projectDashboardDataService.GetReportsGroupedByFeaturesAndDate(projectId, filtersDto);
            var reportsByFeatures = GetReportsGroupedByFeatures(reportsByFeaturesAndDate);
            var reportsGroupedByLocation = await _projectDashboardDataService.GetProjectSummaryMap(projectId, filtersDto);

            var dashboardDataDto = new ProjectDashboardResponseDto
            {
                Summary = projectSummary,
                ReportsGroupedByDate = reportsByDate,
                ReportsGroupedByFeaturesAndDate = reportsByFeaturesAndDate,
                ReportsGroupedByLocation = reportsGroupedByLocation,
                ReportsGroupedByFeatures = reportsByFeatures,
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
