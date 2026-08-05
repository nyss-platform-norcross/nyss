using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.FuncApp.Tests
{
    public class MTNReportPublisherServiceTests
    {
        private const string QueueName = "mtn-report-queue";

        private readonly ServiceBusSender _senderMock;
        private readonly IMTNReportPublisherService _publisher;

        public MTNReportPublisherServiceTests()
        {
            _senderMock = Substitute.For<ServiceBusSender>();
            var serviceBusClientMock = Substitute.For<ServiceBusClient>();
            serviceBusClientMock.CreateSender(QueueName).Returns(_senderMock);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["SERVICEBUS_MTNREPORTQUEUE"] = QueueName
                })
                .Build();

            _publisher = new MTNReportPublisherService(configuration, serviceBusClientMock);
        }

        [Fact]
        public async Task AddMTNReportToQueue_ShouldSendSerializedReportWithJsonContentType()
        {
            var report = new MTNReport
            {
                SenderAddress = "+4711111111",
                ReceiverAddress = "+4722222222",
                SubmittedDate = "2026-01-01",
                Message = "1#2",
                Created = "2026-01-01T00:00:00Z",
                Id = "tx-1",
                ReportSource = ReportSource.MTNSmsGateway
            };
            var expectedBody = JsonConvert.SerializeObject(report);

            await _publisher.AddMTNReportToQueue(report);

            await _senderMock.Received(1).SendMessageAsync(
                Arg.Is<ServiceBusMessage>(m =>
                    Encoding.UTF8.GetString(m.Body) == expectedBody &&
                    m.ContentType == "application/json"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddMTNReportToQueue_ShouldPreserveReportFields()
        {
            var report = new MTNReport
            {
                Message = "payload",
                Id = "tx-42",
                ReportSource = ReportSource.MTNSmsGateway
            };

            ServiceBusMessage capturedMessage = null;
            await _senderMock.SendMessageAsync(Arg.Do<ServiceBusMessage>(m => capturedMessage = m));

            await _publisher.AddMTNReportToQueue(report);

            capturedMessage.ShouldNotBeNull();
            var published = JsonConvert.DeserializeObject<MTNReport>(Encoding.UTF8.GetString(capturedMessage.Body));
            published.Message.ShouldBe("payload");
            published.Id.ShouldBe("tx-42");
            published.ReportSource.ShouldBe(ReportSource.MTNSmsGateway);
        }
    }
}
