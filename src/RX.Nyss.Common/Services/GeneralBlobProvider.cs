using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Common.Services
{
    public interface IGeneralBlobProvider
    {
        Task<string> GetStringsResources();
        Task SaveStringsResources(string value);
        Task<string> GetEmailContentResources();
        Task<string> GetSmsContentResources();
        string GetPlatformAgreementUrl(string languageCode);
    }

    public class GeneralBlobProvider : IGeneralBlobProvider
    {
        private readonly IConfig _config;
        private readonly BlobProvider _blobProvider;

        public GeneralBlobProvider(IConfig config)
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

        public string GetPlatformAgreementUrl(string languageCode) =>
            _blobProvider.GetBlobUrl(_config.PlatformAgreementBlobObjectName.Replace("{languageCode}", languageCode), TimeSpan.FromHours(1));
    }
}
