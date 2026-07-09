using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using Newtonsoft.Json;
using System.Text;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp;

public class TelerivetReportApiTrigger
{
    private readonly ILogger<TelerivetReportApiTrigger> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _reportApiBaseUrl;

    public TelerivetReportApiTrigger(ILogger<TelerivetReportApiTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl.TrimEnd('/') + '/', UriKind.Absolute);
    }

    [Function("DequeueTelerivetReport")]
    public async Task DequeueTelerivetReport(
        [ServiceBusTrigger("%SERVICEBUS_TELERIVETREPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] TelerivetReport report)
    {
        _logger.Log(LogLevel.Debug, "Dequeued telerivet report: {Report}", report);

        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonConvert.SerializeObject(report), Encoding.UTF8, "application/json");
        var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/Report/telerivetReport"), content);

        if (!postResult.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Telerivet report dequeue failed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                (int)postResult.StatusCode,
                postResult.ReasonPhrase);
        }
    }
}
