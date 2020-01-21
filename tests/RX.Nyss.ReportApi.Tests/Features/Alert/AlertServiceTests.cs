using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Configuration;
using RX.Nyss.Common.Services.StringsResources;
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

        public AlertServiceTests()
        {
            var reportLabelingServiceMock = Substitute.For<IReportLabelingService>();
            var emailToSmsPublisherService = Substitute.For<IQueuePublisherService>();
            var config = Substitute.For<INyssReportApiConfig>();
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            var stringsResourcesService = Substitute.For<IStringsResourcesService>();
            _alertService = new AlertService(_nyssContextMock, reportLabelingServiceMock, _loggerAdapterMock, emailToSmsPublisherService, config, stringsResourcesService);

            _testData = new AlertServiceTestData(_nyssContextMock);
        }

        [Theory]
        [InlineData(ReportType.Activity)]
        [InlineData(ReportType.Aggregate)]
        [InlineData(ReportType.DataCollectionPoint)]
        public async Task ReportAdded_WhenReportTypeIsNotSingleOrNonHuman_ShouldReturnNull(ReportType reportType)
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

        [Theory]
        [InlineData(ReportType.Single)]
        [InlineData(ReportType.NonHuman)]
        public async Task ReportAdded_WhenReportTypeIsNonHumanAndFromDataCollectionPoint_ShouldReturnNull(ReportType reportType)
        {
            // arrange
            _testData.SimpleCasesData.GenerateData();
            var report = _testData.SimpleCasesData.AdditionalData.DataCollectionPointReport;
            report.ReportType = reportType;

            // act
            var result = await _alertService.ReportAdded(report);

            // assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenSingleReportDoesNotHaveAProjectHealthRisk_ShouldThrow()
        {
            //arrange
            _testData.SimpleCasesData.GenerateData();
            var report = _testData.SimpleCasesData.AdditionalData.SingleReportWithoutHealthRisk;


            //assert
            await Should.ThrowAsync<System.Reflection.TargetInvocationException>(() => _alertService.ReportAdded(report));
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldReturnNewPendingAlert()
        {
            //arrange
            _testData.WhenCountThresholdIsOne.GenerateData();
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
            _testData.WhenCountThresholdIsOne.GenerateData();
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
            _testData.WhenCountThresholdIsOne.GenerateData();
            var report = _testData.WhenCountThresholdIsOne.EntityData.Reports.Single();

            //act
            var result = await _alertService.ReportAdded(report);

            // Assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 1 && list.Any(ar => ar.Alert == result && ar.Report == report)
            ));
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldSaveChanges2Times()
        {
            //arrange
            _testData.WhenCountThresholdIsOne.GenerateData();
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
            _testData.WhenCountThresholdIsThreeAndIsNotSatisfied.GenerateData();
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
            _testData.WhenCountThresholdIsThreeAndIsNotSatisfied.GenerateData();
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
            _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.GenerateData();
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
            _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.GenerateData();
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
            _testData.WhenAddingToGroupWithNoAlertAndMeetingThreshold.GenerateData();
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
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData();
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
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData();
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
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData();
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
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData();
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
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData();
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
            _testData.WhenCountThresholdIsZero.GenerateData();
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
            _testData.WhenThereAreTrainingReportsInLabelGroup.GenerateData();
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
            _testData.WhenThereAreTrainingReportsInLabelGroup.GenerateData();
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
            _testData.WhenAddingToGroupWithAnExistingAlert.GenerateData();

            //act
            await _alertService.ReportDismissed(AlertServiceTestData.SimpleTestCaseAdditionalData.NotExistingReportId);

            //assert
            _loggerAdapterMock.Received(1).Warn(Arg.Any<string>());
            await _nyssContextMock.Received(0).SaveChangesAsync();
            _nyssContextMock.Received(0).SaveChanges();
            await _nyssContextMock.Received(0).ExecuteSqlInterpolatedAsync(Arg.Any<FormattableString>());
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingReportInAlertWithCountThreshold1_AlertShouldBeRejected()
        {
            //arrange
            _testData.WhenDismissingReportInAlertWithCountThreshold1.GenerateData();
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
            _testData.WhenDismissingReportInAlertWithCountThreshold1.GenerateData();
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
            _testData.WhenDismissingReportInAlertWithCountThreshold1.GenerateData();
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
            _testData.WhenTheReportStillHasAGroupThatMeetsCountThreshold.GenerateData();
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
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.GenerateData();
            var reportBeingDismissed = (Report)_testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.AdditionalData.ReportBeingDismissed;
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
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.GenerateData();
            var reportBeingDismissed = (Report) _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymore.AdditionalData.ReportBeingDismissed;

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert_MovedSomeReportsToAnotherAlert()
        {
            //arrange
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.GenerateData();
            var reportBeingDismissed = (Report)_testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportBeingDismissed;
            var alertThatReceivedNewReports = (Data.Models.Alert)_testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.AlertThatReceivedNewReports;
            var reportsBeingMoved = (List<Report>)_testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportsBeingMoved;

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
            _testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.GenerateData();
            var reportBeingDismissed = (Report)_testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportBeingDismissed;
            var reportsBeingMoved = (List<Report>)_testData.WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert.AdditionalData.ReportsBeingMoved;

            var acceptedMovedReport = reportsBeingMoved.Single(r => r.Status == ReportStatus.Accepted);

            //act
            await _alertService.ReportDismissed(reportBeingDismissed.Id);

            //assert
            acceptedMovedReport.Status.ShouldBe(ReportStatus.Accepted);;
        }
    }
}





