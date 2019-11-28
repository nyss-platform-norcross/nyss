using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using RX.Nyss.Data;
using RX.Nyss.ReportApi.Configuration;

namespace RX.Nyss.ReportApi.Services
{
    public interface IEmailToSMSPublisherService
    {
        Task SendMessage(int smsEagleId, List<string> recipientPhoneNumbers, string body);
    }

    public class EmailToSMSPublisherService : IEmailToSMSPublisherService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IQueueClient _queueClient;

        public EmailToSMSPublisherService(INyssContext nyssContext, IConfig config)
        {
            _config = config;
            _nyssContext = nyssContext;
            _queueClient = new QueueClient(_config.ConnectionStrings.ServiceBus, _config.ServiceBusQueues.SendEmailQueue);
        }

        public async Task SendMessage(int smsEagleId, List<string> recipientPhoneNumbers, string body)
        {
            var smsEagle = await _nyssContext.GatewaySettings.FindAsync(smsEagleId);
            var recipients = string.Join(",", recipientPhoneNumbers);

            var sendEmail = new SendEmailMessage {To = new Contact{Email = smsEagle.EmailAddress, Name = smsEagle.Name}, Body = body, Subject = recipients};

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
    }

    public class Contact
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
