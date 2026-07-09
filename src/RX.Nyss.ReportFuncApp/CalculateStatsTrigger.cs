using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
namespace RX.Nyss.ReportFuncApp;

public class CalculateStatsTrigger
{
    private readonly ILogger<CalculateStatsTrigger> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _reportApiBaseUrl;

    public CalculateStatsTrigger(ILogger<CalculateStatsTrigger> logger, IHttpClientFactory httpClientFactory, IConfig config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        if (string.IsNullOrWhiteSpace(config?.ReportApiBaseUrl))
        {
            throw new ArgumentException("ReportApiBaseUrl is not configured");
        }

        _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl.TrimEnd('/') + '/', UriKind.Absolute);
    }

    [Function("CalculateStatsTrigger")]
    public async Task CalculateStats(
    [TimerTrigger("0 0 0 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Running calculate stats trigger.");

        if (timer.IsPastDue)
        {
            _logger.LogWarning("Calculate stats trigger function is running late. Executed at: {ExecutedAt}", DateTime.UtcNow);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            var targetUri = new Uri(_reportApiBaseUrl, "api/stats/calculate");

            _logger.LogInformation("Calling: {TargetUri}", targetUri);

            var postResult = await client.PostAsync(targetUri, null);

            if (!postResult.IsSuccessStatusCode)
            {
                var responseBody = await postResult.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Calculate stats failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Response: {ResponseBody}",
                    (int)postResult.StatusCode,
                    postResult.ReasonPhrase,
                    responseBody);
                throw new Exception("Calculate stats was not handled properly by the Report API.");
            }

            _logger.LogInformation("Calculate stats completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during calculate stats execution");
            throw;
        }
    }
}
