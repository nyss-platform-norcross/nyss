using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface ISmsGatewayBlobProvider
    {
        Task UpdateApiKeys(string content);
    }

    public class SmsGatewayBlobProvider : ISmsGatewayBlobProvider
    {
        private readonly BlobProvider _blobProvider;
        private readonly INyssWebConfig _config;
        private readonly ILoggerAdapter _loggerAdapter;

        public SmsGatewayBlobProvider(INyssWebConfig config, ILoggerAdapter loggerAdapter, IAzureClientFactory<BlobServiceClient> azureClientFactory)
        {
            _config = config;
            _loggerAdapter = loggerAdapter;
            var blobServiceClient = azureClientFactory.CreateClient("SmsGatewayBlobClient");
            _blobProvider = new BlobProvider(blobServiceClient, config.SmsGatewayBlobContainerName);
        }

        public async Task UpdateApiKeys(string content)
        {
            try
            {
                await _blobProvider.SetBlobValue(_config.AuthorizedApiKeysBlobObjectName, content);
            }
            catch (ArgumentException e)
            {
                _loggerAdapter.Error("Unable to update authorized API keys: " + e.Message);
                throw new ResultException(ResultKey.UnexpectedError);
            }
        }
    }
}
