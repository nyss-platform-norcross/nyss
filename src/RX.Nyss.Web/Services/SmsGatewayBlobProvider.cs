using System;
using System.Threading.Tasks;
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

        public SmsGatewayBlobProvider(INyssWebConfig config, ILoggerAdapter loggerAdapter)
        {
            _config = config;
            _loggerAdapter = loggerAdapter;
            _blobProvider = new BlobProvider(config.SmsGatewayBlobContainerName, config.ConnectionStrings.SmsGatewayBlobContainer);
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
