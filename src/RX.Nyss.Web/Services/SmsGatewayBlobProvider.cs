using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Services
{
    public interface ISmsGatewayBlobProvider
    {
        Task UpdateApiKeys(string content);
    }

    public class SmsGatewayBlobProvider : ISmsGatewayBlobProvider
    {
        private readonly BlobProvider _blobProvider;
        private readonly IConfig<NyssConfig.ConnectionStringOptions, NyssConfig.ServiceBusQueuesOptions> _config;
        private readonly ILoggerAdapter _loggerAdapter;

        public SmsGatewayBlobProvider(IConfig<NyssConfig.ConnectionStringOptions, NyssConfig.ServiceBusQueuesOptions> config, ILoggerAdapter loggerAdapter)
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
