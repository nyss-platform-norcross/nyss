using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts
{
    public class AlertReportServiceTests
    {
        private readonly INyssContext _nyssContext;
        private readonly AlertReportService _alertReportService;
        private readonly List<AlertReport> _alertReports;
        private readonly IAlertService _alertService;
        private readonly IQueueService _queueService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly DateTime _now = DateTime.UtcNow;
        private readonly User _currentUser = new GlobalCoordinatorUser();

        public AlertReportServiceTests()
        {
            _nyssContext = Substitute.For<INyssContext>();
            var config = Substitute.For<INyssWebConfig>();

            _alertService = Substitute.For<IAlertService>();
            _queueService = Substitute.For<IQueueService>();
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _authorizationService = Substitute.For<IAuthorizationService>();
            _alertReportService = new AlertReportService(config, _nyssContext, _alertService, _queueService, _dateTimeProvider, _authorizationService);

            _alertReports = TestData.GetAlertReports();
            var alertReportsDbSet = _alertReports.AsQueryable().BuildMockDbSet();
            _nyssContext.AlertReports.Returns(alertReportsDbSet);

            config.ServiceBusQueues.Returns(new ServiceBusQueuesOptions
            {
                ReportDismissalQueue = TestData.ReportDismissalQueue
            });

            _dateTimeProvider.UtcNow.Returns(_now);
            _authorizationService.GetCurrentUser().Returns(_currentUser);
        }

        [Fact]
        public async Task AcceptReport_WhenAlertIsClosed_ShouldReturnError()
        {
            _alertReports.First().Alert.Status = AlertStatus.Closed;

            var result = await _alertReportService.AcceptReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.AcceptReport.WrongAlertStatus);
        }

        [Theory]
        [InlineData(AlertStatus.Dismissed)]
        [InlineData(AlertStatus.Escalated)]
        [InlineData(AlertStatus.Rejected)]
        public async Task AcceptReport_WhenAlertIsInRightStatus_ShouldReturnSuccess(AlertStatus status)
        {
            _alertReports.First().Alert.Status = status;

            var result = await _alertReportService.AcceptReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(ReportStatus.Rejected)]
        [InlineData(ReportStatus.Accepted)]
        [InlineData(ReportStatus.Removed)]
        public async Task AcceptReport_WhenReportIsNotPending_ShouldReturnError(ReportStatus status)
        {
            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = status;

            var result = await _alertReportService.AcceptReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.AcceptReport.WrongReportStatus);
        }

        [Fact]
        public async Task AcceptReport_WhenCriteriaAreMet_ShouldUpdateStatusInDatabase()
        {
            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = ReportStatus.Pending;

            await _alertReportService.AcceptReport(TestData.AlertId, TestData.ReportId);

            _alertReports.First().Report.Status.ShouldBe(ReportStatus.Accepted);
            _alertReports.First().Report.AcceptedAt.ShouldBe(_now);
            _alertReports.First().Report.AcceptedBy.ShouldBe(_currentUser);
            await _nyssContext.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AcceptReport_WhenCriteriaAreMet_ShouldReturnAssessmentStatus()
        {
            var alertAssessmentStatus = AlertAssessmentStatus.ToEscalate;
            _alertService.GetAssessmentStatus(TestData.AlertId).Returns(alertAssessmentStatus);

            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = ReportStatus.Pending;

            var result = await _alertReportService.AcceptReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeTrue();
            result.Value.AssessmentStatus.ShouldBe(alertAssessmentStatus);
        }

        [Fact]
        public async Task DismissReport_WhenAlertIsClosed_ShouldReturnError()
        {
            _alertReports.First().Alert.Status = AlertStatus.Closed;

            var result = await _alertReportService.DismissReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.DismissReport.WrongAlertStatus);
        }

        [Theory]
        [InlineData(ReportStatus.Rejected)]
        [InlineData(ReportStatus.Accepted)]
        [InlineData(ReportStatus.Removed)]
        public async Task DismissReport_WhenReportIsNotPending_ShouldReturnError(ReportStatus status)
        {
            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = status;

            var result = await _alertReportService.DismissReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Alert.DismissReport.WrongReportStatus);
        }

        [Fact]
        public async Task DismissReport_WhenCriteriaAreMet_ShouldUpdateStatusInDatabase()
        {
            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = ReportStatus.Pending;

            await _alertReportService.DismissReport(TestData.AlertId, TestData.ReportId);

            _alertReports.First().Report.Status.ShouldBe(ReportStatus.Rejected);
            _alertReports.First().Report.RejectedAt.ShouldBe(_now);
            _alertReports.First().Report.RejectedBy.ShouldBe(_currentUser);
            await _nyssContext.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DismissReport_WhenCriteriaAreMet_ShouldSendMessageToTheQueue()
        {
            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = ReportStatus.Pending;

            await _alertReportService.DismissReport(TestData.AlertId, TestData.ReportId);

            _alertReports.First().Report.Status.ShouldBe(ReportStatus.Rejected);
            await _queueService.Received(1).Send(TestData.ReportDismissalQueue, Arg.Is<DismissReportMessage>(x => x.ReportId == TestData.ReportId));
        }

        [Fact]
        public async Task DismissReport_WhenCriteriaAreMet_ShouldReturnAssessmentStatus()
        {
            var alertAssessmentStatus = AlertAssessmentStatus.ToEscalate;
            _alertService.GetAssessmentStatus(TestData.AlertId).Returns(alertAssessmentStatus);

            _alertReports.First().Alert.Status = AlertStatus.Pending;
            _alertReports.First().Report.Status = ReportStatus.Pending;

            var result = await _alertReportService.DismissReport(TestData.AlertId, TestData.ReportId);

            result.IsSuccess.ShouldBeTrue();
            result.Value.AssessmentStatus.ShouldBe(alertAssessmentStatus);
        }

        private static class TestData
        {
            public const int AlertId = 1;
            public const int ReportId = 23;
            public const string ReportDismissalQueue = "ReportDismissalQueue";

            public static List<AlertReport> GetAlertReports() =>
                new List<AlertReport>
                {
                    new AlertReport
                    {
                        AlertId = AlertId,
                        ReportId = ReportId,
                        Alert = new Alert { Id = AlertId },
                        Report = new Report { Id = ReportId, Status = ReportStatus.Pending }
                    }
                };
        }
    }
}
