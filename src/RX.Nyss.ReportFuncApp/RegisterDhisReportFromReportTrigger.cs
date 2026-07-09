using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using Newtonsoft.Json;
using System.Text;
using RX.Nyss.ReportFuncApp.Contracts;

namespace RX.Nyss.ReportFuncApp;

public class RegisterDhisReportFromReportTrigger
{
    private readonly ILogger<RegisterDhisReportFromReportTrigger> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _reportApiBaseUrl;

    public RegisterDhisReportFromReportTrigger(ILogger<RegisterDhisReportFromReportTrigger> logger, IConfig config, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _reportApiBaseUrl = new Uri(config.ReportApiBaseUrl.TrimEnd('/') + '/', UriKind.Absolute);
    }

    [Function("RegisterDhisReportFromReport")]
    public void DequeueReportForDhis(
        [ServiceBusTrigger("%SERVICEBUS_DHISREPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] ReportForDhis reportForDhis)
    {
        _logger.Log(LogLevel.Debug, $"Dequeued report for dhis: '{reportForDhis}'");

        /*var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonConvert.SerializeObject(reportForDhis), Encoding.UTF8, "application/json");

        var postResult = await client.PostAsync(new Uri(_reportApiBaseUrl, "api/Report/registerDhisReport"), content);

        if (!postResult.IsSuccessStatusCode)
        {
            _logger.LogError($"Status code: {(int)postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
            throw new Exception($"A report '{reportForDhis.ReportId}' was not registered properly as a dhis report by the Report API.");
        }*/
    }
}

