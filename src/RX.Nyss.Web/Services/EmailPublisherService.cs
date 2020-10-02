using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IEmailPublisherService
    {
        Task SendEmail((string email, string name) to, string subject, string body, bool sendAsTextOnly = false);
        Task SendEmailWithAttachment((string email, string name) to, string subject, string body, string filename);
    }

    public class EmailPublisherService : IEmailPublisherService
    {
        private readonly QueueClient _queueClient;

        public EmailPublisherService(INyssWebConfig config)
        {
            _queueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.SendEmailQueue);
        }

        public async Task SendEmail((string email, string name) to, string subject, string body, bool sendAsTextOnly = false)
        {
            var sendEmail = new SendEmailMessage
            {
                To = new Contact
                {
                    Email = to.email,
                    Name = to.name
                },
                Body = body,
                Subject = subject,
                SendAsTextOnly = sendAsTextOnly
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail))) { Label = "RX.Nyss.Web" };

            await _queueClient.SendAsync(message);
        }

        public async Task SendEmailWithAttachment((string email, string name) to, string subject, string body, string filename)
        {
            var sendEmail = new SendEmailMessage
            {
                To = new Contact
                {
                    Email = to.email,
                    Name = to.name
                },
                Body = body,
                Subject = subject,
                AttachmentFilename = filename
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail))) { Label = "RX.Nyss.Web" };

            await _queueClient.SendAsync(message);
        }
    }

    public class SendEmailMessage
    {
        public Contact To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string AttachmentFilename { get; set; }

        public bool SendAsTextOnly { get; set; }
    }

    public class Contact
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
