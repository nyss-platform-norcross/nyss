using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.ReportApi.Models;
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
        public async Task<IActionResult> Post([FromBody] Sms sms) =>
            await _reportService.ReceiveSms(sms.Content) ? (StatusCodeResult) new OkResult() : new BadRequestResult();
    }
}
