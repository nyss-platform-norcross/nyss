using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.Report.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Report
{
    [Route("api")]
    public class ReportController : BaseController
    {
        public IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Gets a list of reports in a project
        /// </summary>
        /// <returns></returns>
        [HttpGet("project/{projectId:int}/report/list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<PagedResult<List<ReportListResponseDto>>> List(int projectId, int pageNumber) =>
            await _reportService.List(projectId, pageNumber, User.Identity.Name);
    }
}
