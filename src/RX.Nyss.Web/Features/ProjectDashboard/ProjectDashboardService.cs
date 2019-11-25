using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
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
        private readonly INyssContext _nyssContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly IProjectService _projectService;

        public ProjectDashboardService(
            INyssContext nyssContext,
            IDateTimeProvider dateTimeProvider,
            INationalSocietyStructureService nationalSocietyStructureService,
            IProjectService projectService)
        {
            _nyssContext = nyssContext;
            _dateTimeProvider = dateTimeProvider;
            _nationalSocietyStructureService = nationalSocietyStructureService;
            _projectService = projectService;
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
                    .Select(p => new ProjectDashboardFiltersResponseDto.HealthRiskDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
            };

            return Success(dto);
        }

        public async Task<Result<ProjectDashboardResponseDto>> GetDashboardData(int projectId, FiltersRequestDto filtersDto)
        {
            var dashboardDataDto = new ProjectDashboardResponseDto
            {
                Summary = await GetSummaryData(projectId, filtersDto)
            };

            return Success(dashboardDataDto);
        }

        private async Task<ProjectSummaryResponseDto> GetSummaryData(int projectId, FiltersRequestDto filtersDto)
        {
            var minReportDate = (filtersDto.StartDate ?? _dateTimeProvider.UtcNow).Date.Subtract(TimeSpan.FromDays(28));
            var dayAfterMaxReportDate = (filtersDto.EndDate ?? _dateTimeProvider.UtcNow).Date.AddDays(1);

            return await _nyssContext.ProjectHealthRisks
                .Where(ph => ph.Project.Id == projectId && (!filtersDto.HealthRiskId.HasValue || ph.HealthRiskId == filtersDto.HealthRiskId.Value))
                .Select(ph => ph.Project)
                .Select(p => new ProjectSummaryResponseDto
                {
                    ReportCount = p.DataCollectors
                        .Where(dc => dc.CreatedAt < minReportDate && (!dc.DeletedAt.HasValue || dc.DeletedAt >= minReportDate))
                        .SelectMany(d => d.Reports)
                        .Count(r => r.ReceivedAt >= minReportDate && r.ReceivedAt < dayAfterMaxReportDate),
                    ActiveDataCollectorCount = p.DataCollectors.Count(dc =>
                        dc.DataCollectorType == DataCollectorType.Human &&
                        dc.Reports.Any(r => r.ReceivedAt >= minReportDate && r.ReceivedAt < dayAfterMaxReportDate)),
                    InactiveDataCollectorCount = p.DataCollectors.Count(dc =>
                        dc.DataCollectorType == DataCollectorType.Human &&
                        !dc.Reports.Any(r => r.ReceivedAt >= minReportDate && r.ReceivedAt < dayAfterMaxReportDate) &&
                        dc.CreatedAt < minReportDate && (!dc.DeletedAt.HasValue || dc.DeletedAt >= minReportDate)),
                    InTrainingDataCollectorCount = p.DataCollectors.Count(dc =>
                        dc.DataCollectorType == DataCollectorType.Human &&
                        dc.Reports.Any(r => r.IsTraining && r.ReceivedAt >= minReportDate && r.ReceivedAt < dayAfterMaxReportDate)),
                })
                .FirstOrDefaultAsync();
        }

    }
}
