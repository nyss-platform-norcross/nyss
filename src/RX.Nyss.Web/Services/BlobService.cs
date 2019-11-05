using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Services
{
    public interface IBlobService
    {
        Task UpdateBlob(string blobObjectName, string content);
    }

    public class BlobService : IBlobService
    {
        private readonly IConfig _config;
        private readonly ILoggerAdapter _loggerAdapter;

        public BlobService(IConfig config, ILoggerAdapter loggerAdapter)
        {
            _config = config;
            _loggerAdapter = loggerAdapter;
        }

        public Task UpdateBlob(string blobObjectName, string content)
        {
            var storageAccountConnectionString = _config.ConnectionStrings.SmsGatewayBlobContainer;
            var smsGatewayBlobContainerName = _config.SmsGatewayBlobContainerName;
            
            if (string.IsNullOrWhiteSpace(smsGatewayBlobContainerName) ||
                string.IsNullOrWhiteSpace(blobObjectName) ||
                !CloudStorageAccount.TryParse(storageAccountConnectionString, out var storageAccount))
            {
                _loggerAdapter.Error("Unable to update authorized API keys. A configuration of a blob storage is not valid.");
                throw new ResultException(ResultKey.UnexpectedError);
            }

            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var smsGatewayContainer = cloudBlobClient.GetContainerReference(smsGatewayBlobContainerName);
            var authorizedApiKeysBlob = smsGatewayContainer.GetBlockBlobReference(blobObjectName);

            return authorizedApiKeysBlob.UploadTextAsync(content);
        }
    }
}
