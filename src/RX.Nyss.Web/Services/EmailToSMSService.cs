using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IEmailToSMSService
    {
        Task SendMessage(GatewaySetting gatewaySetting, List<string> recipientPhoneNumbers, string body);
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

        public async Task SendMessage(GatewaySetting gatewaySetting, List<string> recipientPhoneNumbers, string body)
        {
            var recipients = string.Join(",", recipientPhoneNumbers);
            await _emailPublisherService.SendEmail((gatewaySetting.EmailAddress, gatewaySetting.Name), recipients, body, true);
        }
    }
}
