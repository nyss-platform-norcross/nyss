using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using SendGrid.Helpers.Mail;

namespace RX.Nyss.FuncApp.Services;

public interface IEmailAttachmentService
{
    Task AttachPdf(SendGridMessage message, string filename, BlobContainerClient blobContainer);
}

public class EmailAttachmentService : IEmailAttachmentService
{
    public async Task AttachPdf(SendGridMessage message, string filename, BlobContainerClient blobContainer)
    {
        var blobClient = blobContainer.GetBlobClient(filename);
        await using var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);
        stream.Position = 0;
        await message.AddAttachmentAsync(filename, stream, "application/pdf");
        await stream.DisposeAsync();
    }
}