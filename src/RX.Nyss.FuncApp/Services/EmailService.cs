using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
    public interface IEmailService
    {
        Task<HttpResponseMessage> SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SendEmailTrigger> _logger;
        private readonly IMailjetEmailClient _emailClient;

        public EmailService(ILogger<SendEmailTrigger> logger, IConfiguration config, IMailjetEmailClient emailClient)
        {
            _logger = logger;
            _config = config;
            _emailClient = emailClient;
        }

        public async Task<HttpResponseMessage> SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses)
        {
            if (!bool.TryParse(_config["MailjetSendToAll"], out var sendMailToAll))
            {
                _logger.LogWarning($"Failed parsing SendToAll config, will only send to whitelisted emails");
            }

            var sandboxMode = false;
            if (!sendMailToAll)
            {
                sandboxMode = !IsWhitelisted(whitelistedEmailAddresses, message.To.Email);
            }

            return await _emailClient.SendEmail(message, sandboxMode);
        }

        private bool IsWhitelisted(string whitelistedEmailAddresses, string email)
        {
            if (string.IsNullOrWhiteSpace(whitelistedEmailAddresses))
            {
                _logger.Log(LogLevel.Critical, "The email whitelist is empty.");
                return false;
            }

            var whitelist = whitelistedEmailAddresses.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var isWhitelisted = whitelist.Contains(email);
            if (isWhitelisted)
            {
                _logger.Log(LogLevel.Information, $"{email} found on the email address whitelist");
            }

            return isWhitelisted;
        }
    }
}
