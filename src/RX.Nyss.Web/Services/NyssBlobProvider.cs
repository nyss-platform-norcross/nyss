using System.Threading.Tasks;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Services
{
    public interface INyssBlobProvider
    {
        Task<string> GetStringsResources();
        Task SaveStringsResources(string value);
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
    }
}
