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
            var subject = GetTranslation("email.alertEscalated.subject", emailContents.Value);
            var body = GetTranslation("email.alertEscalated.body", emailContents.Value);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateResetPasswordEmail(string resetUrl, string name, string languageCode)
        {
            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            if (!emailContents.Value.TryGetValue("email.reset.subject", out var subject))
            {
                throw new Exception($"Could not find translations for email.reset.subject with language: {languageCode}");
            }

            if (!emailContents.Value.TryGetValue("email.reset.body", out var body))
            {
                throw new Exception($"Could not find translations for email.reset.body with language: {languageCode}");
            }

            body = body
                .Replace("{{name}}", name)
                .Replace("{{resetUrl}}", resetUrl);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateEmailVerificationEmail(Role role, string callbackUrl, string name, string languageCode)
        {
            var stringTranslations = await _stringsResourcesService.GetStringsResources(languageCode);
            var roleName = GetTranslation($"roles.{role.ToString().ToCamelCase()}", stringTranslations.Value);

            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation("email.verification.subject", emailContents.Value);
            var body = GetTranslation("email.verification.body", emailContents.Value);

            body = body
                .Replace("{{username}}", name)
                .Replace("{{roleName}}", roleName)
                .Replace("{{link}}", callbackUrl);

            return (subject, body);
        }

        public async Task<(string subject, string body)> GenerateEmailVerificationForDataConsumersEmail(Role role, string callbackUrl, string organizations, string name, string languageCode)
        {
            var stringTranslations = await _stringsResourcesService.GetStringsResources(languageCode);
            var roleName = GetTranslation($"roles.{role.ToString().ToCamelCase()}", stringTranslations.Value);

            var emailContents = await _stringsResourcesService.GetEmailContentResources(languageCode);
            var subject = GetTranslation("email.verification.subject", emailContents.Value);
            var body = GetTranslation("email.dataConsumerVerification.body", emailContents.Value);

            body = body
                .Replace("{{username}}", name)
                .Replace("{{roleName}}", roleName)
                .Replace("{{organizations}}", organizations)
                .Replace("{{link}}", callbackUrl);

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
