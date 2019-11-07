using System.Threading.Tasks;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Services
{
    public interface INyssBlobProvider
    {
        Task<string> GetStringsResources();
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
    }
}
