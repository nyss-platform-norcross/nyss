using System.Threading.Tasks;
using Azure.Storage.Blobs;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RX.Nyss.FuncApp.Services;

public class SendGridEmailClient : IEmailClient
{
    private readonly IConfig _config;
    private readonly IEmailAttachmentService _emailAttachmentService;

    public SendGridEmailClient(IConfig config, IEmailAttachmentService emailAttachmentService)
    {
        _config = config;
        _emailAttachmentService = emailAttachmentService;
    }

    public async Task SendEmail(SendEmailMessage message, bool sandboxMode, BlobContainerClient blobContainer)
    {
        var sendGridClient = new SendGridClient(_config.MailConfig.SendGrid.ApiKey);
        var sendGridMessage = CreateSendGridMessage(message: message, sandboxMode: sandboxMode, MimeType.Html);

        if (!string.IsNullOrEmpty(message.AttachmentFilename))
        {
            await _emailAttachmentService.AttachPdf(sendGridMessage, message.AttachmentFilename, blobContainer);
        }

        await sendGridClient.SendEmailAsync(sendGridMessage);
    }

    public async Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode)
    {
        var sendGridClient = new SendGridClient(_config.MailConfig.SendGrid.ApiKey);
        var sendGridMessage = CreateSendGridMessage(message: message, sandboxMode: sandboxMode, MimeType.Text);

        await sendGridClient.SendEmailAsync(sendGridMessage);
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
}