using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace RX.Nyss.FuncApp.Services
{
    public interface IEmailAttachmentService
    {
        Task<string> DownloadAndEncodeAttachment(string filename, CloudBlobContainer blobContainer);
    }

    public class EmailAttachmentService : IEmailAttachmentService
    {
        public async Task<string> DownloadAndEncodeAttachment(string filename, CloudBlobContainer blobContainer)
        {
            var attachment = blobContainer.GetBlockBlobReference(filename);
            using (var ms = new MemoryStream())
            {
                await attachment.DownloadToStreamAsync(ms);

                var byteArray = ms.ToArray();
                return Convert.ToBase64String(byteArray);
            }
        }
    }
}
