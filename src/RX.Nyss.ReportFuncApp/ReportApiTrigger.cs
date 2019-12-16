using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp
{
    public class ReportApiTrigger
    {
        private readonly ILogger<ReportApiTrigger> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public ReportApiTrigger(ILogger<ReportApiTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("DequeueReport")]
        public async Task DequeueReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] Report report)
        {
            _logger.Log(LogLevel.Debug, $"Dequeued report: '{report}'");

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/Report"), report, new JsonMediaTypeFormatter());

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int) postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"A report '{report}' was not handled properly by the Report API.");
            }
        }
    }
}
