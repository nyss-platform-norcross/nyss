using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;

namespace RX.Nyss.ReportApi.Services
{
    public interface IQueuePublisherService
    {
        Task QueueAlertCheck(int alertId);
        Task SendEmail((string Name, string EmailAddress) to, string emailSubject, string emailBody, bool sendAsTextOnly = false);
        Task SendSms(List<string> recipients, GatewaySetting gatewaySetting, string message);
    }

    public class QueuePublisherService : IQueuePublisherService
    {
        private readonly IQueueClient _sendEmailQueueClient;
        private readonly IQueueClient _checkAlertQueueClient;
        private readonly IQueueClient _sendSmsEmailQueueClient;
        private readonly INyssReportApiConfig _config;
        private readonly IDateTimeProvider _dateTimeProvider;

        public QueuePublisherService(INyssReportApiConfig config, IDateTimeProvider dateTimeProvider)
        {
            _config = config;
            _dateTimeProvider = dateTimeProvider;
            _sendEmailQueueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.SendEmailQueue);
            _checkAlertQueueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.CheckAlertQueue);
            _sendSmsEmailQueueClient = new QueueClient(config.ConnectionStrings.ServiceBus, config.ServiceBusQueues.SendSmsQueue);
        }

        public async Task SendSms(List<string> recipients, GatewaySetting gatewaySetting, string message)
        {
            if (string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
            {
                await SendSmsViaEmail(gatewaySetting.EmailAddress, gatewaySetting.Name, recipients, message);
            }

            await SendSmsViaIotHub(gatewaySetting.IotHubDeviceName, recipients, message);
        }

        public async Task QueueAlertCheck(int alertId)
        {
            var message = new Message(Encoding.UTF8.GetBytes(alertId.ToString()))
            {
                Label = "RX.Nyss.ReportApi",
                ScheduledEnqueueTimeUtc = _dateTimeProvider.UtcNow.AddMinutes(_config.CheckAlertTimeoutInMinutes)
            };

            await _checkAlertQueueClient.SendAsync(message);
        }

        public Task SendEmail((string Name, string EmailAddress) to, string emailSubject, string emailBody, bool sendAsTextOnly = false)
        {
            var sendEmail = new SendEmailMessage
            {
                To = new Contact
                {
                    Email = to.EmailAddress,
                    Name = to.Name
                },
                Body = emailBody,
                Subject = emailSubject,
                SendAsTextOnly = sendAsTextOnly
            };

            var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail))) { Label = "RX.Nyss.ReportApi" };

            return _sendEmailQueueClient.SendAsync(message);
        }

        private async Task SendSmsViaEmail(string smsEagleEmailAddress, string smsEagleName, List<string> recipientPhoneNumbers, string body) =>
            await Task.WhenAll(recipientPhoneNumbers.Select(recipientPhoneNumber =>
                SendEmail((smsEagleName, smsEagleEmailAddress), recipientPhoneNumber, body, true))
            );

        private async Task SendSmsViaIotHub(string iotHubDeviceName, List<string> recipientPhoneNumbers, string smsMessage) =>
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
                    Label = "RX.Nyss.ReportApi",
                    UserProperties = { { "IoTHubDevice", iotHubDeviceName } }
                };

                return _sendSmsEmailQueueClient.SendAsync(message);
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

    public class SendSmsMessage
    {
        public string IotHubDeviceName { get; set; }

        public string PhoneNumber { get; set; }

        public string SmsMessage { get; set; }
    }
}
