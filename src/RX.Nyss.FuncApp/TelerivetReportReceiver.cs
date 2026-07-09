using System.Net;
using System.Web;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;

namespace RX.Nyss.FuncApp;

public class TelerivetReportReceiver
{
    private const string _apiKeyQueryParameterName = "apikey";
    private readonly ILogger<TelerivetReportReceiver> _logger;
    private readonly IConfig _config;
    private readonly ITelerivetReportPublisherService _reportPublisherService;
    private readonly BlobServiceClient _blobServiceClient;

    public TelerivetReportReceiver(
        ILogger<TelerivetReportReceiver> logger, 
        IConfig config, 
        ITelerivetReportPublisherService reportPublisherService,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _config = config;
        _reportPublisherService = reportPublisherService;
        _blobServiceClient = blobServiceClient;
    }

    [Function("EnqueueTelerivetReport")]
    public async Task<HttpResponseData> EnqueueTelerivetReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueTelerivetReport")] HttpRequestData request)
    {
        if (request.Body.Length == 0)
        {
            _logger.LogWarning("Received a Telerivet report with header content length null.");
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var httpRequestContent = await new StreamReader(request.Body).ReadToEndAsync();
        _logger.LogDebug($"Received Telerivet report: {httpRequestContent}.{Environment.NewLine}HTTP request: {request}");

        if (string.IsNullOrWhiteSpace(httpRequestContent))
        {
            _logger.LogWarning("Received an empty Telerivet report.");
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var decodedHttpRequestContent = HttpUtility.UrlDecode(httpRequestContent);
        var result = HttpUtility.ParseQueryString(decodedHttpRequestContent);

        // Read authorized API keys from blob
        string authorizedApiKeys;
        try
        {
            var authorizedApiKeysBlobPath = Environment.GetEnvironmentVariable("AuthorizedApiKeysBlobPath");
            if (string.IsNullOrWhiteSpace(authorizedApiKeysBlobPath))
            {
                _logger.LogCritical("AuthorizedApiKeysBlobPath environment variable is not set.");
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var blobClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(authorizedApiKeysBlobPath))
                .GetBlobClient(GetBlobName(authorizedApiKeysBlobPath));
            
            var blobDownloadResult = await blobClient.DownloadContentAsync();
            authorizedApiKeys = blobDownloadResult.Value.Content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read authorized API keys from blob storage.");
            return request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        if (!VerifyApiKey(authorizedApiKeys, decodedHttpRequestContent))
        {
            return request.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var report = new TelerivetReport
        {
            TimeCreated = result["time_created"] ?? string.Empty,
            TimeUpdated = result["time_updated"] ?? string.Empty,
            MessageContent = result["content"] ?? string.Empty,
            FromNumber = result["from_number_e164"] ?? string.Empty,
            ApiKey = result["apikey"] ?? string.Empty,
            ProjectId = result["project_id"] ?? string.Empty,
            ReportSource = ReportSource.Telerivet
        };

        await _reportPublisherService.AddTelerivetReportToQueue(report);
        return request.CreateResponse(HttpStatusCode.OK);
    }

    private bool VerifyApiKey(string authorizedApiKeys, string decodedHttpRequestContent)
    {
        if (string.IsNullOrWhiteSpace(authorizedApiKeys))
        {
            _logger.LogCritical("The authorized API key list is empty.");
            return false;
        }

        var authorizedApiKeyList = authorizedApiKeys.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var apiKey = HttpUtility.ParseQueryString(decodedHttpRequestContent)[_apiKeyQueryParameterName];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Received a Telerivet report with an empty API key.");
            return false;
        }

        if (!authorizedApiKeyList.Contains(apiKey))
        {
            _logger.LogWarning($"Received a Telerivet report with not authorized API key: {apiKey}.");
            return false;
        }

        return true;
    }

    private string GetContainerName(string blobPath)
    {
        var parts = blobPath.Split('/', 2);
        return parts[0];
    }

    private string GetBlobName(string blobPath)
    {
        var parts = blobPath.Split('/', 2);
        return parts.Length > 1 ? parts[1] : string.Empty;
    }
}