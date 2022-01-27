using System.Threading.Tasks;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Common.Services;

public interface IDataBlobService
{
    Task StorePlatformAgreement(string sourceAgreement, string blobName);
    Task StorePublicStats(string stats);
}

public class DataBlobService : IDataBlobService
{
    private readonly IConfig _config;

    public DataBlobService(IConfig config)
    {
        _config = config;
    }

    public async Task StorePlatformAgreement(string sourceAgreement, string blobName)
    {
        var blobProvider = new BlobProvider(_config.PlatformAgreementsContainerName, _config.ConnectionStrings.DataBlobContainer);
        await blobProvider.CopyBlob(sourceAgreement, blobName);
    }

    public async Task StorePublicStats(string stats)
    {
        var blobProvider = new BlobProvider(_config.PublicStatsBlobContainerName, _config.ConnectionStrings.DataBlobContainer);
        await blobProvider.SetBlobValue(_config.PublicStatsBlobObjectName, stats);
    }
}