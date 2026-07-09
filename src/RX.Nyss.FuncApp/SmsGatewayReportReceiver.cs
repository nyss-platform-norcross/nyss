using System.Web;
using Microsoft.Extensions.Logging;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;
using System.Net;


namespace RX.Nyss.FuncApp;

public class SmsGatewayReportReceiver
{
    private const string _apiKeyQueryParameterName = "apikey";
    private readonly ILogger<SmsGatewayReportReceiver> _logger;
    private readonly IConfig _config;
    private readonly IReportPublisherService _reportPublisherService;
    private readonly BlobServiceClient _blobServiceClient;

    public SmsGatewayReportReceiver(
        ILogger<SmsGatewayReportReceiver> logger,
        IConfig config, 
        IReportPublisherService reportPublisherService,
        BlobServiceClient blobServiceClient
    )
    {
        _logger = logger;
        _config = config;
        _reportPublisherService = reportPublisherService;
        _blobServiceClient = blobServiceClient;
    }

    [Function("EnqueueSmsGatewayReport")]
    public async Task<HttpResponseData> EnqueueSmsGatewayReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueSmsGatewayReport")] HttpRequestData httpRequest)
    {
        if (httpRequest.Body.Length == 0)
        {
            _logger.LogWarning("Received SMS Gateway report with header content length null.");
            return httpRequest.CreateResponse(HttpStatusCode.BadRequest);
        }

        var httpRequestContent = await new StreamReader(httpRequest.Body).ReadToEndAsync();
        _logger.Log(LogLevel.Debug, $"Received SMS Gateway report: {httpRequestContent}.{Environment.NewLine}HTTP request: {httpRequest}");

        if (string.IsNullOrWhiteSpace(httpRequestContent))
        {
            _logger.Log(LogLevel.Warning, "Received an empty SMS Gateway report.");
            return httpRequest.CreateResponse(HttpStatusCode.BadRequest);
        }

        var decodedHttpRequestContent = HttpUtility.UrlDecode(httpRequestContent);

        try
        {
            // Read authorized API keys from blob
        var authorizedApiKeysBlobPath = Environment.GetEnvironmentVariable("AuthorizedApiKeysBlobPath");
            if (string.IsNullOrWhiteSpace(authorizedApiKeysBlobPath))
            {
                _logger.LogError("Environment variable 'AuthorizedApiKeysBlobPath' is not set or empty.");
                throw new InvalidOperationException("Missing AuthorizedApiKeysBlobPath environment variable.");
            }
         var blobClient = GetBlobClient(_blobServiceClient, authorizedApiKeysBlobPath);
         var blobDownloadResult = await blobClient.DownloadContentAsync();
         var authorizedApiKeys = blobDownloadResult.Value.Content.ToString();

        if (!VerifyApiKey(authorizedApiKeys, decodedHttpRequestContent))
        {
            return httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var report = new Report
        {
            Content = httpRequestContent,
            ReportSource = ReportSource.SmsGateway
        };

        await _reportPublisherService.AddReportToQueue(report);
        return httpRequest.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendSms trigger");
            throw;
        }
    }

    private BlobClient GetBlobClient(BlobServiceClient serviceClient, string blobPath)
    {
        var parts = blobPath.Split('/', 2);
        var containerName = parts[0];
        var blobName = parts.Length > 1 ? parts[1] : string.Empty;
        
        return serviceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
    }

    private bool VerifyApiKey(string authorizedApiKeys, string decodedHttpRequestContent)
    {
        if (string.IsNullOrWhiteSpace(authorizedApiKeys))
        {
            _logger.Log(LogLevel.Critical, "The authorized API key list is empty.");
            return false;
        }

        var authorizedApiKeyList = authorizedApiKeys.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var apiKey = HttpUtility.ParseQueryString(decodedHttpRequestContent)[_apiKeyQueryParameterName];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.Log(LogLevel.Warning, "Received a SMS Gateway report with an empty API key.");
            return false;
        }

        if (!authorizedApiKeyList.Contains(apiKey))
        {
            _logger.Log(LogLevel.Warning, $"Received a SMS Gateway report with not authorized API key: {apiKey}.");
            return false;
        }

        return true;
    }
}