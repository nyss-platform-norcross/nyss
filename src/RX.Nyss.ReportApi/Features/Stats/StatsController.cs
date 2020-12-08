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

        [HttpPost("stats")]
        public async Task<IActionResult> Stats()
        {
            await _statsService.CalculateStats();

            return Ok();
        }
    }
}
