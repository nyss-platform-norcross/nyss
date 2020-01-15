using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Utils;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    [Route("api/projectDashboard")]
    public class ProjectDashboardController : BaseController
    {
        private readonly IProjectDashboardService _projectDashboardService;
        private readonly IProjectDashboardDataService _projectDashboardDataService;

        public ProjectDashboardController(
            IProjectDashboardService projectDashboardService,
            IProjectDashboardDataService projectDashboardDataService)
        {
            _projectDashboardService = projectDashboardService;
            _projectDashboardDataService = projectDashboardDataService;
        }

        /// <summary>
        /// Gets filters data for the project dashboard
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        [HttpGet("filters")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectDashboardFiltersResponseDto>> GetFilters(int projectId) =>
            _projectDashboardService.GetDashboardFiltersData(projectId);

        /// <summary>
        /// Gets a summary of specified project displayed on the dashboard page.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>A summary of specified project</returns>
        [HttpPost("data")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectDashboardResponseDto>> GetData(int projectId, [FromBody]FiltersRequestDto dto) =>
            _projectDashboardService.GetDashboardData(projectId, dto);

        /// <summary>
        /// Gets health risk summary basing on a location and filters
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="latitude">Latitude of chosen location</param>
        /// <param name="longitude">Longitude of chosen location</param>
        /// <param name="filters">Filters</param>
        [HttpPost("reportHealthRisks")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<IEnumerable<ProjectSummaryReportHealthRiskResponseDto>>> GetReportHealthRisks(int projectId, double latitude, double longitude, [FromBody]FiltersRequestDto filters) =>
            Success(await _projectDashboardDataService.GetProjectReportHealthRisks(projectId, latitude, longitude, filters));
    }
}
