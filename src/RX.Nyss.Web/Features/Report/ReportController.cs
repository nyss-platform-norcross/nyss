using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.Report.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Report
{
    [Route("api/report")]
    public class ReportController : BaseController
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Gets a list of reports in a project.
        /// </summary>
        [HttpPost("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, [FromBody] ReportListFilterRequestDto filterRequest) =>
            await _reportService.List(projectId, pageNumber, User.Identity.Name, filterRequest);

        /// <summary>
        /// Export the list of reports in a project to a csv file
        /// </summary>
        /// <param name="projectId">The ID of the project to export the reports from</param>
        /// <param name="reportListType">The type of the reports to export</param>
        /// <param name="isTrainig">A switch that specifies whether the main list or the report list should be exported</param>
        [HttpGet("exportToExcel")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> Export(int projectId, ReportListType reportListType, bool isTrainig)
        {
            var excelSheetBytes = await _reportService.Export(projectId, reportListType, isTrainig);
            return File(excelSheetBytes, "text/csv", "reports.csv");
        }
    }
}
