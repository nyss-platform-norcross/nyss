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
        private readonly INyssBlobProvider _blobProvider;
        private readonly IConfig _config;

        public EmailToSMSService(IEmailPublisherService emailPublisherService, INyssBlobProvider blobProvider, IConfig config)
        {
            _emailPublisherService = emailPublisherService;
            _blobProvider = blobProvider;
            _config = config;
        }

        public async Task SendMessage(GatewaySetting gatewaySetting, List<string> recipientPhoneNumbers, string body)
        {
            if (string.IsNullOrEmpty(gatewaySetting.EmailAddress)) return;
            recipientPhoneNumbers = _config.SendFeedbackToAll ? recipientPhoneNumbers : await RemoveWhitelistedPhoneNumbers(recipientPhoneNumbers);
            if (recipientPhoneNumbers.Count < 1)
            {
                return;
            }

            var recipients = string.Join(",", recipientPhoneNumbers);
            await _emailPublisherService.SendEmail((gatewaySetting.EmailAddress, gatewaySetting.Name), recipients, body, true);
        }

        public async Task<List<string>> RemoveWhitelistedPhoneNumbers(List<string> phoneNumbers)
        {
            var blob = await _blobProvider.GetWhitelistedPhoneNumbers();
            var whitelistedPhoneNumbers = blob.Split("\n");

            return phoneNumbers.Where(n => whitelistedPhoneNumbers.Contains(n)).ToList();
        }
    }
}
