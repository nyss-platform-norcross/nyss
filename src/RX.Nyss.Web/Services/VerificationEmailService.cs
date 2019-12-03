using System;
using System.Net;
using System.Threading.Tasks;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IVerificationEmailService
    {
        Task SendVerificationEmail(User user, string securityStamp);
    }

    public class VerificationEmailService : IVerificationEmailService
    {
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;
        private readonly IConfig _config;

        public VerificationEmailService(IConfig config, IEmailPublisherService emailPublisherService, IEmailTextGeneratorService emailTextGeneratorService)
        {
            _config = config;
            _emailPublisherService = emailPublisherService;
            _emailTextGeneratorService = emailTextGeneratorService;
        }

        public async Task SendVerificationEmail(User user, string securityStamp)
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(user.EmailAddress)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();

            var (emailSubject, emailBody) = await _emailTextGeneratorService.GenerateEmailVerificationEmail(
                role:user.Role,
                callbackUrl: verificationUrl,
                name: user.Name,
                user.ApplicationLanguage.LanguageCode);

            await _emailPublisherService.SendEmail((user.EmailAddress, user.Name), emailSubject, emailBody);
        }
    }
}
