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
    public class CheckAlertTrigger
    {
        private readonly ILogger<CheckAlertTrigger> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public CheckAlertTrigger(ILogger<CheckAlertTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("CheckAlert")]
        public async Task DequeueReport(
            [ServiceBusTrigger("%SERVICEBUS_CHECKALERTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] int alertId)
        {
            _logger.Log(LogLevel.Debug, $"Checking alert triggered. AlertId: '{alertId}'");

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, $"api/alert/check?alertId={alertId}"), null);

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int) postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"Check alert '{alertId}' was not handled properly by the Report API.");
            }
        }
    }
}
