using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace RX.Nyss.Common.Utils;

public class BlobProvider
{
    private readonly string _blobContainerName;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobProvider(BlobServiceClient blobServiceClient, string blobContainerName)
    {
        _blobServiceClient = blobServiceClient;
        _blobContainerName = blobContainerName;
    }

    public async Task<string> GetBlobValue(string blobName)
    {
        var blob = await GetBlobClient(blobName);
        BlobDownloadResult content = await blob.DownloadContentAsync();
        return content.Content.ToString();
    }

    public async Task SetBlobValue(string blobName, string value, bool isStringResources = false)
    {
        await using var stream = new MemoryStream();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(value));
        stream.Position = 0;

        var blob = await GetBlobClient(blobName);
        await blob.UploadAsync(stream, true);

        if (isStringResources)
        {
            await blob.SetTagsAsync(new Dictionary<string, string> { { "type", "string-resource" } });
        }

        await stream.DisposeAsync();
    }

    public async Task<string> GetBlobUrl(string blobName, TimeSpan lifeTime)
    {
        var blob = await GetBlobClient(blobName);
        if (!await blob.ExistsAsync())
        {
            return null;
        }

        if (!blob.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Cannot generate SAS token for blob.");
        }

        var blobSasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(lifeTime));
        blobSasBuilder.ContentDisposition = $"inline; filename*=UTF-8''{blobName}";
        var sasUri = blob.GenerateSasUri(blobSasBuilder);

        var blobUrl = $"{sasUri}";
        return blobUrl;
    }

    public async Task<BlobProperties> GetBlobProperties(string blobName)
    {
        var blob = await GetBlobClient(blobName);
        if (!await blob.ExistsAsync())
        {
            return null;
        }

        return await blob.GetPropertiesAsync();
    }

    private async Task<BlobClient> GetBlobClient(string blobName)
    {
        if (string.IsNullOrWhiteSpace(_blobContainerName) ||
            string.IsNullOrWhiteSpace(blobName))
        {
            throw new ArgumentException("The configuration of blob is invalid.");
        }

        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

        if (!await blobContainerClient.ExistsAsync())
        {
            throw new InvalidOperationException("Blob container does not exist.");
        }

        return blobContainerClient.GetBlobClient(blobName);
    }
}