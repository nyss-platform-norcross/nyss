using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp
{
    public class SmsReceiver
    {
        private readonly ILogger<SmsReceiver> _logger;
        private readonly IConfiguration _configuration;

        public SmsReceiver(ILogger<SmsReceiver> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [FunctionName("Ping")]
        public IActionResult Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestMessage httpRequest)
        {
            _logger.Log(LogLevel.Debug, "Received ping request");
            return new OkObjectResult("I am alive!");
        }

        [FunctionName("EnqueueReport")]
        [return: ServiceBus("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")]
        public async Task<string> EnqueueReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueReport")] HttpRequestMessage httpRequest)
        {
            var httpRequestContent = await httpRequest.Content.ReadAsStringAsync();
            _logger.Log(LogLevel.Debug, $"Received report: {httpRequestContent}");
            return httpRequestContent;
        }

        [FunctionName("DequeueReport")]
        public async Task DequeueReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] string report)
        {
            _logger.Log(LogLevel.Debug, $"Dequeued report: {report}");
            await _configuration["InternalApiReportUrl"].PostUrlEncodedAsync(report);
        }

        [FunctionName("Report")]
        public async Task Report(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "report")] HttpRequestMessage httpRequest)
        {
            var httpRequestContent = await httpRequest.Content.ReadAsStringAsync();
            _logger.Log(LogLevel.Debug, $"Report stored: {httpRequestContent}");
        }
    }
}
