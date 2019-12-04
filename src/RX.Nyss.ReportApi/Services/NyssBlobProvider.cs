using System.Threading.Tasks;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Utils;

namespace RX.Nyss.ReportApi.Services
{
    public interface INyssBlobProvider
    {
        Task<string> GetWhiteListedPhoneNumbers();
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

        public async Task<string> GetWhiteListedPhoneNumbers() =>
            await _blobProvider.GetBlobValue(_config.WhitelistedPhoneNumbersBlobObjectName);
    }
}
