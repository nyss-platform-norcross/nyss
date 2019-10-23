using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp
{
    public class HealthCheck
    {
        private readonly ILogger<HealthCheck> _logger;

        public HealthCheck(ILogger<HealthCheck> logger)
        {
            _logger = logger;
        }

        [FunctionName("Ping")]
        public IActionResult Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestMessage httpRequest)
        {
            _logger.Log(LogLevel.Debug, "Received ping request");
            return new OkObjectResult("I am alive!");
        }
    }
}
