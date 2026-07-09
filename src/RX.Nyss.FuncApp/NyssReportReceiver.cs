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

public class NyssReportReceiver
{
    private const string _apiKeyQueryParameterName = "apikey";
    private readonly ILogger<NyssReportReceiver> _logger;
    private readonly IConfig _config;
    private readonly IReportPublisherService _reportPublisherService;
    private readonly BlobServiceClient _blobServiceClient;

    public NyssReportReceiver(
        ILogger<NyssReportReceiver> logger, 
        IConfig config, 
        IReportPublisherService reportPublisherService,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _config = config;
        _reportPublisherService = reportPublisherService;
        _blobServiceClient = blobServiceClient;
    }

    [Function("EnqueueNyssReport")]
    public async Task<HttpResponseData> EnqueueNyssReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueNyssReport")] HttpRequestData request)
    {
        if (request.Body.Length == 0)
        {
            _logger.LogWarning("Received a Nyss report with header content length null.");
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var httpRequestContent = await new StreamReader(request.Body).ReadToEndAsync();
        _logger.LogDebug($"Received Nyss report: {httpRequestContent}.{Environment.NewLine}HTTP request: {request}");

        if (string.IsNullOrWhiteSpace(httpRequestContent))
        {
            _logger.LogWarning("Received an empty Nyss report.");
            return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var decodedHttpRequestContent = HttpUtility.UrlDecode(httpRequestContent);

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

        var report = new Report
        {
            Content = httpRequestContent,
            ReportSource = ReportSource.Nyss
        };

        await _reportPublisherService.AddReportToQueue(report);
        return request.CreateResponse(HttpStatusCode.OK);
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
            _logger.LogWarning("Received a Nyss report with an empty API key.");
            return false;
        }

        if (!authorizedApiKeyList.Contains(apiKey))
        {
            _logger.LogWarning($"Received a Nyss report with not authorized API key: {apiKey}.");
            return false;
        }

        return true;
    }
}