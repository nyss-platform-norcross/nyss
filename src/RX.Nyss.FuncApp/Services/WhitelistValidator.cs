using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp.Services
{
    public interface IWhitelistValidator
    {
        bool IsWhitelistedEmailAddress(string whitelistedEmailAddresses, string email);
        bool IsWhiteListedPhoneNumber(string phoneNumber, string whitelistedPhoneNumbers);
    }

    public class WhitelistValidator : IWhitelistValidator
    {
        private readonly ILogger<WhitelistValidator> _logger;

        public WhitelistValidator(ILogger<WhitelistValidator> logger)
        {
            _logger = logger;
        }

        public bool IsWhitelistedEmailAddress(string whitelistedEmailAddresses, string email)
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

        public bool IsWhiteListedPhoneNumber(string phoneNumber, string whitelistedPhoneNumbers)
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
                _logger.Log(LogLevel.Information, $"{phoneNumber} found on the sms whitelist.");
            }
            else
            {
                _logger.LogWarning($"Phone number: {phoneNumber} not whitelisted.");
            }

            return isWhitelisted;
        }
    }
}
