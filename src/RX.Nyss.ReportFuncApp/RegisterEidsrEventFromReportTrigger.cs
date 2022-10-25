using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RX.Nyss.ReportFuncApp.Configuration;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp
{
    public class RegisterEidsrEventFromReportTrigger
    {
        private readonly ILogger<RegisterEidsrEventFromReportTrigger> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public RegisterEidsrEventFromReportTrigger(ILogger<RegisterEidsrEventFromReportTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("RegisterEidsrEventFromReport")]
        public async Task DequeueReportForEidsr(
            [ServiceBusTrigger("%SERVICEBUS_EIDSRREPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] ReportForEidsr reportForEidsr)
        {
            _logger.Log(LogLevel.Debug, $"Dequeued report for Eidsr: '{reportForEidsr}'");

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(reportForEidsr), Encoding.UTF8, "application/json");

            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/Report/registerEidsrEvent"), content);

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int)postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"A report '{reportForEidsr.ReportId}' was not registered properly as an eidsr event by the Report API.");
            }
        }
    }
}
