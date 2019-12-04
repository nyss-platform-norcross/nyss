using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.ReportApi.Contracts;
using RX.Nyss.ReportApi.Services;

namespace RX.Nyss.ReportApi.Controllers
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
    }
}
