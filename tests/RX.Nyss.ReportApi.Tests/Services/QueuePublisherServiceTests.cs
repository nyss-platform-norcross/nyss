using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using NSubstitute;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Common;
using RX.Nyss.ReportApi.Services;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Services
{
    public class QueuePublisherServiceTests
    {
        private readonly IQueuePublisherService _queuePublisherService;
        private readonly ServiceBusClient _serviceBusClientMock;
        private readonly ServiceBusSender _emailQueueSenderMock;
        private readonly ServiceBusSender _smsQueueSenderMock;
        private readonly ServiceBusSender _checkAlertQueueSenderMock;
        private readonly ILoggerAdapter _loggerAdapterMock;

        public QueuePublisherServiceTests()
        {
            var nyssReportApiConfig = new ConfigSingleton
            {
                ServiceBusQueues = new ServiceBusQueuesOptions
                {
                    SendEmailQueue = "SendEmail",
                    CheckAlertQueue = "CheckAlert",
                    SendSmsQueue = "SendSms"
                }
            };
            _emailQueueSenderMock = Substitute.For<ServiceBusSender>();
            _smsQueueSenderMock = Substitute.For<ServiceBusSender>();
            _checkAlertQueueSenderMock = Substitute.For<ServiceBusSender>();

            _serviceBusClientMock = Substitute.For<ServiceBusClient>();
            _serviceBusClientMock.CreateSender("SendEmail").Returns(_emailQueueSenderMock);
            _serviceBusClientMock.CreateSender("CheckAlert").Returns(_checkAlertQueueSenderMock);
            _serviceBusClientMock.CreateSender("SendSms").Returns(_smsQueueSenderMock);

            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _queuePublisherService = new QueuePublisherService(
                nyssReportApiConfig,
                Substitute.For<IDateTimeProvider>(),
                _loggerAdapterMock,
                _serviceBusClientMock);
        }

        [Fact]
        public async Task SendSms_WhenMissingDeviceName_ShouldSendViaEmail()
        {
            var recipients = new List<SendSmsRecipient> { new SendSmsRecipient
                {
                    PhoneNumber = "+12345678"
                }
            };

            await _queuePublisherService.SendSms(recipients, new GatewaySetting { EmailAddress = "eagle@example.com" }, "This is a test");
            await _smsQueueSenderMock.DidNotReceive().SendMessageAsync(Arg.Any<ServiceBusMessage>());
            await _emailQueueSenderMock.Received(1).SendMessageAsync(Arg.Any<ServiceBusMessage>());
        }

        [Fact]
        public async Task SendSms_WhenIotDeviceNameSpecified_ShouldSendViaIotHub()
        {
            // Arrange
            var recipients = new List<SendSmsRecipient> { new SendSmsRecipient
                {
                    PhoneNumber = "+12345678"
                }
            };
            var gatewaySetting = new GatewaySetting
            {
                EmailAddress = "eagle@example.com",
                IotHubDeviceName = "TestDevice",
                Modems = new List<GatewayModem>()
            };

            // Act
            await _queuePublisherService.SendSms(recipients, gatewaySetting, "This is a test");


            // Assert
            var messageBody = JsonSerializer.Serialize(new SendSmsMessage
            {
                IotHubDeviceName = "TestDevice",
                PhoneNumber = "+12345678",
                SmsMessage = "This is a test"
            });

            await _smsQueueSenderMock.Received(1).SendMessageAsync(Arg.Is<ServiceBusMessage>(m => Encoding.UTF8.GetString(m.Body) == messageBody));
            await _emailQueueSenderMock.DidNotReceive().SendMessageAsync(Arg.Any<ServiceBusMessage>());
        }

        [Fact]
        public async Task SendSms_WhenEmailNorIotHubNotSpecified_ShouldNotSend()
        {
            // Arrange
            var recipients = new List<SendSmsRecipient> { new SendSmsRecipient
                {
                    PhoneNumber = "+12345678"
                }
            };

            // Act
            await _queuePublisherService.SendSms(recipients, new GatewaySetting{Name = "Missing gateway"}, "This is a test");


            // Assert
            await _smsQueueSenderMock.DidNotReceive().SendMessageAsync(Arg.Any<ServiceBusMessage>());
            await _emailQueueSenderMock.DidNotReceive().SendMessageAsync(Arg.Any<ServiceBusMessage>());
            _loggerAdapterMock.Received(1).Warn($"No email or IoT device found for gateway Missing gateway, not able to send feedback SMS!");
        }

        [Fact]
        public async Task SendSms_WhenGatewayUsesDualModem_ShouldSendMessageWithModemNumber()
        {
            // Arrange
            var recipients = new List<SendSmsRecipient> { new SendSmsRecipient
                {
                    PhoneNumber = "+12345678",
                    Modem = 1
                }
            };
            var gateway = new GatewaySetting
            {
                IotHubDeviceName = "iotdevice",
                Modems = new List<GatewayModem>
                {
                    new GatewayModem { ModemId = 1 },
                    new GatewayModem { ModemId = 2 }
                }
            };
            var smsMessage = new SendSmsMessage
            {
                IotHubDeviceName = "iotdevice",
                PhoneNumber = "+12345678",
                ModemNumber = 1,
                SmsMessage = "Feedback"
            };
            var messageJson = JsonSerializer.Serialize(smsMessage);
            var sendMessage = new ServiceBusMessage(messageJson);

            // Act
            await _queuePublisherService.SendSms(recipients, gateway, "Feedback");

            // Assert
            await _smsQueueSenderMock.Received(1).SendMessageAsync(Arg.Is<ServiceBusMessage>(m => m.Body.ToString() == messageJson));
        }
    }
}
