using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Data;
using RX.Nyss.Web.Utils;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard
{
    [Route("api/nationalSocietyDashboard")]
    public class NationalSocietyDashboardController : BaseController
    {
        private readonly INationalSocietyDashboardReportsMapService _nationalSocietyDashboardReportsMapService;
        private readonly INationalSocietyDashboardService _nationalSocietyDashboardService;

        public NationalSocietyDashboardController(
            INationalSocietyDashboardService nationalSocietyDashboardService,
            INationalSocietyDashboardReportsMapService nationalSocietyDashboardReportsMapService)
        {
            _nationalSocietyDashboardReportsMapService = nationalSocietyDashboardReportsMapService;
            _nationalSocietyDashboardService = nationalSocietyDashboardService;
        }

        /// <summary>
        /// Gets filters data for the national society dashboard
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        [HttpGet("filters")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.GlobalCoordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<NationalSocietyDashboardFiltersResponseDto>> GetFilters(int nationalSocietyId) =>
            _nationalSocietyDashboardService.GetDashboardFiltersData(nationalSocietyId);

        /// <summary>
        /// Gets a summary of specified national society displayed on the dashboard page.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <returns>A summary of specified project</returns>
        [HttpPost("data")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.GlobalCoordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<NationalSocietyDashboardResponseDto>> GetData(int nationalSocietyId, [FromBody]NationalSocietyDashboardFiltersRequestDto dto) =>
            _nationalSocietyDashboardService.GetDashboardData(nationalSocietyId, dto);

        /// <summary>
        /// Gets health risk summary basing on a location and filters
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="latitude">Latitude of chosen location</param>
        /// <param name="longitude">Longitude of chosen location</param>
        /// <param name="filters">Filters</param>
        [HttpPost("reportHealthRisks")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.GlobalCoordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<IEnumerable<NationalSocietySummaryReportHealthRiskResponseDto>>> GetReportHealthRisks(int nationalSocietyId, double latitude, double longitude, [FromBody]NationalSocietyDashboardFiltersRequestDto filters) =>
            Success(await _nationalSocietyDashboardReportsMapService.GetLocationHealthRisks(nationalSocietyId, latitude, longitude, filters));
    }
}
