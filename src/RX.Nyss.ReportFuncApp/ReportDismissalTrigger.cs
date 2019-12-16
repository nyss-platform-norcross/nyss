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
    public class ReportDismissalTrigger
    {
        private readonly ILogger<ReportDismissalTrigger> _logger;
        private readonly IConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public ReportDismissalTrigger(ILogger<ReportDismissalTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("DismissReport")]
        public async Task DismissReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTDISMISSALQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] DismissReportMessage dismissReportMessage)
        {
            _logger.Log(LogLevel.Debug, $"Dismissing report: '{dismissReportMessage.ReportId}'");

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, $"api/report/dismiss?reportId={dismissReportMessage.ReportId}"), null);

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int) postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"A report '{dismissReportMessage}' was not properly dismissed by the Report API.");
            }
        }
    }
}
