using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
    public interface IEmailService
    {
        Task SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses, string whitelistedPhoneNumbers);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfig _config;
        private readonly IMailjetEmailClient _emailClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger, IConfig config, IMailjetEmailClient emailClient)
        {
            _logger = logger;
            _config = config;
            _emailClient = emailClient;
        }

        public async Task SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses, string whitelistedPhoneNumbers)
        {
            if (message.SendAsTextOnly)
            {
                if (_config.MailjetConfig.EnableFeedbackSms)
                {
                    var sandboxMode = !(_config.MailjetConfig.SendFeedbackSmsToAll || IsWhiteListedPhoneNumber(message.Subject, whitelistedPhoneNumbers));
                    await _emailClient.SendEmailAsTextOnly(message, sandboxMode);
                }
            }
            else
            {
                var sandboxMode = false;
                if (!_config.MailjetConfig.SendToAll)
                {
                    sandboxMode = !IsWhitelistedEmailAddress(whitelistedEmailAddresses, message.To.Email);
                }

                _logger.LogDebug($"Sending email to '{message.To.Email.Substring(0, Math.Min(message.To.Email.Length, 4))}...' SandboxMode: {sandboxMode}");
                await _emailClient.SendEmail(message, sandboxMode);
            }
        }

        private bool IsWhitelistedEmailAddress(string whitelistedEmailAddresses, string email)
        {
            if (string.IsNullOrWhiteSpace(whitelistedEmailAddresses))
            {
                _logger.Log(LogLevel.Critical, "The email whitelist is empty.");
                return false;
            }

            var whitelist = whitelistedEmailAddresses
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            var isWhitelisted = whitelist.Contains(email);
            if (isWhitelisted)
            {
                _logger.Log(LogLevel.Information, $"{email} found on the email address whitelist");
            }

            return isWhitelisted;
        }

        private bool IsWhiteListedPhoneNumber(string phoneNumber, string whitelistedPhoneNumbers)
        {
            if (string.IsNullOrWhiteSpace(whitelistedPhoneNumbers))
            {
                _logger.Log(LogLevel.Critical, "The sms whitelist is empty.");
                return false;
            }

            var whitelist = whitelistedPhoneNumbers
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());

            var isWhitelisted = whitelist.Contains(phoneNumber);
            if (isWhitelisted)
            {
                _logger.Log(LogLevel.Information, $"{phoneNumber} found on the sms whitelist");
            }
            else
            {
                _logger.LogWarning($"Phone number: {phoneNumber} not whitelisted. Skip sending email.");
            }

            return isWhitelisted;
        }
    }
}
