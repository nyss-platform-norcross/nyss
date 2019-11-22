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
        Task<Result<ProjectSummaryResponseDto>> GetProjectSummary(int projectId, int? healthRiskId = null, DateTime? startDate = default, DateTime? endDate = default);
        Task<Result<ProjectDashboardFiltersResponseDto>> GetDashboardFiltersData(int projectId);
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

        public async Task<Result<ProjectSummaryResponseDto>> GetProjectSummary(int projectId, int? healthRiskId = null,
            DateTime? startDate = default, DateTime? endDate = default)
        {
            var minReportDate = (startDate ?? _dateTimeProvider.UtcNow).Date.Subtract(TimeSpan.FromDays(28));
            var dayAfterMaxReportDate = (endDate ?? _dateTimeProvider.UtcNow).Date.AddDays(1);

            var projectDashboardData = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new ProjectSummaryResponseDto
                {
                    StartDate = p.StartDate,
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
                    HealthRisks = p.ProjectHealthRisks
                        .Where(phr => healthRiskId == null || phr.HealthRiskId == healthRiskId)
                        .Select(phr => new ProjectSummaryResponseDto.HealthRiskStats
                        {
                            Id = phr.HealthRiskId,
                            Name = phr.HealthRisk.LanguageContents
                                .Where(lc => lc.ContentLanguage.Id == p.NationalSociety.ContentLanguage.Id)
                                .Select(lc => lc.Name)
                                .FirstOrDefault(),
                            TotalReportCount = phr.Reports.Count(),
                            EscalatedAlertCount = phr.Alerts.Count(a => a.Status == AlertStatus.Escalated),
                            DismissedAlertCount = phr.Alerts.Count(a => a.Status == AlertStatus.Dismissed)
                        }),
                    Supervisors = p.SupervisorUserProjects.Select(su => new ProjectSummaryResponseDto.SupervisorInfo
                    {
                        Id = su.SupervisorUser.Id,
                        Name = su.SupervisorUser.Name,
                        EmailAddress = su.SupervisorUser.EmailAddress,
                        PhoneNumber = su.SupervisorUser.PhoneNumber,
                        AdditionalPhoneNumber = su.SupervisorUser.AdditionalPhoneNumber
                    }
                    )
                })
                .FirstOrDefaultAsync();

            var result = Success(projectDashboardData);

            return result;
        }
    }
}
