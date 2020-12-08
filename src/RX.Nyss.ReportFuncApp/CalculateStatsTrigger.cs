using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;

namespace RX.Nyss.ReportFuncApp
{
    public class CalculateStatsTrigger
    {
        private readonly ILogger<CalculateStatsTrigger> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _reportApiBaseUrl;

        public CalculateStatsTrigger(ILogger<CalculateStatsTrigger> logger, IHttpClientFactory httpClientFactory, IConfig config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl, UriKind.Absolute);
        }

        [FunctionName("CalculateStatsTrigger")]
        public async Task CalculateStats(
            [TimerTrigger("0 0 0 * * *")] TimerInfo timer,
            ILogger log)
        {
            log.LogInformation("Running calculate stats trigger.");
            if (timer.IsPastDue)
            {
                log.LogWarning($"Calculate stats trigger function is running late. Executed at: {DateTime.UtcNow}");
            }

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/stats/calculate"), null);

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int)postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception("Calculate stats was not handled properly by the Report API.");
            }
        }
    }
}
