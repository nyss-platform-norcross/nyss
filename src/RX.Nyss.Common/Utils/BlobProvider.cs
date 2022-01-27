using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace RX.Nyss.Common.Utils;

public class BlobProvider
{
    private readonly string _blobContainerName;
    private readonly string _storageAccountConnectionString;

    public BlobProvider(string blobContainerName, string storageAccountConnectionString)
    {
        _blobContainerName = blobContainerName;
        _storageAccountConnectionString = storageAccountConnectionString;
    }

    public async Task<string> GetBlobValue(string blobName)
    {
        var blob = await GetBlobClient(blobName);
        BlobDownloadResult content = await blob.DownloadContentAsync();
        return content.Content.ToString();
    }

    public async Task SetBlobValue(string blobName, string value)
    {
        var blob = await GetBlobClient(blobName);
        await blob.UploadAsync(value);
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

    public async Task CopyBlob(string sourceUri, string to)
    {
        var newBlob = await GetBlobClient(to);
        if (await newBlob.ExistsAsync())
        {
            throw new Exception($"Blob with name {to} already exists!");
        }

        await newBlob.StartCopyFromUriAsync(new Uri(sourceUri));
    }

    private async Task<BlobClient> GetBlobClient(string blobName)
    {
        if (string.IsNullOrWhiteSpace(_blobContainerName) ||
            string.IsNullOrWhiteSpace(blobName))
        {
            throw new ArgumentException("The configuration of blob is invalid.");
        }

        var blobContainerClient = new BlobContainerClient(_storageAccountConnectionString, _blobContainerName);

        if (!await blobContainerClient.ExistsAsync())
        {
            throw new InvalidOperationException("Blob container does not exist.");
        }

        return blobContainerClient.GetBlobClient(blobName);
    }
}