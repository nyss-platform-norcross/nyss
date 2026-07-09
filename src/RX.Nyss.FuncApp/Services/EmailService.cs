using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;

public interface IEmailService
{
    Task SendEmail(SendEmailMessage message, string whitelistedEmailAddresses, string whitelistedPhoneNumbers, BlobContainerClient blobContainerClient);
}

public class EmailService : IEmailService
{
    private readonly IConfig _config;
    private readonly IEmailClient _emailClient;
    private readonly ILogger<EmailService> _logger;
    private readonly IWhitelistValidator _whitelistValidator;

    public EmailService(ILogger<EmailService> logger, IConfig config, IEmailClient emailClient, IWhitelistValidator whitelistValidator)
    {
        _logger = logger;
        _config = config;
        _emailClient = emailClient;
        _whitelistValidator = whitelistValidator;
    }

    public async Task SendEmail(SendEmailMessage message, string whitelistedEmailAddresses, string whitelistedPhoneNumbers, BlobContainerClient blobContainerClient)
    {
        if (message.SendAsTextOnly)
        {
            if (_config.MailConfig.EnableFeedbackSms)
            {
                var sandboxMode = !(_config.MailConfig.SendFeedbackSmsToAll || _whitelistValidator.IsWhiteListedPhoneNumber(whitelistedPhoneNumbers, message.Subject));
                await _emailClient.SendEmailAsTextOnly(message, sandboxMode);
            }
        }
        else
        {
            var sandboxMode = false;
            if (!_config.MailConfig.SendToAll)
            {
                sandboxMode = !_whitelistValidator.IsWhitelistedEmailAddress(whitelistedEmailAddresses, message.To.Email);
            }

            _logger.LogDebug($"Sending email to '{message.To.Email.Substring(0, Math.Min(message.To.Email.Length, 4))}...' SandboxMode: {sandboxMode}");
            await _emailClient.SendEmail(message, sandboxMode, blobContainerClient);
        }
    }
}