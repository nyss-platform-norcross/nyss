﻿using System;
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
        Task<HttpResponseMessage> SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses);
    }

    public class EmailService : IEmailService
    {
        private readonly INyssFuncAppConfig _nyssFuncAppConfig;
        private readonly ILogger<EmailService> _logger;
        private readonly IMailjetEmailClient _emailClient;

        public EmailService(ILogger<EmailService> logger, INyssFuncAppConfig nyssFuncAppConfig, IMailjetEmailClient emailClient)
        {
            _logger = logger;
            _nyssFuncAppConfig = nyssFuncAppConfig;
            _emailClient = emailClient;
        }

        public async Task<HttpResponseMessage> SendEmailWithMailjet(SendEmailMessage message, string whitelistedEmailAddresses)
        {
            var sandboxMode = false;
            if (!_nyssFuncAppConfig.MailjetConfig.SendToAll)
            {
                sandboxMode = !IsWhitelisted(whitelistedEmailAddresses, message.To.Email);
            }

            _logger.LogDebug($"Sending email to '{message.To.Email.Substring(0, Math.Min(message.To.Email.Length, 4))}...' SandboxMode: {sandboxMode}");
            return await _emailClient.SendEmail(message, sandboxMode);
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
    }
}
