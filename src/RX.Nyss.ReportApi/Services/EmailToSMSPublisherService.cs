using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using RX.Nyss.ReportApi.Configuration;

namespace RX.Nyss.ReportApi.Services
{
    public interface IEmailToSmsPublisherService
    {
        Task SendMessage(string smsEagleEmailAddress, string smsEagleName, List<string> recipientPhoneNumbers, string body);
    }

    public class EmailToSmsPublisherService : IEmailToSmsPublisherService
    {
        private readonly IConfig _config;
        private readonly IQueueClient _queueClient;

        public EmailToSmsPublisherService(IConfig config)
        {
            _config = config;
            _queueClient = new QueueClient(_config.ConnectionStrings.ServiceBus, _config.ServiceBusQueues.SendEmailQueue);
        }

        public async Task SendMessage(string smsEagleEmailAddress, string smsEagleName, List<string> recipientPhoneNumbers, string body)
        {
            var recipients = string.Join(",", recipientPhoneNumbers);

            var sendEmail = new SendEmailMessage {To = new Contact{Email = smsEagleEmailAddress, Name = smsEagleName}, Body = body, Subject = recipients, SendAsTextOnly = true};

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendEmail)))
            {
                Label = "RX.Nyss.ReportApi",
            };

            await _queueClient.SendAsync(message);
        }
    }

    public class SendEmailMessage
    {
        public Contact To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public bool SendAsTextOnly { get; set; }
    }

    public class Contact
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
