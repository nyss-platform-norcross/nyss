using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.NationalSocietyReports.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSocietyReports
{
    [Route("api/nationalSocietyReport")]
    public class NationalSocietyReportsController : BaseController
    {
        private readonly INationalSocietyReportService _nationalSocietyReportService;

        public NationalSocietyReportsController(INationalSocietyReportService nationalSocietyReportService)
        {
            _nationalSocietyReportService = nationalSocietyReportService;
        }

        /// <summary>
        /// Gets a list of reports in a national society.
        /// </summary>
        [HttpPost("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, [FromBody] NationalSocietyReportListFilterRequestDto filterRequest) =>
            await _nationalSocietyReportService.List(nationalSocietyId, pageNumber, filterRequest);
    }
}
