using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using SendGrid.Helpers.Mail;

namespace RX.Nyss.FuncApp.Services
{
    public interface IEmailAttachmentService
    {
        Task AttachPdf(SendGridMessage message, string filename, CloudBlobContainer blobContainer);
    }

    public class EmailAttachmentService : IEmailAttachmentService
    {
        public async Task AttachPdf(SendGridMessage message, string filename, CloudBlobContainer blobContainer)
        {
            var attachment = blobContainer.GetBlockBlobReference(filename);
            using (var stream = new MemoryStream())
            {
                await attachment.DownloadToStreamAsync(stream);
                stream.Position = 0;
                await message.AddAttachmentAsync(filename, stream, "application/pdf");
            }
        }
    }
}
