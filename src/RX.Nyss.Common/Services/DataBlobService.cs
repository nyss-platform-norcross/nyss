using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Common.Services;

public interface IDataBlobService
{
    Task StorePlatformAgreement(string sourceAgreement, string blobName);
    Task StorePublicStats(string stats);
}

public class DataBlobService : IDataBlobService
{
    private readonly IConfig _config;
    private readonly BlobServiceClient _dataBlobServiceClient;
    private readonly BlobServiceClient _generalBlobServiceClient;
    public DataBlobService(IConfig config, IAzureClientFactory<BlobServiceClient> azureClientFactory)
    {
        _config = config;
        _dataBlobServiceClient = azureClientFactory.CreateClient("DataBlobClient");
        _generalBlobServiceClient = azureClientFactory.CreateClient("GeneralBlobClient");
    }

    public async Task StorePlatformAgreement(string languageCode, string blobName)
    {
        var sourceBlobContainerClient = _generalBlobServiceClient.GetBlobContainerClient(_config.GeneralBlobContainerName);
        var destinationBlobContainerClient = _dataBlobServiceClient.GetBlobContainerClient(_config.PlatformAgreementsContainerName);
        var sourceBlob = sourceBlobContainerClient.GetBlobClient(_config.PlatformAgreementBlobObjectName.Replace("{languageCode}", languageCode));

        if (!await sourceBlob.ExistsAsync())
        {
            throw new Exception("Platform agreement blob does not exist");
        }

        var destinationBlob = destinationBlobContainerClient.GetBlobClient(blobName);
        BlobDownloadStreamingResult sourceStream = await sourceBlob.DownloadStreamingAsync();
        await destinationBlob.UploadAsync(sourceStream.Content);

        sourceStream.Dispose();
    }

    public async Task StorePublicStats(string stats)
    {
        var blobProvider = new BlobProvider(_dataBlobServiceClient, _config.PublicStatsBlobContainerName);
        await blobProvider.SetBlobValue(_config.PublicStatsBlobObjectName, stats);
    }
}