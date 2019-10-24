using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IEmailPublisherService
    {
        Task SendEmail(string to, string subject, string body);
    }

    public class EmailPublisherService : IEmailPublisherService
    {
        private readonly IConfig _config;
        private readonly QueueClient _queueClient;

        public EmailPublisherService(IConfig config)
        {
            _config = config;
            _queueClient = new QueueClient(_config.ConnectionStrings.ServiceBus, _config.ServiceBusQueues.SendEmailQueue);
        }
        
        public async Task SendEmail(string to, string subject, string body)
        {
            var sendEmail = new SendEmailMessage {To = to, Body = body, Subject = subject,};

            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendEmail)))
            {
                Label = "RX.Nyss.Web",
            };
            
            await _queueClient.SendAsync(message);
        }
    }

    public class SendEmailMessage
    {
        public string To { get; set; }

        // Todo: Maybe it should be possible to optionally specify from email

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
