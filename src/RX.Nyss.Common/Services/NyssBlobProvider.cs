using System.Threading.Tasks;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Common.Services
{
    public interface INyssBlobProvider
    {
        Task<string> GetSmsContentResources();
    }

    public class NyssBlobProvider : INyssBlobProvider
    {
        private readonly IConfig<BlobConfig.ConnectionStringOptions> _config;
        private readonly BlobProvider _blobProvider;

        public NyssBlobProvider(IConfig<BlobConfig.ConnectionStringOptions> config)
        {
            _config = config;
            _blobProvider = new BlobProvider(config.GeneralBlobContainerName, config.ConnectionStrings.GeneralBlobContainer);
        }

        public async Task<string> GetSmsContentResources() =>
            await _blobProvider.GetBlobValue(_config.SmsContentResourcesBlobObjectName);
    }
}
