using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Services
{
    public interface IEmailTextGeneratorService
    {
        Task<(string subject, string body)> GenerateResetPasswordEmail(string resetUrl, string name, string languageCode);

        Task<(string subject, string body)> GenerateEmailVerificationEmail(Role role, string callbackUrl, string name, string languageCode);

        Task<(string subject, string body)> GenerateEscalatedAlertEmail(string languageCode);

        Task<(string subject, string body)> GenerateEmailVerificationForDataConsumersEmail(Role role, string callbackUrl, string organizations, string name, string languageCode);

        Task<(string subject, string body)> GenerateAgreementDocumentEmail(string languageCode);
    }

    public class EmailTextGeneratorService : IEmailTextGeneratorService
    {
        private readonly IStringsResourcesService _stringsResourcesService;

        public EmailTextGeneratorService(IStringsResourcesService stringsResourcesService)
        {
            _stringsResourcesService = stringsResourcesService;
        }

        public async Task<(string subject, string body)> GenerateEscalatedAlertEmail(string languageCode)
        {
            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation(EmailContentKey.AlertEscalated.Subject, emailContents.Value);
            var body = GetTranslation(EmailContentKey.AlertEscalated.Body, emailContents.Value);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateResetPasswordEmail(string resetUrl, string name, string languageCode)
        {
            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation(EmailContentKey.ResetPassword.Subject, emailContents.Value);
            var body = GetTranslation(EmailContentKey.ResetPassword.Body, emailContents.Value);

            body = body
                .Replace("{{name}}", name)
                .Replace("{{resetUrl}}", resetUrl);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateEmailVerificationEmail(Role role, string callbackUrl, string name, string languageCode)
        {
            var strings = await _stringsResourcesService.GetStrings(languageCode);
            var roleName = strings.Get($"roles.{role.ToString().ToCamelCase()}");

            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation(EmailContentKey.EmailVerification.Subject, emailContents.Value);
            var body = GetTranslation(EmailContentKey.EmailVerification.Body, emailContents.Value);

            body = body
                .Replace("{{username}}", name)
                .Replace("{{roleName}}", roleName)
                .Replace("{{link}}", callbackUrl);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateEmailVerificationForDataConsumersEmail(Role role, string callbackUrl, string organizations, string name, string languageCode)
        {
            var strings = await _stringsResourcesService.GetStrings(languageCode);
            var roleName = strings.Get($"roles.{role.ToString().ToCamelCase()}");

            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation(EmailContentKey.EmailVerification.Subject, emailContents.Value);
            var body = GetTranslation(EmailContentKey.EmailVerification.DataConsumerBody, emailContents.Value);

            body = body
                .Replace("{{username}}", name)
                .Replace("{{roleName}}", roleName)
                .Replace("{{organizations}}", organizations)
                .Replace("{{link}}", callbackUrl);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateAgreementDocumentEmail(string languageCode)
        {
            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation(EmailContentKey.Consent.Subject, emailContents.Value);
            var body = GetTranslation(EmailContentKey.Consent.Body, emailContents.Value);

            return (subject, body);
        }

        private static string GetTranslation(string key, IDictionary<string, string> translations)
        {
            if (!translations.TryGetValue(key, out var value))
            {
                throw new Exception($"Could not find translations for {key}");
            }

            return value;
        }
    }
}
