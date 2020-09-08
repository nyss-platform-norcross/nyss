using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Services
{
    public interface IEmailToSMSService
    {
        Task SendMessage(GatewaySetting gatewaySetting, List<string> recipientPhoneNumbers, string body);
    }

    public class EmailToSMSService : IEmailToSMSService
    {
        private readonly IEmailPublisherService _emailPublisherService;

        public EmailToSMSService(IEmailPublisherService emailPublisherService)
        {
            _emailPublisherService = emailPublisherService;
        }

        public async Task SendMessage(GatewaySetting gatewaySetting, List<string> recipientPhoneNumbers, string body)
        {
            if (string.IsNullOrEmpty(gatewaySetting.EmailAddress))
            {
                return;
            }

            await Task.WhenAll(recipientPhoneNumbers.Select(recipient =>
                _emailPublisherService.SendEmail((gatewaySetting.EmailAddress, gatewaySetting.Name), recipient, body, true)
            ));
        }
    }
}
