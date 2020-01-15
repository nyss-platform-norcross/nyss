using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.ReportApi.Features.Reports.Contracts;

namespace RX.Nyss.ReportApi.Features.Reports
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Report report) =>
            await _reportService.ReceiveReport(report) ? (StatusCodeResult) new OkResult() : new BadRequestResult();

        [HttpPost("dismiss")]
        public async Task<IActionResult> Dismiss(int reportId) =>
            await _reportService.DismissReport(reportId) ? (StatusCodeResult)new OkResult() : new BadRequestResult();
    }
}
