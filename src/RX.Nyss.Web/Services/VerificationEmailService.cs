using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IVerificationEmailService
    {
        Task SendVerificationEmail(string email, string name, string securityStamp);
    }

    public class VerificationEmailService : IVerificationEmailService
    {
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IConfig _config;

        public VerificationEmailService(IConfig config, IEmailPublisherService emailPublisherService)
        {
            _config = config;
            _emailPublisherService = emailPublisherService;
        }

        public Task SendVerificationEmail(string email, string name, string securityStamp)
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();

            var (emailSubject, emailBody) = EmailTextGenerator.GenerateEmailVerificationEmail(
                role: Role.GlobalCoordinator.ToString(),
                callbackUrl: verificationUrl,
                name: name);

            return _emailPublisherService.SendEmail((email, name), emailSubject, emailBody);
        }
    }
}
