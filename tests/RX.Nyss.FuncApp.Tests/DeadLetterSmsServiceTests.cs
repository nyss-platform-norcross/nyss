using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.FuncApp.Tests
{
    public class DeadLetterSmsServiceTests
    {
        private const string SmsQueueName = "send-sms-queue";

        private readonly IConfig _config;
        private readonly ServiceBusReceiver _receiverMock;
        private readonly ServiceBusSender _senderMock;
        private readonly IDeadLetterSmsService _deadLetterSmsService;

        public DeadLetterSmsServiceTests()
        {
            _config = Substitute.For<IConfig>();
            _receiverMock = Substitute.For<ServiceBusReceiver>();
            _senderMock = Substitute.For<ServiceBusSender>();

            var serviceBusClientMock = Substitute.For<ServiceBusClient>();
            serviceBusClientMock
                .CreateReceiver(SmsQueueName, Arg.Any<ServiceBusReceiverOptions>())
                .Returns(_receiverMock);
            serviceBusClientMock.CreateSender(SmsQueueName).Returns(_senderMock);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["SERVICEBUS_SENDSMSQUEUE"] = SmsQueueName
                })
                .Build();

            _deadLetterSmsService = new DeadLetterSmsService(
                _config,
                configuration,
                Substitute.For<ILogger<DeadLetterSmsService>>(),
                serviceBusClientMock);
        }

        [Fact]
        public async Task ResubmitDeadLetterMessages_WhenResendingDisabled_ShouldReturnTrueWithoutTouchingQueues()
        {
            _config.EnableResendingFeedbackMessages.Returns(false);

            var result = await _deadLetterSmsService.ResubmitDeadLetterMessages();

            result.ShouldBeTrue();
            await _receiverMock.DidNotReceiveWithAnyArgs().ReceiveMessagesAsync(default, default);
            await _senderMock.DidNotReceiveWithAnyArgs().SendMessageAsync(default);
        }
    }
}
