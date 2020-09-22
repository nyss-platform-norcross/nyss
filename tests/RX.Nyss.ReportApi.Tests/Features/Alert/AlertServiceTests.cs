using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Alert
{
    public class AlertServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IAlertService _alertService;
        private readonly ILoggerAdapter _loggerAdapterMock;

        private readonly AlertServiceTestData _testData;
        private readonly IQueuePublisherService _queuePublisherServiceMock;
        private readonly IStringsResourcesService _stringsResourcesServiceMock;
        private readonly INyssReportApiConfig _nyssReportApiConfigMock;
        private readonly IDateTimeProvider _dateTimeProviderMock;
        private readonly IReportLabelingService _reportLabelingServiceMock;

        public AlertServiceTests()
        {
            _reportLabelingServiceMock = Substitute.For<IReportLabelingService>();
            _queuePublisherServiceMock = Substitute.For<IQueuePublisherService>();
            _nyssReportApiConfigMock = Substitute.For<INyssReportApiConfig>();
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _stringsResourcesServiceMock = Substitute.For<IStringsResourcesService>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
            _alertService = new AlertService(
                _nyssContextMock,
                _reportLabelingServiceMock,
                _loggerAdapterMock,
                _queuePublisherServiceMock,
                _nyssReportApiConfigMock,
                _stringsResourcesServiceMock,
                _dateTimeProviderMock
            );

            _testData = new AlertServiceTestData(_nyssContextMock);
        }

        [Theory]
        [InlineData(ReportType.Aggregate)]
        [InlineData(ReportType.DataCollectionPoint)]
        public async Task ReportAdded_WhenReportTypeIsAggregateOrDCP_ShouldReturnNull(ReportType reportType)
        {
            //arrange
            _testData.SimpleCasesData.GenerateData();
            var report = _testData.SimpleCasesData.AdditionalData.HumanDataCollectorReport;
            report.ReportType = reportType;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenReportTypeIsStatementAndHealthRiskIsActivity_ShouldReturnNull()
        {
            //arrange
            _testData.SimpleCasesData.GenerateData();
            var report = _testData.SimpleCasesData.AdditionalData.HumanDataCollectorReport;
            report.ReportType = ReportType.Statement;
            report.ProjectHealthRisk.HealthRisk.HealthRiskType = HealthRiskType.Activity;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Theory]
        [InlineData(ReportType.Single)]
        [InlineData(ReportType.Statement)]
        [InlineData(ReportType.Aggregate)]
        [InlineData(ReportType.DataCollectionPoint)]
        public async Task ReportAdded_WhenReportTypeIsNonHumanAndFromDataCollectionPoint_ShouldReturnNull(ReportType reportType)
        {
            //arrange
            _testData.SimpleCasesData.GenerateData();
            var report = _testData.SimpleCasesData.AdditionalData.DataCollectionPointReport;
            report.ReportType = reportType;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public void ReportAdded_WhenSingleReportDoesNotHaveAProjectHealthRisk_ShouldThrow()
        {
            //arrange
            _testData.SimpleCasesData.GenerateData().AddToDbContext();
            var report = _testData.SimpleCasesData.AdditionalData.SingleReportWithoutHealthRisk;

            //assert
            Should.ThrowAsync<TargetInvocationException>(() => _alertService.ReportAdded(report));
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldReturnNewPendingAlert()
        {
            //arrange
            _testData.WhenCountThresholdIsOne.GenerateData().AddToDbContext();
            var report = _testData.WhenCountThresholdIsOne.EntityData.Reports.Single();
            var existingAlerts = _testData.WhenCountThresholdIsOne.EntityData.Alerts;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeOfType<Data.Models.Alert>();
            result.Status.ShouldBe(AlertStatus.Pending);
            existingAlerts.ShouldNotContain(result);
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ReportShouldBeChangedToPending()
        {
            //arrange
            _testData.WhenCountThresholdIsOne.GenerateData().AddToDbContext();
            var report = _testData.WhenCountThresholdIsOne.EntityData.Reports.Single();

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            report.Status.ShouldBe(ReportStatus.Pending);
        }


        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldAddAlertReportEntity()
        {
            //arrange
            _testData.WhenCountThresholdIsOne.GenerateData().AddToDbContext();
            var report = _testData.WhenCountThresholdIsOne.EntityData.Reports.Single();

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 1 && list.Any(ar => ar.Alert == result && ar.Report == report)
            ));
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldSaveChanges2Times()
        {
            //arrange
            _testData.WhenCountThresholdIsOne.GenerateData().AddToDbContext();
            var report = _testData.WhenCountThresholdIsOne.EntityData.Reports.Single();

            //act
            var result = await _alertService.ReportAdded(report);

            // Assert
            await _nyssContextMock.Received(2).SaveChangesAsync();
        }


        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsThreeAndIsNotSatisfied_ShouldReturnNull()
        {
            //arrange
            _testData.WhenCountThresholdIsThreeAndIsNotSatisfied.GenerateData().AddToDbContext();
            var report = _testData.WhenCountThresholdIsThreeAndIsNotSatisfied.EntityData.Reports.Single();

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsThreeAndIsNotSatisfied_ReportStatusShouldRemainNew()
        {
            //arrange
            _testData.WhenCountThresholdIsThreeAndIsNotSatisfied.GenerateData().AddToDbContext();
            var report = _testData.WhenCountThresholdIsThreeAndIsNotSatisfied.EntityData.Reports.Single();

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            report.Status.ShouldBe(ReportStatus.New);
        }


        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithNoAlertAndMeetingThreshold_ShouldReturnNewPendingAlert()
        {
            //arrange
            _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.EntityData.Reports.FirstOrDefault();
            var existingAlerts = _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.EntityData.Alerts;

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            result.ShouldBeOfType<Data.Models.Alert>();
            result.Status.ShouldBe(AlertStatus.Pending);
            existingAlerts.ShouldNotContain(result);
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithNoAlertAndMeetingThreshold_ShouldAddAlertReportEntities()
        {
            //arrange
            _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.EntityData.Reports.FirstOrDefault();
            var existingAlerts = _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.EntityData.Alerts;

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 3
                && list.Any(ar => ar.Alert == result && ar.Report == reportBeingAdded)
                && list.All(ar => ar.Alert == result)
            ));
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithNoAlertAndMeetingThreshold_ShouldSaveChanges2Times()
        {
            //arrange
            _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.EntityData.Reports.FirstOrDefault();

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            await _nyssContextMock.Received(2).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ShouldReturnNull()
        {
            //arrange
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithAnExistingAlert.EntityData.Reports.Single(r => r.Status == ReportStatus.New);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ReportStatusShouldBeChangedToPending()
        {
            //arrange
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithAnExistingAlert.EntityData.Reports.Single(r => r.Status == ReportStatus.New);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            reportBeingAdded.Status.ShouldBe(ReportStatus.Pending);
        }


        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_NoNewAlertShouldCreated()
        {
            //arrange
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithAnExistingAlert.EntityData.Reports.Single(r => r.Status == ReportStatus.New);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            await _nyssContextMock.Alerts.Received(0).AddAsync(Arg.Any<Data.Models.Alert>());
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ReportShouldBeAddedToAlert()
        {
            //arrange
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithAnExistingAlert.EntityData.Reports.Single(r => r.Status == ReportStatus.New);
            var existingAlert = _testData.WhenAddingToGroupWithAnExistingAlert.EntityData.Alerts.Single();

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 1
                && list.Any(ar => ar.Alert == existingAlert && ar.Report == reportBeingAdded)
            ));
        }


        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ShouldSaveChanges2Times()
        {
            //arrange
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenAddingToGroupWithAnExistingAlert.EntityData.Reports.Single(r => r.Status == ReportStatus.New);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            await _nyssContextMock.Received(2).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsZero_ShouldNotCreateAnyNewAlert()
        {
            //arrange
            _testData.WhenCountThresholdIsZero.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenCountThresholdIsZero.EntityData.Reports.Single(r => r.Status == ReportStatus.New);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenThereAreTrainingReportsInLabelGroup_AddingTrainingReportShouldNotTriggerAlerts()
        {
            //arrange
            _testData.WhenThereAreTrainingReportsInLabelGroup.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenThereAreTrainingReportsInLabelGroup.EntityData.Reports.Single(r => r.IsTraining);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenThereAreTrainingReportsInLabelGroup_AddingRealReportShouldNotTriggerAlerts()
        {
            //arrange
            _testData.WhenThereAreTrainingReportsInLabelGroup.GenerateData().AddToDbContext();
            var reportBeingAdded = _testData.WhenThereAreTrainingReportsInLabelGroup.EntityData.Reports.First(r => !r.IsTraining);

            //act
            var result = await _alertService.ReportAdded(reportBeingAdded);

            //assert
            result.ShouldBeNull();
        }


        [Fact]
        public async Task ReportDismissed_WhenDismissingNonExistingAlert_ShouldDoNothingAndLogWarning()
        {
            //arrange
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData().AddToDbContext();

            //act
            await _alertService.ReportDismissed(AlertServiceTestData.SimpleTestCaseAdditionalData.NotExistingReportId);

            //assert
            _loggerAdapterMock.Received(1).Warn(Arg.Any<string>());
            await _nyssContextMock.Received(0).SaveChangesAsync();
            await _nyssContextMock.Received(0).SaveChangesAsync();
            await _nyssContextMock.Received(0).ExecuteSqlInterpolatedAsync(Arg.Any<FormattableString>());
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingReportInAlertWithCountThreshold1_AlertShouldBeRejected()
        {
            //arrange
            _testData.WhenDismissingReportInAlertWithCountThreshold1.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenDismissingReportInAlertWithCountThreshold1.EntityData.Reports.Single(r => r.Status == ReportStatus.Pending);
            var existingAlert = _testData.WhenDismissingReportInAlertWithCountThreshold1.EntityData.Alerts.Single();

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            existingAlert.Status.ShouldBe(AlertStatus.Rejected);
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingReportInAlertWithCountThreshold1_ReportShouldBeRejected()
        {
            //arrange
            _testData.WhenDismissingReportInAlertWithCountThreshold1.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenDismissingReportInAlertWithCountThreshold1.EntityData.Reports.Single(r => r.Status == ReportStatus.Pending);

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            reportBeingDismissed.Status.ShouldBe(ReportStatus.Rejected);
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingReportInAlertWithCountThreshold1_ShouldSaveChangesOnce()
        {
            //arrange
            _testData.WhenDismissingReportInAlertWithCountThreshold1.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenDismissingReportInAlertWithCountThreshold1.EntityData.Reports.Single(r => r.Status == ReportStatus.Pending);

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportDismissed_WhenTheReportStillHasAGroupThatMeetsCountThreshold_ShouldNotRejectAlert()
        {
            //arrange
            _testData.WhenTheReportStillHasAGroupThatMeetsCountThreshold.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenTheReportStillHasAGroupThatMeetsCountThreshold.EntityData.Reports.First(r => r.Status == ReportStatus.Pending);
            var alert = reportBeingDismissed.ReportAlerts.Select(ra => ra.Alert).Single();

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            alert.Status.ShouldBe(AlertStatus.Pending);
        }


        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymore_ShouldRejectAlert()
        {
            //arrange
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.AdditionalData.ReportBeingDismissed;
            var alert = reportBeingDismissed.ReportAlerts.Select(ra => ra.Alert).Single();

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            alert.Status.ShouldBe(AlertStatus.Rejected);
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymore_ShouldCallSaveChanges3Times()
        {
            //arrange
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.AdditionalData.ReportBeingDismissed;

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert_MovedSomeReportsToAnotherAlert()
        {
            //arrange
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportBeingDismissed;
            var alertThatReceivedNewReports = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.AlertThatReceivedNewReports;
            var reportsBeingMoved = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportsBeingMoved;

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 2
                && list.Any(ar => ar.Alert == alertThatReceivedNewReports && ar.Report == reportsBeingMoved[0])
                && list.Any(ar => ar.Alert == alertThatReceivedNewReports && ar.Report == reportsBeingMoved[1])
            ));
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert_ReportsThatWereAcceptedInPreviousAlertShouldRemainAcceptedInTheNewOne()
        {
            //arrange
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.GenerateData().AddToDbContext();
            var reportBeingDismissed = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportBeingDismissed;
            var reportsBeingMoved = _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportsBeingMoved;

            var acceptedMovedReport = reportsBeingMoved.Single(r => r.Status == ReportStatus.Accepted);

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            acceptedMovedReport.Status.ShouldBe(ReportStatus.Accepted);
            ;
        }

        [Fact]
        public async Task ReportReset_WhenResettingDismissedReport_ShouldReturnSuccess()
        {
            //arrange
            _testData.WhenADismissedReportIsReset.GenerateData().AddToDbContext();
            var reportBeingReset = _testData.WhenADismissedReportIsReset.AdditionalData.ReportBeingReset;

            //act
            await _alertService.ReportReset(reportBeingReset.Id);

            //assert
            reportBeingReset.Status.ShouldBe(ReportStatus.Pending);
        }

        [Fact]
        public async Task ReportReset_WhenResettingKeptReport_ShouldReturnSuccess()
        {
            //arrange
            _testData.WhenAnAcceptedReportIsReset.GenerateData().AddToDbContext();
            var reportBeingReset = _testData.WhenAnAcceptedReportIsReset.AdditionalData.ReportBeingReset;

            //act
            await _alertService.ReportReset(reportBeingReset.Id);

            //assert
            reportBeingReset.Status.ShouldBe(ReportStatus.Pending);
        }

        [Fact]
        public async Task ReportReset_WhenResettingReportInEscalatedAlert_StatusOfReportShouldNotChange()
        {
            //arrange
            _testData.WhenResettingAReportInAlertWithStatusNotPending.GenerateData().AddToDbContext();
            var reportBeingReset = _testData.WhenResettingAReportInAlertWithStatusNotPending.AdditionalData.ReportBeingReset;
            reportBeingReset.AcceptedAt = new DateTime(2020, 2, 1);

            //act
            await _alertService.ReportReset(reportBeingReset.Id);

            //assert
            reportBeingReset.Status.ShouldBe(ReportStatus.Accepted);
            await _reportLabelingServiceMock.DidNotReceiveWithAnyArgs().ResolveLabelsOnReportAdded(null, null);
        }

        [Fact]
        public async Task ReportReset_WhenResettingAcceptedReport_ShouldNotRecalculate()
        {
            //arrange
            _testData.WhenAnAcceptedReportIsReset.GenerateData().AddToDbContext();
            var reportBeingReset = _testData.WhenAnAcceptedReportIsReset.AdditionalData.ReportBeingReset;
            var alertRule = _testData.WhenAnAcceptedReportIsReset.AdditionalData.AlertRule;

            //act
            await _alertService.ReportReset(reportBeingReset.Id);

            //assert
            await _reportLabelingServiceMock.Received(0).CalculateNewLabelsInLabelGroup(reportBeingReset.ReportGroupLabel, alertRule.KilometersThreshold.Value * 1000 * 2, reportBeingReset.Id);
        }

        [Fact]
        public async Task ReportReset_WhenResettingDismissedReport_ShouldRecalculate()
        {
            //arrange
            _testData.WhenADismissedReportIsReset.GenerateData().AddToDbContext();
            var reportBeingReset = _testData.WhenADismissedReportIsReset.AdditionalData.ReportBeingReset;
            var alertRule = _testData.WhenADismissedReportIsReset.AdditionalData.AlertRule;

            //act
            await _alertService.ReportReset(reportBeingReset.Id);

            //assert
            await _reportLabelingServiceMock.Received(1).ResolveLabelsOnReportAdded(reportBeingReset, reportBeingReset.ProjectHealthRisk);
        }

        [Fact]
        public async Task CheckAlert_WhenAlertIsStillPending_ShouldSendAlert()
        {
            // arrange
            _testData.SimpleCasesData.GenerateData().AddToDbContext();

            // act
            await _alertService.CheckIfAlertHasBeenHandled(1);

            // assert
            await _queuePublisherServiceMock.DidNotReceiveWithAnyArgs().SendEmail((null, null), null, null);
        }

        [Fact]
        public async Task CheckAlert_WhenAlertNotFound_ShouldDoNothing()
        {
            // arrange
            _testData.SimpleCasesData.GenerateData().AddToDbContext();

            // act
            await _alertService.CheckIfAlertHasBeenHandled(1);

            // assert
            await _queuePublisherServiceMock.DidNotReceiveWithAnyArgs().SendEmail((null, null), null, null);
        }

        [Fact]
        public async Task SendNotificationsForNewAlert_ShouldSendSmsToAllSupervisorsAndQueueCheckBack()
        {
            // arrange
            _testData.WhenAnAlertAreTriggered.GenerateData().AddToDbContext();
            var alert = _testData.WhenAnAlertAreTriggered.EntityData.Alerts.Single();
            var stringResourceResult = new Dictionary<string, string> { { SmsContentKey.Alerts.AlertTriggered, "Alert triggered!" } };
            _stringsResourcesServiceMock.GetSmsContentResources(Arg.Any<string>())
                .Returns(Result.Success<IDictionary<string, string>>(stringResourceResult));
            _nyssReportApiConfigMock.BaseUrl = "http://example.com";

            // act
            await _alertService.SendNotificationsForNewAlert(alert, new GatewaySetting { ApiKey = "123" });

            // assert
            await _queuePublisherServiceMock.Received(1).QueueAlertCheck(alert.Id);
        }

        [Fact]
        public async Task CheckAlert_WhenAlertIsStillPending_ShouldNotifyHeadManagers()
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
            _nyssReportApiConfigMock.BaseUrl = "http://example.com";

            // act
            await _alertService.CheckIfAlertHasBeenHandled(alert.Id);

            // assert
            await _queuePublisherServiceMock.Received(2).SendEmail(Arg.Any<(string, string)>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(AlertStatus.Closed)]
        [InlineData(AlertStatus.Dismissed)]
        [InlineData(AlertStatus.Escalated)]
        [InlineData(AlertStatus.Rejected)]
        public async Task CheckAlert_WhenAlertIsNoLongerPending_ShouldNotNotifyHeadManager(AlertStatus alertStatus)
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
            _nyssReportApiConfigMock.BaseUrl = "http://example.com";

            // act
            await _alertService.CheckIfAlertHasBeenHandled(alert.Id);

            // assert
            await _queuePublisherServiceMock.DidNotReceive().SendEmail(Arg.Any<(string, string)>(), Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
