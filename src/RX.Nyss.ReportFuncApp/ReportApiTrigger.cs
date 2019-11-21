using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;

namespace RX.Nyss.ReportFuncApp
{
    public class ReportApiTrigger
    {
        private readonly ILogger<ReportApiTrigger> _logger;
        private readonly INyssReportFuncAppConfig _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReportApiTrigger(ILogger<ReportApiTrigger> logger, INyssReportFuncAppConfig configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("DequeueReport")]
        public async Task DequeueReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] string report)
        {
            _logger.Log(LogLevel.Debug, $"Dequeued report: {report}");
            var client = _httpClientFactory.CreateClient();
            await client.PostAsync(_configuration.ReportApiUrl, new StringContent(report, Encoding.UTF8, "application/x-www-form-urlencoded"));
        }
    }
}
