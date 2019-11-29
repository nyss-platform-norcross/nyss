using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Data;

namespace RX.Nyss.Web.Services
{
    public interface IEmailToSMSService
    {
        Task SendMessage(int smsEagleId, List<string> recipientPhoneNumbers, string body);
    }

    public class EmailToSMSService : IEmailToSMSService
    {
        private readonly IEmailPublisherService _emailPublisherService;
        private INyssContext _nyssContext;

        public EmailToSMSService(INyssContext nyssContext, IEmailPublisherService emailPublisherService)
        {
            _nyssContext = nyssContext;
            _emailPublisherService = emailPublisherService;
        }

        public async Task SendMessage(int smsEagleId, List<string> recipientPhoneNumbers, string body)
        {
            var smsEagle = await _nyssContext.GatewaySettings.FindAsync(smsEagleId);
            if (string.IsNullOrEmpty(smsEagle.EmailAddress)) return;
            var recipients = string.Join(",", recipientPhoneNumbers);
            await _emailPublisherService.SendEmail((smsEagle.EmailAddress, smsEagle.Name), recipients, body, true);
        }
    }
}
