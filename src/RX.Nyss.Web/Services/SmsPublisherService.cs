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
        Task SendSms(string iotHubDeviceName, List<string> recipientPhoneNumbers, string smsMessage);
    }

    public class SmsPublisherService : ISmsPublisherService
    {
        private readonly QueueClient _queueClient;

        public SmsPublisherService(INyssWebConfig config)
        {
            _queueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.SendSmsQueue);
        }

        public async Task SendSms(string iotHubDeviceName, List<string> recipientPhoneNumbers, string smsMessage) =>
            await Task.WhenAll(recipientPhoneNumbers.Select(recipientPhoneNumber =>
            {
                var sendSms = new SendSmsMessage
                {
                    IotHubDeviceName = iotHubDeviceName,
                    PhoneNumber = recipientPhoneNumber,
                    SmsMessage = smsMessage
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
    }
}
