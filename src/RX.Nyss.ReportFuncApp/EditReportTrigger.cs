using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp
{
    public class EditReportTrigger
    {
        private readonly ILogger<EditReportTrigger> _logger;
        private readonly IConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public EditReportTrigger(ILogger<EditReportTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("EditReport")]
        public async Task EditReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTEDITQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] int reportId)
        {
            _logger.Log(LogLevel.Debug, $"Potential alert for the following report is recalculated: '{reportId}'");

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, $"api/report/edit?reportId={reportId}"), null);

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int)postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"Recalculation of potential alert for the following report failed: '{reportId}'");
            }
        }
    }
}
