using System;
using System.Linq;
using System.Net.Http;
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
        private readonly ILogger<EmailService> _logger;
        private readonly IMailjetEmailClient _emailClient;

        public EmailService(ILogger<EmailService> logger, IConfig config, IMailjetEmailClient emailClient)
        {
            _logger = logger;
            _config = config;
            _emailClient = emailClient;
        }

        public async Task SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses, string whitelistedPhoneNumbers)
        {
            var sandboxMode = false;
            if (!_config.MailjetConfig.SendToAll)
            {
                sandboxMode = !IsWhitelisted(whitelistedEmailAddresses, message.To.Email);
            }

            _logger.LogDebug($"Sending email to '{message.To.Email.Substring(0, Math.Min(message.To.Email.Length, 4))}...' SandboxMode: {sandboxMode}");
            if (message.SendAsTextOnly)
            {
                if (!_config.MailjetConfig.SendFeedbackSmsToAll)
                {
                    message.Subject = RemoveNotWhitelistedPhoneNumbers(message.Subject, whitelistedPhoneNumbers);
                    if (string.IsNullOrEmpty(message.Subject))
                    {
                        _logger.LogWarning($"Phone number(s): {message.Subject} not whitelisted. Skip sending email.");
                        return;
                    }
                }
                await _emailClient.SendEmailAsTextOnly(message, sandboxMode);
                return;
            }
            await _emailClient.SendEmail(message, sandboxMode);
        }

        private bool IsWhitelisted(string whitelistedEmailAddresses, string email)
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

        private string RemoveNotWhitelistedPhoneNumbers(string phoneNumbers, string whitelistedPhoneNumbers)
        {
            var numbers = phoneNumbers.Split(",");
            var whitelisted = whitelistedPhoneNumbers.Split("\n");
            return String.Join(",", numbers.Where(number => whitelisted.Contains(number)).ToList());
        }
    }
}
