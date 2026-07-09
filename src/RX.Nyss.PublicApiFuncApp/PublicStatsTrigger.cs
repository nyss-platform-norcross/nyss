using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace RX.Nyss.PublicApiFuncApp;

public class PublicStatsTrigger
{
    private readonly ILogger<PublicStatsTrigger> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public PublicStatsTrigger(ILogger<PublicStatsTrigger> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }


    [Function("Stats")]
    public async Task<HttpResponseData> Stats(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stats")] HttpRequestData httpRequest)
    {
         // Read azureWebJobsStorage from blob
        string azureWebJobsStorage;
        try
        {
            var azureWebJobsStorageBlobPath = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            if (string.IsNullOrWhiteSpace(azureWebJobsStorageBlobPath))
            {
                _logger.LogCritical("AzureWebJobsStorage environment variable is not set.");
                return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var blobClient = GetBlobClient(_blobServiceClient, azureWebJobsStorageBlobPath);
            
            var blobDownloadResult = await blobClient.DownloadContentAsync();
            azureWebJobsStorage = blobDownloadResult.Value.Content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read authorized API keys from blob storage.");
            return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
        }


        if (string.IsNullOrEmpty(azureWebJobsStorage))
        {
            _logger.LogCritical("AzureWebJobsStorage connection string is empty.");
            return httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
        }
        

        var response = httpRequest.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(azureWebJobsStorage);
        return response;
    }
      private BlobClient GetBlobClient(BlobServiceClient serviceClient, string blobPath)
    {
        var parts = blobPath.Split('/', 2);
        var containerName = parts[0];
        var blobName = parts.Length > 1 ? parts[1] : string.Empty;
        
        return serviceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
    }
}

