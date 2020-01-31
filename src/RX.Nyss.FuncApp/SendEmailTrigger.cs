using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;

namespace RX.Nyss.FuncApp
{
    public class SendEmailTrigger
    {
        private readonly IEmailService _emailService;

        public SendEmailTrigger(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [FunctionName("SendEmail")]
        public async Task SendEmail(
            [ServiceBusTrigger("%SERVICEBUS_SENDEMAILQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] SendEmailMessage message,
            [Blob("%WhitelistedPhoneNumbersBlobPath%", FileAccess.Read)] string whitelistedPhoneNumbers,
            [Blob("%WhitelistedEmailAddressesBlobPath%", FileAccess.Read)] string whitelistedEmailAddresses) =>
            await _emailService.SendEmailWithMailjet(message, whitelistedEmailAddresses, whitelistedPhoneNumbers);
    }
}
