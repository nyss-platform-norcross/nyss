using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;

public interface IEmailClient
{
    Task SendEmail(SendEmailMessage message, bool sandboxMode, BlobContainerClient blobContainer);
    Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode);
}

public class DummyConsoleEmailClient : IEmailClient
{
    public async Task SendEmail(SendEmailMessage message, bool sandboxMode, BlobContainerClient blobContainer)
    {
#if DEBUG
        await Console.Out.WriteLineAsync($"To: {message.To.Name} <{message.To.Email}>");
        await Console.Out.WriteLineAsync($"Subject: {message.Subject}");
        await Console.Out.WriteLineAsync("Email body:");
        await Console.Out.WriteLineAsync(message.Body);
        if (!string.IsNullOrEmpty(message.AttachmentFilename))
        {
            await Console.Out.WriteLineAsync($"Attachment: {message.AttachmentFilename}");
        }
#else
            throw new System.Exception("Dummy methods are only available in debug builds!");
#endif
    }

    public async Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode)
    {
#if DEBUG
        await Console.Out.WriteLineAsync($"As text only to: {message.To.Name} <{message.To.Email}>");
        await Console.Out.WriteLineAsync($"Subject: {message.Subject}");
        await Console.Out.WriteLineAsync("Email body:");
        await Console.Out.WriteLineAsync(message.Body);
#else
            throw new System.Exception("Dummy methods are only available in debug builds!");
#endif
    }
}