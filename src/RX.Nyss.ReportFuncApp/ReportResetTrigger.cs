using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp
{
    public class ReportResetTrigger
    {
        private readonly ILogger<ReportResetTrigger> _logger;
        private readonly IConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public ReportResetTrigger(ILogger<ReportResetTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("ResetReport")]
        public async Task DismissReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTRESETQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] ResetReportMessage resetReportMessage)
        {
            _logger.Log(LogLevel.Debug, $"Resetting report: '{resetReportMessage.ReportId}'");

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, $"api/report/reset?reportId={resetReportMessage.ReportId}"), null);

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int)postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"A report '{resetReportMessage}' was not properly reset by the Report API.");
            }
        }
    }
}
