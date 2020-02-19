using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace RX.Nyss.Common.Utils
{
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
            var blob = GetBlobReference(blobName);
            return await blob.DownloadTextAsync();
        }

        public async Task SetBlobValue(string blobName, string value)
        {
            var blob = GetBlobReference(blobName);
            await blob.UploadTextAsync(value);
        }

        public string GetBlobUrl(string blobName, TimeSpan lifeTime)
        {
            var blob = GetBlobReference(blobName);
            if (!blob.Exists())
            {
                return null;
            }

            var sasToken = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.Add(lifeTime)
            }, new SharedAccessBlobHeaders { ContentDisposition = $"inline; filename*=UTF-8''{blobName}" });

            var blobUrl = $"{blob.Uri}{sasToken}";
            return blobUrl;
        }

        private CloudBlockBlob GetBlobReference(string blobName)
        {
            if (string.IsNullOrWhiteSpace(_blobContainerName) ||
                string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("The configuration of blob is invalid.");
            }

            if (!CloudStorageAccount.TryParse(_storageAccountConnectionString, out var storageAccount))
            {
                throw new InvalidOperationException("Unable to get access to the blob container.");
            }

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_blobContainerName);
            return blobContainer.GetBlockBlobReference(blobName);
        }
    }
}
