using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Services;
using RX.Nyss.ReportApi.Tests.Features.Alert.TestData;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Alert
{
    public class AlertNotificationServiceTests
    {
        private readonly IAlertNotificationService _alertNotificationService;

        private readonly INyssContext _nyssContextMock;
        private readonly INyssContext _nyssContextInMemoryMock;
        private readonly IDateTimeProvider _dateTimeProviderMock;
        private readonly IQueuePublisherService _queuePublisherServiceMock;
        private readonly ILoggerAdapter _loggerAdapterMock;
        private readonly IStringsResourcesService _stringsResourcesServiceMock;
        private readonly INyssReportApiConfig _configMock;

        private readonly AlertServiceTestData _testData;

        public AlertNotificationServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
            _stringsResourcesServiceMock = Substitute.For<IStringsResourcesService>();
            _queuePublisherServiceMock = Substitute.For<IQueuePublisherService>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _configMock = Substitute.For<INyssReportApiConfig>();

            var builder = new DbContextOptionsBuilder<NyssContext>();
            builder.UseInMemoryDatabase("AlertNotificationTestsDatabaseInMemory");
            _nyssContextInMemoryMock = new NyssContext(builder.Options);

            _testData = new AlertServiceTestData(_nyssContextMock);

            _alertNotificationService = new AlertNotificationService(
                    _nyssContextMock,
                    _queuePublisherServiceMock,
                    _stringsResourcesServiceMock,
                    _configMock,
                    _loggerAdapterMock,
                    _dateTimeProviderMock);
        }

        [Fact]
        public async Task CheckAlert_WhenAlertIsStillPending_ShouldSendAlert()
        {
            // arrange
            _testData.SimpleCasesData.GenerateData().AddToDbContext();

            // act
            await _alertNotificationService.EmailAlertNotHandledRecipientsIfAlertIsPending(1);

            // assert
            await _queuePublisherServiceMock.DidNotReceiveWithAnyArgs().SendEmail((null, null), null, null);
        }

        [Fact]
        public async Task CheckAlert_WhenAlertNotFound_ShouldDoNothing()
        {
            // arrange
            _testData.SimpleCasesData.GenerateData().AddToDbContext();

            // act
            await _alertNotificationService.EmailAlertNotHandledRecipientsIfAlertIsPending(1);

            // assert
            await _queuePublisherServiceMock.DidNotReceiveWithAnyArgs().SendEmail((null, null), null, null);
        }

        [Fact]
        public async Task SendNotificationsForNewAlert_ShouldSendSmsToAllSupervisorsAndQueueCheckBack()
        {
            // arrange
            var testData = new AlertServiceTestData(_nyssContextInMemoryMock);
            testData.WhenAnAlertAreTriggered.GenerateData().AddToDbContext(useInMemoryDb: true);
            var alert = testData.WhenAnAlertAreTriggered.EntityData.Alerts.Single();
            var stringResourceResult = new Dictionary<string, string> { { SmsContentKey.Alerts.AlertTriggered, "Alert triggered!" } };
            _stringsResourcesServiceMock.GetSmsContentResources(Arg.Any<string>())
                .Returns(Result.Success<IDictionary<string, string>>(stringResourceResult));
            _configMock.BaseUrl = "http://example.com";
            var alertNotificationService = new AlertNotificationService(
                _nyssContextInMemoryMock,
                _queuePublisherServiceMock,
                _stringsResourcesServiceMock,
                _configMock,
                _loggerAdapterMock,
                _dateTimeProviderMock
            );

            // act
            await alertNotificationService.SendNotificationsForNewAlert(alert, new GatewaySetting { ApiKey = "123", Modems = new List<GatewayModem>()});

            // assert
            await _queuePublisherServiceMock.Received(1).QueueAlertCheck(alert.Id);
        }

        [Fact]
        public async Task CheckAlert_WhenAlertIsStillPending_ShouldNotifyAlertNotHandledRecipients()
        {
            // arrange
            _testData.WhenAnAlertAreTriggered.GenerateData().AddToDbContext();
            var alert = _testData.WhenAnAlertAreTriggered.EntityData.Alerts.Single();
            var stringResourceResult = new Dictionary<string, string>
            {
                { EmailContentKey.AlertHasNotBeenHandled.Subject, "Alert escalated subject" },
                { EmailContentKey.AlertHasNotBeenHandled.Body, "Alert escalated body" }
            };
            _stringsResourcesServiceMock.GetEmailContentResources(Arg.Any<string>())
                .Returns(Result.Success<IDictionary<string, string>>(stringResourceResult));
            _configMock.BaseUrl = "http://example.com";

            // act
            await _alertNotificationService.EmailAlertNotHandledRecipientsIfAlertIsPending(alert.Id);

            // assert
            await _queuePublisherServiceMock.Received(2).SendEmail(Arg.Any<(string, string)>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(AlertStatus.Closed)]
        [InlineData(AlertStatus.Dismissed)]
        [InlineData(AlertStatus.Escalated)]
        public async Task CheckAlert_WhenAlertIsNoLongerPending_ShouldNotNotifyAlertNotHandledRecipient(AlertStatus alertStatus)
        {
            // arrange
            _testData.WhenAnAlertAreTriggered.GenerateData().AddToDbContext();
            var alert = _testData.WhenAnAlertAreTriggered.EntityData.Alerts.Single();
            alert.Status = alertStatus;
            var stringResourceResult = new Dictionary<string, string>
            {
                { EmailContentKey.AlertHasNotBeenHandled.Subject, "Alert escalated subject" },
                { EmailContentKey.AlertHasNotBeenHandled.Body, "Alert escalated body" }
            };
            _stringsResourcesServiceMock.GetEmailContentResources(Arg.Any<string>())
                .Returns(Result.Success<IDictionary<string, string>>(stringResourceResult));
            _configMock.BaseUrl = "http://example.com";

            // act
            await _alertNotificationService.EmailAlertNotHandledRecipientsIfAlertIsPending(alert.Id);

            // assert
            await _queuePublisherServiceMock.DidNotReceive().SendEmail(Arg.Any<(string, string)>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
