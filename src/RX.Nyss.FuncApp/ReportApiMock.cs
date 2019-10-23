using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp
{
    public class ReportApiMock
    {
        private readonly ILogger<ReportApiMock> _logger;

        public ReportApiMock(ILogger<ReportApiMock> logger)
        {
            _logger = logger;
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
