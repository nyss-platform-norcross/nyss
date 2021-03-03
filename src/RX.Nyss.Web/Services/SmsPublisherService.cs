using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface ISmsPublisherService
    {
        Task SendSms(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage, bool specifyModem);
    }

    public class SmsPublisherService : ISmsPublisherService
    {
        private readonly QueueClient _queueClient;

        public SmsPublisherService(INyssWebConfig config)
        {
            _queueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.SendSmsQueue);
        }

        public async Task SendSms(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage, bool specifyModem) =>
            await Task.WhenAll(recipients.Select(recipient =>
            {
                var sendSms = new SendSmsMessage
                {
                    IotHubDeviceName = iotHubDeviceName,
                    PhoneNumber = recipient.PhoneNumber,
                    SmsMessage = smsMessage,
                    ModemNumber = recipient.Modem
                };

                var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendSms)))
                {
                    Label = "RX.Nyss.Web",
                    UserProperties = { { "IoTHubDevice", iotHubDeviceName } }
                };

                return _queueClient.SendAsync(message);
            }));
    }

    public class SendSmsMessage
    {
        public string IotHubDeviceName { get; set; }

        public string PhoneNumber { get; set; }

        public string SmsMessage { get; set; }
        public int? ModemNumber { get; set; }
    }

    public class SendSmsRecipient
    {
        public string PhoneNumber { get; set; }
        public int? Modem { get; set; }
    }
}
