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
    public class ReportPublisherServiceTests
    {
        private const string ReportQueueName = "report-queue";

        private readonly ServiceBusSender _senderMock;
        private readonly IReportPublisherService _reportPublisherService;

        public ReportPublisherServiceTests()
        {
            _senderMock = Substitute.For<ServiceBusSender>();
            var serviceBusClientMock = Substitute.For<ServiceBusClient>();
            serviceBusClientMock.CreateSender(ReportQueueName).Returns(_senderMock);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["SERVICEBUS_REPORTQUEUE"] = ReportQueueName
                })
                .Build();

            _reportPublisherService = new ReportPublisherService(configuration, serviceBusClientMock);
        }

        [Fact]
        public async Task AddReportToQueue_ShouldSendSerializedReportWithJsonContentType()
        {
            // Arrange
            var report = new Report
            {
                Content = "sender=123&apikey=abc&text=1%232",
                ReportSource = ReportSource.Nyss
            };
            var expectedBody = JsonConvert.SerializeObject(report);

            // Act
            await _reportPublisherService.AddReportToQueue(report);

            // Assert
            await _senderMock.Received(1).SendMessageAsync(
                Arg.Is<ServiceBusMessage>(m =>
                    Encoding.UTF8.GetString(m.Body) == expectedBody &&
                    m.ContentType == "application/json"),
                Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(ReportSource.SmsEagle)]
        [InlineData(ReportSource.Telerivet)]
        [InlineData(ReportSource.SmsGateway)]
        [InlineData(ReportSource.MTNSmsGateway)]
        public async Task AddReportToQueue_ShouldPreserveReportSource(ReportSource reportSource)
        {
            // Arrange
            var report = new Report
            {
                Content = "payload",
                ReportSource = reportSource
            };

            ServiceBusMessage capturedMessage = null;
            await _senderMock.SendMessageAsync(Arg.Do<ServiceBusMessage>(m => capturedMessage = m));

            // Act
            await _reportPublisherService.AddReportToQueue(report);

            // Assert
            capturedMessage.ShouldNotBeNull();
            var publishedReport = JsonConvert.DeserializeObject<Report>(Encoding.UTF8.GetString(capturedMessage.Body));
            publishedReport.ReportSource.ShouldBe(reportSource);
            publishedReport.Content.ShouldBe("payload");
        }
    }
}
