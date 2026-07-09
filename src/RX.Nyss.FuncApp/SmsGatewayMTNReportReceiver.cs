using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text;

namespace RX.Nyss.FuncApp;

public class SmsGatewayMTNReportReceiver
{
    private const string _apiKeyQueryParameterName = "apikey";
    private readonly ILogger<SmsGatewayMTNReportReceiver> _logger;
    private readonly IConfig _config;
    private readonly IMTNReportPublisherService _reportPublisherService;
    private readonly BlobServiceClient _blobServiceClient;

    public SmsGatewayMTNReportReceiver(
        ILogger<SmsGatewayMTNReportReceiver> logger,
        IConfig config, 
        IMTNReportPublisherService reportPublisherService,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _config = config;
        _reportPublisherService = reportPublisherService;
        _blobServiceClient = blobServiceClient;
    }

    [Function("EnqueueSmsGatewayMTNReport")]
    public async Task<HttpResponseData> EnqueueSmsGatewayMTNReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueSmsGatewayMTNReport")] HttpRequestData httpRequest)
    {
        var httpRequestContent = await new StreamReader(httpRequest.Body).ReadToEndAsync();
        _logger.LogDebug($"Received SMS Eagle report: {httpRequestContent}.{Environment.NewLine}HTTP request: {httpRequest}");

        if (string.IsNullOrWhiteSpace(httpRequestContent))
        {
            _logger.LogWarning("Received an empty Nyss report.");
            return ReturnBadHttpResult(httpRequest);
        }

        var input = JsonConvert.DeserializeObject<MTNReport>(httpRequestContent);
        if (input == null)
        {
            _logger.LogWarning("Failed to deserialize MTN report from request body.");
            return ReturnBadHttpResult(httpRequest);
        }
        
        var report = new MTNReport
        {
            SenderAddress = input.SenderAddress,
            ReceiverAddress = input.ReceiverAddress,
            SubmittedDate = input.SubmittedDate,
            Message = input.Message,
            Created = input.Created,
            Id = input.Id,
            ReportSource = ReportSource.MTNSmsGateway
        };
        
        await _reportPublisherService.AddMTNReportToQueue(report);
        return ReturnOkHttpResult(httpRequest, report.Id);
    }
    private HttpResponseData ReturnBadHttpResult(HttpRequestData httpRequest)
    {
        var json = JsonConvert.SerializeObject(new SendSuccessCallbackMessageObject
        {
            Status = "Error",
            TransactionId = null,
        });
        var response = httpRequest.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var bytes = Encoding.UTF8.GetBytes(json);
        response.Body = new MemoryStream(bytes);
        return response;
    }
        
    private HttpResponseData ReturnOkHttpResult(HttpRequestData httpRequest, string messageId)
    {
        var json = JsonConvert.SerializeObject(new SendSuccessCallbackMessageObject
        {
            Status = "Processed",
            TransactionId = messageId,
        });
        var response = httpRequest.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var bytes = Encoding.UTF8.GetBytes(json);
        response.Body = new MemoryStream(bytes);
        return response;
    }

}

public class SendSuccessCallbackMessageObject
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }
}