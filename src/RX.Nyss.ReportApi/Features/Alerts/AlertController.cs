using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckAlert(int alertId)
        {
            await _alertService.CheckIfAlertHaveBeenHandled(alertId);

            return Ok();
        }
    }
}
