using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
            [ServiceBusTrigger("%SERVICEBUS_SENDEMAILQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")]SendEmailMessage message,
            [Blob("%WhitelistedEmailAddressesBlobPath%", FileAccess.Read)] string whitelistedEmailAddresses) =>
            await _emailService.SendEmailWithMailjet(message, whitelistedEmailAddresses);
    }
}
