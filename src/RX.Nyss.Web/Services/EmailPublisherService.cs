using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IEmailPublisherService
    {
        Task SendEmail((string email, string name) to, string subject, string body, bool sendAsTextOnly = false);
        Task SendEmailWithAttachment((string email, string name) to, string subject, string body, string filename);
        Task SendSmsAsEmail((string email, string name) to, IEnumerable<SendSmsRecipient> recipients, string body);
    }

    public class EmailPublisherService : IEmailPublisherService
    {
        private readonly ServiceBusSender _sender;

        public EmailPublisherService(INyssWebConfig config, ServiceBusClient serviceBusClient)
        {
            _sender = serviceBusClient.CreateSender(config.ServiceBusQueues.SendEmailQueue);
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

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail)))
            {
                Subject = "RX.Nyss.Web"
            };

            await _sender.SendMessageAsync(message);
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

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail)));

            await _sender.SendMessageAsync(message);
        }

        public async Task SendSmsAsEmail((string email, string name) to, IEnumerable<SendSmsRecipient> recipients, string body) =>
            await Task.WhenAll(recipients.Select(recipient =>
            {
                var sendEmail = new SendEmailMessage
                {
                    To = new Contact
                    {
                        Email = to.email,
                        Name = to.name
                    },
                    Body = body,
                    Subject = recipient.PhoneNumber,
                    SendAsTextOnly = true
                };

                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail))) { Subject = "RX.Nyss.Web" };

                return _sender.SendMessageAsync(message);
            }));
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
