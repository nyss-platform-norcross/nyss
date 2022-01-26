using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface ISmsPublisherService
    {
        Task SendSms(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage);
    }

    public class SmsPublisherService : ISmsPublisherService
    {
        private readonly ServiceBusSender _sender;

        public SmsPublisherService(INyssWebConfig config, ServiceBusClient serviceBusClient)
        {
            _sender = serviceBusClient.CreateSender(config.ServiceBusQueues.SendSmsQueue);
        }

        public async Task SendSms(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage) =>
            await Task.WhenAll(recipients.Select(recipient =>
            {
                var sendSms = new SendSmsMessage
                {
                    IotHubDeviceName = iotHubDeviceName,
                    PhoneNumber = recipient.PhoneNumber,
                    SmsMessage = smsMessage,
                    ModemNumber = recipient.Modem
                };

                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendSms)))
                {
                    Subject = "RX.Nyss.Web",
                    ApplicationProperties = { {"IoTHubDevice", iotHubDeviceName } }
                };

                return _sender.SendMessageAsync(message);
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
