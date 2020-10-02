using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RX.Nyss.FuncApp.Services
{
    public class SendGridEmailClient : IEmailClient
    {
        private readonly IConfig _config;

        public SendGridEmailClient(IConfig config)
        {
            _config = config;
        }

        public async Task SendEmail(SendEmailMessage message, bool sandboxMode, CloudBlobContainer blobContainer)
        {
            var sendGridClient = new SendGridClient(_config.MailConfig.SendGrid.ApiKey);
            var sendGridMessage = CreateSendGridMessage(message: message, sandboxMode: sandboxMode, MimeType.Html);

            if (!string.IsNullOrEmpty(message.AttachmentFilename))
            {
                await AttachFile(sendGridMessage, message.AttachmentFilename, blobContainer);
            }

            await sendGridClient.SendEmailAsync(sendGridMessage).ConfigureAwait(false);
        }

        public async Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode)
        {
            var sendGridClient = new SendGridClient(_config.MailConfig.SendGrid.ApiKey);
            var sendGridMessage = CreateSendGridMessage(message: message, sandboxMode: sandboxMode, MimeType.Text);

            await sendGridClient.SendEmailAsync(sendGridMessage).ConfigureAwait(false);
        }

        private SendGridMessage CreateSendGridMessage(SendEmailMessage message, bool sandboxMode, string contentType)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_config.MailConfig.FromAddress, _config.MailConfig.FromName),
                Subject = message.Subject,
                MailSettings = new MailSettings
                {
                    SandboxMode = new SandboxMode
                    {
                        Enable = sandboxMode
                    }
                }
            };

            msg.AddTo(message.To.Email, message.To.Name);
            msg.AddContent(contentType, message.Body);
            return msg;
        }

        private async Task AttachFile(SendGridMessage message, string filename, CloudBlobContainer blobContainer)
        {
            var attachment = await DownloadAndEncodeAttachment(filename, blobContainer);
            message.AddAttachment(new Attachment
            {
                Content = attachment,
                Filename = filename,
                Type = "application/pdf"
            });
        }

        private async Task<string> DownloadAndEncodeAttachment(string filename, CloudBlobContainer blobContainer)
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
