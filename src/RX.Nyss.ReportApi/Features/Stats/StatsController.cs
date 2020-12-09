using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RX.Nyss.ReportApi.Features.Stats
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;

        public StatsController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate()
        {
            await _statsService.CalculateStats();

            return Ok();
        }
    }
}
