using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IEmailPublisherService
    {
        Task SendEmail((string email, string name) to, string subject, string body, bool sendAsTextOnly = false);
    }

    public class EmailPublisherService : IEmailPublisherService
    {
        private readonly QueueClient _queueClient;

        public EmailPublisherService(INyssWebConfig config)
        {
            var config1 = config;
            _queueClient = new QueueClient(config1.ConnectionStrings.ServiceBus, config1.ServiceBusQueues.SendEmailQueue);
        }

        public async Task SendEmail((string email, string name) to, string subject, string body, bool sendAsTextOnly = false)
        {
            var sendEmail = new SendEmailMessage {To = new Contact{Email = to.email, Name = to.name}, Body = body, Subject = subject, SendAsTextOnly = sendAsTextOnly};

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendEmail)))
            {
                Label = "RX.Nyss.Web",
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
