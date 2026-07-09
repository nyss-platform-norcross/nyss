using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using Newtonsoft.Json;
using System.Text;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp;

public class MTNReportApiTrigger
{
    private readonly ILogger<MTNReportApiTrigger> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _reportApiBaseUrl;

    public MTNReportApiTrigger(ILogger<MTNReportApiTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl.TrimEnd('/') + '/', UriKind.Absolute);
    }

    [Function("DequeueMTNReport")]
    public async Task DequeueMTNReport(
        [ServiceBusTrigger("%SERVICEBUS_MTNREPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] MTNReport report)
    {
        _logger.Log(LogLevel.Debug, $"Dequeued MTN report: '{report}'");

        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonConvert.SerializeObject(report), Encoding.UTF8, "application/json");
        var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/Report/mtnReport"), content);

        if (!postResult.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Status code: {(int)postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
        }
    }
}
