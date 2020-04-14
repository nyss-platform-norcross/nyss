using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using NSubstitute;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Services;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Services
{
    public class QueuePublisherServiceTests
    {
        private readonly IQueuePublisherService _queuePublisherService;
        private readonly IQueueClientProvider _queueClientProviderMock;
        private readonly IQueueClient _emailQueueClientMock;
        private readonly IQueueClient _smsQueueClientMock;
        private readonly IQueueClient _checkAlertQueueClientMock;
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
            _emailQueueClientMock = Substitute.For<IQueueClient>();
            _smsQueueClientMock = Substitute.For<IQueueClient>();
            _checkAlertQueueClientMock = Substitute.For<IQueueClient>();

            _queueClientProviderMock = Substitute.For<IQueueClientProvider>();
            _queueClientProviderMock.GetClient("SendEmail").Returns(_emailQueueClientMock);
            _queueClientProviderMock.GetClient("CheckAlert").Returns(_checkAlertQueueClientMock);
            _queueClientProviderMock.GetClient("SendSms").Returns(_smsQueueClientMock);

            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _queuePublisherService = new QueuePublisherService(
                nyssReportApiConfig,
                Substitute.For<IDateTimeProvider>(),
                _loggerAdapterMock,
                _queueClientProviderMock);
        }

        [Fact]
        public async Task SendSms_WhenMissingDeviceName_ShouldSendViaEmail()
        {
            var recipients = new List<string> { "+12345678" };

            await _queuePublisherService.SendSms(recipients, new GatewaySetting { EmailAddress = "eagle@example.com" }, "This is a test");
            await _smsQueueClientMock.DidNotReceive().SendAsync(Arg.Any<Message>());
            await _emailQueueClientMock.Received(1).SendAsync(Arg.Any<Message>());
        }

        [Fact]
        public async Task SendSms_WhenIotDeviceNameSpecified_ShouldSendViaIotHub()
        {
            // Arrange
            var recipients = new List<string> { "+12345678" };
            
            // Act
            await _queuePublisherService.SendSms(recipients, new GatewaySetting { EmailAddress = "eagle@example.com", IotHubDeviceName = "TestDevice"}, "This is a test");


            // Assert
            var messageBody = JsonSerializer.Serialize(new SendSmsMessage
            {
                IotHubDeviceName = "TestDevice",
                PhoneNumber = "+12345678",
                SmsMessage = "This is a test"
            });

            await _smsQueueClientMock.Received(1).SendAsync(Arg.Is<Message>(m => Encoding.UTF8.GetString(m.Body) == messageBody));
            await _emailQueueClientMock.DidNotReceive().SendAsync(Arg.Any<Message>());
        }

        [Fact]
        public async Task SendSms_WhenEmailNorIotHubNotSpecified_ShouldNotSend()
        {
            // Arrange
            var recipients = new List<string> { "+12345678" };
            
            // Act
            await _queuePublisherService.SendSms(recipients, new GatewaySetting{Name = "Missing gateway"}, "This is a test");


            // Assert
            await _smsQueueClientMock.DidNotReceive().SendAsync(Arg.Any<Message>());
            await _emailQueueClientMock.DidNotReceive().SendAsync(Arg.Any<Message>());
            _loggerAdapterMock.Received(1).Warn($"No email or IoT device found for gateway Missing gateway, not able to send feedback SMS!");
        }
    }
}
