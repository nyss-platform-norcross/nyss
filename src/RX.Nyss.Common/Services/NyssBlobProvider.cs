using System.Threading.Tasks;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Common.Services
{
    public interface INyssBlobProvider
    {
        Task<string> GetStringsResources();
        Task SaveStringsResources(string value);
        Task<string> GetEmailContentResources();
        Task<string> GetSmsContentResources();
    }

    public class NyssBlobProvider : INyssBlobProvider
    {
        private readonly IConfig _config;
        private readonly BlobProvider _blobProvider;

        public NyssBlobProvider(IConfig config)
        {
            _config = config;
            _blobProvider = new BlobProvider(config.GeneralBlobContainerName, config.ConnectionStrings.GeneralBlobContainer);
        }

        public async Task<string> GetStringsResources() =>
            await _blobProvider.GetBlobValue(_config.StringsResourcesBlobObjectName);

        public async Task SaveStringsResources(string value) =>
            await _blobProvider.SetBlobValue(_config.StringsResourcesBlobObjectName, value);

        public async Task<string> GetEmailContentResources() =>
            await _blobProvider.GetBlobValue(_config.EmailContentResourcesBlobObjectName);

        public async Task<string> GetSmsContentResources() =>
            await _blobProvider.GetBlobValue(_config.SmsContentResourcesBlobObjectName);
    }
}
