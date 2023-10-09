using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Common;
using Telerivet.Client;


namespace RX.Nyss.ReportApi.Services
{
    public interface IQueuePublisherService
    {
        Task QueueAlertCheck(int alertId);
        Task SendEmail((string Name, string EmailAddress) to, string emailSubject, string emailBody, bool sendAsTextOnly = false);
        Task SendSms(List<SendSmsRecipient> recipients, GatewaySetting gatewaySetting, string message);
        Task SendTelerivetSms(long number, string message);
    }

    public class QueuePublisherService : IQueuePublisherService
    {
        private readonly ServiceBusSender _sendEmailQueueSender;
        private readonly ServiceBusSender _checkAlertQueueSender;
        private readonly ServiceBusSender _sendSmsQueueSender;
        private readonly INyssReportApiConfig _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILoggerAdapter _loggerAdapter;

        public QueuePublisherService(INyssReportApiConfig config, IDateTimeProvider dateTimeProvider, ILoggerAdapter loggerAdapter, ServiceBusClient serviceBusClient)
        {
            _config = config;
            _dateTimeProvider = dateTimeProvider;
            _loggerAdapter = loggerAdapter;
            _sendEmailQueueSender = serviceBusClient.CreateSender(config.ServiceBusQueues.SendEmailQueue);
            _checkAlertQueueSender = serviceBusClient.CreateSender(config.ServiceBusQueues.CheckAlertQueue);
            _sendSmsQueueSender = serviceBusClient.CreateSender(config.ServiceBusQueues.SendSmsQueue);
        }

        public async Task SendSms(List<SendSmsRecipient> recipients, GatewaySetting gatewaySetting, string message)
        {
            //long number = 004798425349;
            if (!string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
            {
                var specifyModemWhenSending = gatewaySetting.Modems.Any();
                await SendSmsViaIotHub(gatewaySetting.IotHubDeviceName, recipients, message, specifyModemWhenSending);
            }
            else if (gatewaySetting.GatewayType == GatewayType.Telerivet)
            {
                _loggerAdapter.Info($"This is logger {gatewaySetting.EmailAddress}, {gatewaySetting.GatewayType}, {recipients.ToString()}, {message}");
                await SendTelerivetSms(98425349, message);
            }
            else if (!string.IsNullOrEmpty(gatewaySetting.EmailAddress))
            {
                await SendSmsViaEmail(gatewaySetting.EmailAddress, gatewaySetting.Name, recipients.Select(r => r.PhoneNumber).ToList(), message);
            }
            else
            {
                _loggerAdapter.Warn($"No email or IoT device found for gateway {gatewaySetting.Name}, not able to send feedback SMS!");
            }
        }

        public async Task QueueAlertCheck(int alertId)
        {
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(alertId.ToString()))
            {
                Subject = "RX.Nyss.ReportApi",
                ScheduledEnqueueTime = _dateTimeProvider.UtcNow.AddMinutes(_config.CheckAlertTimeoutInMinutes)
            };

            await _checkAlertQueueSender.SendMessageAsync(message);
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

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendEmail)));

            return _sendEmailQueueSender.SendMessageAsync(message);
        }

        private async Task SendSmsViaEmail(string smsEagleEmailAddress, string smsEagleName, List<string> recipientPhoneNumbers, string body) =>
            await Task.WhenAll(recipientPhoneNumbers.Select(recipientPhoneNumber =>
                SendEmail((smsEagleName, smsEagleEmailAddress), recipientPhoneNumber, body, true))
            );

        private async Task SendSmsViaIotHub(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage, bool specifyModemWhenSending) =>
            await Task.WhenAll(recipients.Select(recipient =>
            {
                var sendSms = new SendSmsMessage
                {
                    IotHubDeviceName = iotHubDeviceName,
                    PhoneNumber = recipient.PhoneNumber,
                    SmsMessage = smsMessage,
                    ModemNumber = specifyModemWhenSending
                        ? recipient.Modem
                        : null
                };

                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendSms)))
                {
                    Subject = "RX.Nyss.ReportApi",
                    ApplicationProperties = { { "IotHubDevice", iotHubDeviceName } }
                };

                return _sendSmsQueueSender.SendMessageAsync(message);
            }));

        public async Task SendTelerivetSms(long number, string message)
        {
            var tr = new TelerivetAPI("_iN2i_VdAJFTIm5BYMzFmkANujPlTyFeENW0");
                    var project = tr.InitProjectById("PJc4c294a93b50ec5c");


                    // send message
                    Message sent_msg = await project.SendMessageAsync(Util.Options(
                        "content", message,
                        "to_number", number
                    ));
                }
    };
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

        public int? ModemNumber { get; set; }
    }
}
