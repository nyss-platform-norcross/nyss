using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    [Route("api/projectDashboard")]
    public class ProjectDashboardController : BaseController
    {
        private readonly IProjectDashboardService _projectDashboardService;

        public ProjectDashboardController(IProjectDashboardService projectDashboardService)
        {
            _projectDashboardService = projectDashboardService;
        }

        /// <summary>
        /// Gets filters data for the project dashboard
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        [HttpGet("filters")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectDashboardFiltersResponseDto>> Filters(int projectId) =>
            _projectDashboardService.GetFiltersData(projectId);

        /// <summary>
        /// Gets a summary of specified project displayed on the dashboard page.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="dto">Filter</param>
        /// <returns>A summary of specified project</returns>
        [HttpPost("data")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectDashboardResponseDto>> Data(int projectId, [FromBody] FiltersRequestDto dto) =>
            _projectDashboardService.GetData(projectId, dto);

        /// <summary>
        /// Gets health risk summary basing on a location and filters
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="latitude">Latitude of chosen location</param>
        /// <param name="longitude">Longitude of chosen location</param>
        /// <param name="filters">Filters</param>
        [HttpPost("reportHealthRisks")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<IEnumerable<ReportsSummaryHealthRiskResponseDto>>> GetReportHealthRisks(int projectId, double latitude, double longitude, [FromBody] FiltersRequestDto filters) =>
            await _projectDashboardService.GetProjectReportHealthRisks(projectId, latitude, longitude, filters);
    }
}
