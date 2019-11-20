using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Services
{
    public interface IEmailToSMSService
    {
        Task SendMessage(string smsEagleApiKey, List<string> recipientPhoneNumbers, string body);
    }

    public class EmailToSMSService : IEmailToSMSService
    {
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IConfig _config;

        public EmailToSMSService(IEmailPublisherService emailPublisherService, IConfig config)
        {
            _emailPublisherService = emailPublisherService;
            _config = config;
        }

        public async Task SendMessage(string smsEagleApiKey, List<string> recipientPhoneNumbers, string body)
        {
            var address = $"{smsEagleApiKey}@{_config.EmailToSMSDomain}";
            var recipients = string.Join(",", recipientPhoneNumbers);
            await _emailPublisherService.SendEmail((address, smsEagleApiKey), recipients, body);
        }
    }
}
