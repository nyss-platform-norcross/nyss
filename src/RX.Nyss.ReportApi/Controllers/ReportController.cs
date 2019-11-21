using Microsoft.AspNetCore.Mvc;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILoggerAdapter _loggerAdapter;

        public ReportController(ILoggerAdapter loggerAdapter)
        {
            _loggerAdapter = loggerAdapter;
        }

        [HttpPost]
        public void Post([FromForm] string text)
        {
            _loggerAdapter.Debug("Received form:" + text);
        }
    }
}
