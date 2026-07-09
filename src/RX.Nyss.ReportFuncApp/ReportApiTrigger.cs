using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using Newtonsoft.Json;
using System.Text;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp;

public class ReportApiTrigger
{
    private readonly ILogger<ReportApiTrigger> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _reportApiBaseUrl;

    public ReportApiTrigger(ILogger<ReportApiTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl.TrimEnd('/') + '/', UriKind.Absolute);
    }

    [Function("DequeueReport")]
    public async Task DequeueReport(
        [ServiceBusTrigger("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] Report report)
    {
        _logger.Log(LogLevel.Debug, $"Dequeued report: '{report}'");

        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonConvert.SerializeObject(report), Encoding.UTF8, "application/json");
        var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/Report/registerReport"), content);

        if (!postResult.IsSuccessStatusCode)
        {
            var responseBody = await postResult.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Report API returned {StatusCode} {ReasonPhrase} for report '{Report}'. Response body: {ResponseBody}",
                (int)postResult.StatusCode,
                postResult.ReasonPhrase,
                report,
                responseBody);
            //throw new Exception($"A report '{report}' was not handled properly by the Report API. Status: {(int)postResult.StatusCode}, Body: {responseBody}");
        }
    }
}
