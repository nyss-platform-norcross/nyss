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
    public class TelerivetReportPublisherServiceTests
    {
        private const string QueueName = "telerivet-report-queue";

        private readonly ServiceBusSender _senderMock;
        private readonly ITelerivetReportPublisherService _publisher;

        public TelerivetReportPublisherServiceTests()
        {
            _senderMock = Substitute.For<ServiceBusSender>();
            var serviceBusClientMock = Substitute.For<ServiceBusClient>();
            serviceBusClientMock.CreateSender(QueueName).Returns(_senderMock);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["SERVICEBUS_TELERIVETREPORTQUEUE"] = QueueName
                })
                .Build();

            _publisher = new TelerivetReportPublisherService(configuration, serviceBusClientMock);
        }

        [Fact]
        public async Task AddTelerivetReportToQueue_ShouldSendSerializedReportWithJsonContentType()
        {
            var report = new TelerivetReport
            {
                TimeCreated = "100",
                TimeUpdated = "101",
                MessageContent = "1#2",
                FromNumber = "+4712345678",
                ApiKey = "key",
                ProjectId = "proj",
                ReportSource = ReportSource.Telerivet
            };
            var expectedBody = JsonConvert.SerializeObject(report);

            await _publisher.AddTelerivetReportToQueue(report);

            await _senderMock.Received(1).SendMessageAsync(
                Arg.Is<ServiceBusMessage>(m =>
                    Encoding.UTF8.GetString(m.Body) == expectedBody &&
                    m.ContentType == "application/json"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddTelerivetReportToQueue_ShouldPreserveReportFields()
        {
            var report = new TelerivetReport
            {
                MessageContent = "payload",
                FromNumber = "+4799999999",
                ReportSource = ReportSource.Telerivet
            };

            ServiceBusMessage capturedMessage = null;
            await _senderMock.SendMessageAsync(Arg.Do<ServiceBusMessage>(m => capturedMessage = m));

            await _publisher.AddTelerivetReportToQueue(report);

            capturedMessage.ShouldNotBeNull();
            var published = JsonConvert.DeserializeObject<TelerivetReport>(Encoding.UTF8.GetString(capturedMessage.Body));
            published.MessageContent.ShouldBe("payload");
            published.FromNumber.ShouldBe("+4799999999");
            published.ReportSource.ShouldBe(ReportSource.Telerivet);
        }
    }
}
