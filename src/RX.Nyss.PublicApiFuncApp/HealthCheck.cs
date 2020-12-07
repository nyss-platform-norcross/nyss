using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using RX.Nyss.PublicApiFuncApp.Configuration;

namespace RX.Nyss.PublicApiFuncApp
{
    public class HealthCheck
    {
        private readonly ILogger<HealthCheck> _logger;
        private readonly IConfig _config;

        public HealthCheck(ILogger<HealthCheck> logger, IConfig config)
        {
            _logger = logger;
            _config = config;
        }

        [FunctionName("Ping")]
        public IActionResult Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestMessage httpRequest)
        {
            _logger.Log(LogLevel.Debug, "Received ping request");
            return new OkObjectResult("I am alive!");
        }

        [FunctionName("Version")]
        public IActionResult Version(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequestMessage httpRequest)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var version = assemblyName.Version;

            return new OkObjectResult(new
            {
                assemblyName.Name,
                Version = $"{version.Major}.{version.Minor}.{version.Build}",
                ReleaseName = _config.ReleaseName,
                Framework = RuntimeInformation.FrameworkDescription
            });
        }
    }
}
