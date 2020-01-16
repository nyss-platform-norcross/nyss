using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using RX.Nyss.ReportApi.Configuration;

namespace RX.Nyss.ReportApi.Services
{
    public interface IEmailToSmsPublisherService
    {
        Task SendMessages(string smsEagleEmailAddress, string smsEagleName, List<string> recipientPhoneNumbers, string body);
    }

    public class EmailToSmsPublisherService : IEmailToSmsPublisherService
    {
        private readonly IQueueClient _queueClient;

        public EmailToSmsPublisherService(INyssReportApiConfig config)
        {
            _queueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.SendEmailQueue);
        }

        public async Task SendMessages(string smsEagleEmailAddress, string smsEagleName, List<string> recipientPhoneNumbers, string body) =>
            await Task.WhenAll(recipientPhoneNumbers.Select(recipientPhoneNumber =>
            {
                var sendEmail = new SendEmailMessage
                {
                    To = new Contact { Email = smsEagleEmailAddress, Name = smsEagleName }, Body = body, Subject = recipientPhoneNumber, SendAsTextOnly = true
                };

                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendEmail))) { Label = "RX.Nyss.ReportApi", };

                return _queueClient.SendAsync(message);
            }));
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
