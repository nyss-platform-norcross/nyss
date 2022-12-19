using System;
using System.Net;
using System.Threading.Tasks;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IVerificationEmailService
    {
        Task SendVerificationEmail(User user, string securityStamp);
        Task SendVerificationForDataConsumersEmail(User user, string organizations, string securityStamp);
    }

    public class VerificationEmailService : IVerificationEmailService
    {
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;
        private readonly INyssWebConfig _config;

        public VerificationEmailService(INyssWebConfig config, IEmailPublisherService emailPublisherService, IEmailTextGeneratorService emailTextGeneratorService)
        {
            _config = config;
            _emailPublisherService = emailPublisherService;
            _emailTextGeneratorService = emailTextGeneratorService;
        }

        public async Task SendVerificationEmail(User user, string securityStamp)
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(user.EmailAddress)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();
            if (user.ApplicationLanguage == null)
            {
                var lang = new ApplicationLanguage();
                lang.SetDisplayLanguageId(1);
                lang.SetDisplayLanguageCode("en");
                lang.SetDisplayLanguageName("English");
                user.ApplicationLanguage = lang;
            }
            var (emailSubject, emailBody) = await _emailTextGeneratorService.GenerateEmailVerificationEmail(
                user.Role,
                verificationUrl,
                user.Name,
                user.ApplicationLanguage.LanguageCode);

            await _emailPublisherService.SendEmail((user.EmailAddress, user.Name), emailSubject, emailBody);
        }

        public async Task SendVerificationForDataConsumersEmail(User user, string organizations, string securityStamp)
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(user.EmailAddress)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();
            if (user.ApplicationLanguage == null)
            {
                var lang = new ApplicationLanguage();
                lang.SetDisplayLanguageId(1);
                lang.SetDisplayLanguageCode("en");
                lang.SetDisplayLanguageName("English");
                user.ApplicationLanguage = lang;
            }
            var (emailSubject, emailBody) = await _emailTextGeneratorService.GenerateEmailVerificationForDataConsumersEmail(
                user.Role,
                verificationUrl,
                organizations,
                user.Name,
                user.ApplicationLanguage.LanguageCode);

            await _emailPublisherService.SendEmail((user.EmailAddress, user.Name), emailSubject, emailBody);
        }
    }
}
