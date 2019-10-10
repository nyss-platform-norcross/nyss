using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Serilog;

namespace RX.Nyss.FuncApp
{
    public class SmsReceiver
    {
        private readonly ILogger _logger;

        public SmsReceiver(ILogger logger)
        {
            _logger = logger;
        }

        [FunctionName("Ping")]
        public async Task<IActionResult> Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")]
            HttpRequestMessage req)
        {
            _logger.Debug("Received ping request");

            return new OkObjectResult("I am alive!");
        }
    }
}
