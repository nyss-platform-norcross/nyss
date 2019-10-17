using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Serilog;
using Flurl;
using Flurl.Http;

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
        public IActionResult Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestMessage httpRequest)
        {
            _logger.Debug("Received ping request");
            return new OkObjectResult("I am alive!");
        }

        [FunctionName("EnqueueReport")]
        [return: ServiceBus("nrx-cbs-dev-nyss-bus-report-queue", Connection = "SERVICEBUS_CONNECTIONSTRING")]
        public async Task<string> EnqueueReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueReport")] HttpRequestMessage httpRequest)
        {
            var httpRequestContent = await httpRequest.Content.ReadAsStringAsync();
            _logger.Debug($"Received report: {httpRequestContent}");
            return httpRequestContent;
        }

        [FunctionName("DequeueReport")]
        public async Task DequeueReport(
            [ServiceBusTrigger("nrx-cbs-dev-nyss-bus-report-queue", Connection = "SERVICEBUS_CONNECTIONSTRING")] string report)
        {
            _logger.Debug($"Dequeued report: {report}");

            await "https://nrx-cbs-dev-nyss-funcapp.azurewebsites.net/"
                .AppendPathSegments("api", "report")
                .PostUrlEncodedAsync(report);
        }

        [FunctionName("Report")]
        public async Task Report(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "report")] HttpRequestMessage httpRequest)
        {
            var httpRequestContent = await httpRequest.Content.ReadAsStringAsync();
            _logger.Debug($"Report stored: {httpRequestContent}");
        }
    }
}
