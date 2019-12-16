using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Services;
using RX.Nyss.ReportApi.Utils.Logging;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Alert
{
    public class AlertServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IReportLabelingService _reportLabelingServiceMock;
        private readonly IAlertService _alertService;
        private readonly ILoggerAdapter _loggerAdapterMock;
        private readonly IEmailToSmsPublisherService _emailToSmsPublisherService;
        private readonly IConfig _config;

        private const int NotExistingReportId = 9999;
        private const int AddedReportWithThreshold1Id = 1;
        private const int AddedReportWithThreshold2Id = 2;
        private const int ExistingReportWithNoAlertId1 = 3;
        private const int ExistingReportWithNoAlertId2 = 4;
        private const int ExistingReportWithNoAlertId3 = 5;
        private const int ExistingReportWithAlertId1 = 6;
        private const int ExistingReportWithAlertId2 = 7;
        private const int ExistingReportWithAlertId3 = 8;
        private const int ExistingReportWithAlertWithCountThreshold1Id = 9;

        private const int ReportInAlert1BrokenApartId1 = 10;
        private const int ReportInAlert1BrokenApartId2 = 11;
        private const int ReportInAlert1BrokenApartId3 = 12;
        private const int ReportInAlert1BrokenApartId4 = 13;
        private const int ReportInAlert1BrokenApartId5 = 14;
        private const int ReportInAlert1BrokenApartId6 = 15;
        private const int DismissedReportInAlert1BrokenApartId = 16;

        private const int ReportInAlert2BrokenApartId1 = 17;
        private const int ReportInAlert2BrokenApartId2 = 18;
        private const int ReportInAlert2BrokenApartId3 = 19;
        private const int ReportInAlert2BrokenApartId4 = 20;
        private const int ReportInAlert2BrokenApartId5 = 21;
        private const int ReportInAlert2BrokenApartId6 = 22;
        private const int ReportInAlert2BrokenApartId7 = 23;
        private const int DismissedReportInAlert2BrokenApartId = 24;


        private const int ProjectHealthRiskWithThreshold1Id = 1;
        private const int ProjectHealthRiskId2 = 2;
        private const int AlertRuleId1 = 1;
        private const int AlertRuleId2 = 2;
        private const int AlertRuleCountThreshold1 = 1;
        private const int AlertRuleCountThreshold3 = 3;

        private readonly Guid _labelNotInAnyGroup1 = Guid.Parse("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19");
        private readonly Guid _labelNotInAnyGroup2 = Guid.Parse("19378b71-c7cd-43b3-b856-117dc30ee291");
        private readonly Guid _labelForGroup1 = Guid.Parse("CBCA820B-CA76-4E67-9C08-2845B61CAA5B");
        private readonly Guid _labelForGroup2 = Guid.Parse("CF03F15E-96C4-4CAB-A33F-3E725CD057B5");
        private readonly Guid _labelForGroup3 = Guid.Parse("1de994ad-4aed-41c2-9717-9785ec9ed738");
        private readonly Guid _labelForSingleReportGroup = Guid.Parse("843a06a9-6cbf-4d76-b236-49151d63a28c");

        private readonly Guid _labelFromAlertBrokenApart1 = Guid.Parse("b181abbf-f3dc-4491-ba1e-d475c196abac");
        private readonly Guid _labelFromAlertBrokenApart2 = Guid.Parse("7b6fa180-5b38-46d7-a9b3-6d1a804202fa");
        private readonly Guid _labelFromAlertBrokenApart3 = Guid.Parse("9af83735-3f8c-4827-b867-78063c9e698e");
        private readonly Guid _labelFromAlertBrokenApart4 = Guid.Parse("77545503-93e2-4208-a07b-b92f37fa8944");
        private readonly Guid _labelFromAlertBrokenApart5 = Guid.Parse("26ed7aaa-64f0-4f6e-86f4-8ceba31a9425");
        private readonly Guid _labelFromAlertBrokenApart6 = Guid.Parse("d0e415f1-fb46-4080-b17d-f9b1814cc282");

        private const int ExistingAlertId = 1;
        private const int ExistingAlertWithOneReportCountThresholdId = 2;
        private const int AlertRuleKilometersThreshold = 1;

        private const int AlertBrokenApart1Id = 3;
        private const int AlertBrokenApart2Id = 4;
        

        public AlertServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _reportLabelingServiceMock = Substitute.For<IReportLabelingService>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _emailToSmsPublisherService = Substitute.For<IEmailToSmsPublisherService>();
            _config = Substitute.For<IConfig>();
            _alertService = new AlertService(_nyssContextMock, _reportLabelingServiceMock, _loggerAdapterMock, _emailToSmsPublisherService, _config);

            var alertRules = new List<AlertRule>
            {
                new AlertRule{ Id = AlertRuleId1, CountThreshold = AlertRuleCountThreshold1, KilometersThreshold = AlertRuleKilometersThreshold},
                new AlertRule{ Id = AlertRuleId2, CountThreshold = AlertRuleCountThreshold3, KilometersThreshold = AlertRuleKilometersThreshold}
            };

            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk { Id = ProjectHealthRiskWithThreshold1Id, AlertRule = alertRules[0] },
                new ProjectHealthRisk { Id = ProjectHealthRiskId2, AlertRule = alertRules[1] }
            };

            var reports = new List<Report>
            {
                new Report{ Id = AddedReportWithThreshold1Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0]},
                new Report{ Id = AddedReportWithThreshold2Id, ReportGroupLabel = _labelNotInAnyGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithNoAlertId1, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithNoAlertId2, ReportGroupLabel = _labelForGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithNoAlertId3, ReportGroupLabel = _labelForGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithAlertId1, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithAlertId2, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithAlertId3, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ExistingReportWithAlertWithCountThreshold1Id, ReportGroupLabel = _labelForSingleReportGroup, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0]},
            };

            var reportsInAlertBrokenApart1 = new List<Report>
            {
                new Report{ Id = ReportInAlert1BrokenApartId1, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.Accepted , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert1BrokenApartId2, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert1BrokenApartId3, ReportGroupLabel = _labelFromAlertBrokenApart2, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert1BrokenApartId4, ReportGroupLabel = _labelFromAlertBrokenApart2, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert1BrokenApartId5, ReportGroupLabel = _labelFromAlertBrokenApart3, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert1BrokenApartId6, ReportGroupLabel = _labelFromAlertBrokenApart3, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = DismissedReportInAlert1BrokenApartId, ReportGroupLabel = _labelFromAlertBrokenApart3, Status = ReportStatus.Rejected , ProjectHealthRisk = projectHealthRisks[1]},
            };

            var reportsInAlertBrokenApart2 = new List<Report>
            {
                new Report{ Id = ReportInAlert2BrokenApartId1, ReportGroupLabel = _labelFromAlertBrokenApart4, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert2BrokenApartId2, ReportGroupLabel = _labelFromAlertBrokenApart4, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert2BrokenApartId3, ReportGroupLabel = _labelFromAlertBrokenApart4, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert2BrokenApartId4, ReportGroupLabel = _labelFromAlertBrokenApart5, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert2BrokenApartId5, ReportGroupLabel = _labelFromAlertBrokenApart5, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert2BrokenApartId6, ReportGroupLabel = _labelFromAlertBrokenApart6, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = ReportInAlert2BrokenApartId7, ReportGroupLabel = _labelFromAlertBrokenApart6, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[1]},
                new Report{ Id = DismissedReportInAlert2BrokenApartId, ReportGroupLabel = _labelFromAlertBrokenApart6, Status = ReportStatus.Rejected , ProjectHealthRisk = projectHealthRisks[1]},
            };

            reports.AddRange(reportsInAlertBrokenApart1);
            reports.AddRange(reportsInAlertBrokenApart2);

            var alerts = new List<Data.Models.Alert>
            {
                new Data.Models.Alert{ Id = ExistingAlertId, Status = AlertStatus.Pending, ProjectHealthRisk = projectHealthRisks[1] },
                new Data.Models.Alert{ Id = ExistingAlertWithOneReportCountThresholdId, Status = AlertStatus.Pending, ProjectHealthRisk = projectHealthRisks[0] },
                new Data.Models.Alert{ Id = AlertBrokenApart1Id, Status = AlertStatus.Pending, ProjectHealthRisk = projectHealthRisks[1] },
                new Data.Models.Alert{ Id = AlertBrokenApart2Id, Status = AlertStatus.Pending, ProjectHealthRisk = projectHealthRisks[1] }
            };


            var reportWithAlert1 = reports.Single(r => r.Id == ExistingReportWithAlertId1);
            var reportWithAlert2 = reports.Single(r => r.Id == ExistingReportWithAlertId2);
            var reportWithAlert3 = reports.Single(r => r.Id == ExistingReportWithAlertId3);
            var reportWithAlertWithCountThreshold1 = reports.Single(r => r.Id == ExistingReportWithAlertWithCountThreshold1Id);

            var alertReports = new List<AlertReport>
            {
                new AlertReport{ Alert = alerts[0], Report =  reportWithAlert1, ReportId = ExistingReportWithAlertId1},
                new AlertReport{ Alert = alerts[0], Report = reportWithAlert2, ReportId = ExistingReportWithAlertId2 },
                new AlertReport{ Alert = alerts[0], Report = reportWithAlert3, ReportId = ExistingReportWithAlertId3 },
                new AlertReport{ Alert = alerts[1], Report = reportWithAlertWithCountThreshold1, ReportId = ExistingReportWithAlertWithCountThreshold1Id },
            };

            var alertReportsForAlertBrokenApart1 = new List<AlertReport>
            {
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[0], ReportId = ReportInAlert1BrokenApartId1 },
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[1], ReportId = ReportInAlert1BrokenApartId2 },
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[2], ReportId = ReportInAlert1BrokenApartId3 },
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[3], ReportId = ReportInAlert1BrokenApartId4 },
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[4], ReportId = ReportInAlert1BrokenApartId5 },
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[5], ReportId = ReportInAlert1BrokenApartId6 },
                new AlertReport{ Alert = alerts[2], AlertId = AlertBrokenApart1Id, Report =  reportsInAlertBrokenApart1[6], ReportId = DismissedReportInAlert1BrokenApartId },
            };

            var alertReportsForAlertBrokenApart2 = new List<AlertReport>
            {
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[0], ReportId = ReportInAlert2BrokenApartId1 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[1], ReportId = ReportInAlert2BrokenApartId2 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[2], ReportId = ReportInAlert2BrokenApartId3 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[3], ReportId = ReportInAlert2BrokenApartId4 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[4], ReportId = ReportInAlert2BrokenApartId5 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[5], ReportId = ReportInAlert2BrokenApartId6 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[6], ReportId = ReportInAlert2BrokenApartId7 },
                new AlertReport{ Alert = alerts[3], AlertId = AlertBrokenApart2Id, Report =  reportsInAlertBrokenApart2[7], ReportId = DismissedReportInAlert2BrokenApartId },
            };

            alertReports.AddRange(alertReportsForAlertBrokenApart1);
            alertReports.AddRange(alertReportsForAlertBrokenApart2);

            reports.ForEach(r => r.ReportAlerts = alertReports.Where(ar => ar.ReportId == r.Id).ToList());



            var alertRulesDbSet = alertRules.AsQueryable().BuildMockDbSet();
            var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            var alertReportsDbSet = alertReports.AsQueryable().BuildMockDbSet();
            var alertsDbSet = alerts.AsQueryable().BuildMockDbSet();
            var reportsDbSet = reports.AsQueryable().BuildMockDbSet();

            _nyssContextMock.AlertRules.Returns(alertRulesDbSet);
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);
            _nyssContextMock.AlertReports.Returns(alertReportsDbSet);
            _nyssContextMock.Alerts.Returns(alertsDbSet);
            _nyssContextMock.Reports.Returns(reportsDbSet);
        }

        [Theory]
        [InlineData(ReportType.Activity)]
        [InlineData(ReportType.Aggregate)]
        [InlineData(ReportType.DataCollectionPoint)]
        [InlineData(ReportType.NonHuman)]
        public async Task ReportAdded_WhenReportTypeIsNotSingle_ShouldReturnNull(ReportType reportType)
        {
            //arrange
            var report = new Report { ReportType = reportType };

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenSingleReportDoesNotHaveAProjectHealthRisk_ShouldThrow()
        {
            //arrange
            var report = new Report { ReportType = ReportType.Single };

            //assert
            await Should.ThrowAsync<System.Reflection.TargetInvocationException>(() => _alertService.ReportAdded(report));
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldReturnNewPendingAlert()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold1Id);
            var existingAlerts = _nyssContextMock.Alerts.ToList();

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
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold1Id);

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            report.Status.ShouldBe(ReportStatus.Pending);
        }


        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsOne_ShouldAddAlertReportEntity()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold1Id);

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
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold1Id);

            //act
            var result = await _alertService.ReportAdded(report);

            // Assert
            await _nyssContextMock.Received(2).SaveChangesAsync();
        }


        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsThreeAndIsNotSatisfied_ShouldReturnNull()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup1;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenCountThresholdIsThreeAndIsNotSatisfied_ReportStatusShouldRemainNew()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup1;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            report.Status.ShouldBe(ReportStatus.New);
        }


        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithNoAlertAndMeetingThreshold_ShouldReturnNewPendingAlert()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup2;
            var existingAlerts = _nyssContextMock.Alerts.ToList();

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeOfType<Data.Models.Alert>();
            result.Status.ShouldBe(AlertStatus.Pending);
            existingAlerts.ShouldNotContain(result);
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithNoAlertAndMeetingThreshold_ShouldAddAlertReportEntities()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup2;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 3
                && list.Any(ar => ar.Alert == result && ar.Report == report)
                && list.All(ar => ar.Alert == result)
            ));
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithNoAlertAndMeetingThreshold_ShouldSaveChanges2Times()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup2;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            await _nyssContextMock.Received(2).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ShouldReturnNull()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup3;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ReportStatusShouldBeChangedToPending()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup3;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            report.Status.ShouldBe(ReportStatus.Pending);
        }


        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_NoNewAlertShouldCreated()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup3;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            await _nyssContextMock.Alerts.Received(0).AddAsync(Arg.Any<Data.Models.Alert>());
        }

        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ReportShouldBeAddedToAlert()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup3;
            var existingAlert = _nyssContextMock.Alerts.Single(r => r.Id == ExistingAlertId);

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 1
                && list.Any(ar => ar.Alert == existingAlert && ar.Report == report)
            ));
        }


        [Fact]
        public async Task ReportAdded_WhenAddingToGroupWithAnExistingAlert_ShouldSaveChanges2Times()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == AddedReportWithThreshold2Id);
            report.ReportGroupLabel = _labelForGroup3;

            //act
            var result = await _alertService.ReportAdded(report);

            //assert
            await _nyssContextMock.Received(2).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingNonExistingAlert_ShouldDoNothingAndLogWarning()
        {
            //act
            await _alertService.ReportDismissed(NotExistingReportId);

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
            var reportId = ExistingReportWithAlertWithCountThreshold1Id;
            var alert = _nyssContextMock.Alerts.Single(a => a.Id == ExistingAlertWithOneReportCountThresholdId);

            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            alert.Status.ShouldBe(AlertStatus.Rejected);
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingReportInAlertWithCountThreshold1_ReportShouldBeRejected()
        {
            //arrange
            var reportId = ExistingReportWithAlertWithCountThreshold1Id;
            var report = _nyssContextMock.Reports.Single(r => r.Id == reportId);

            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            report.Status.ShouldBe(ReportStatus.Rejected);
        }

        [Fact]
        public async Task ReportDismissed_WhenDismissingReportInAlertWithCountThreshold1_ShouldSaveChangesOnce()
        {
            //arrange
            var reportId = ExistingReportWithAlertWithCountThreshold1Id;
            
            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportDismissed_WhenTheReportStillHasAGroupThatMeetsCountThreshold_ShouldNotRejectAlert()
        {
            //arrange
            var reportId = DismissedReportInAlert2BrokenApartId;
            var alert = _nyssContextMock.Alerts.Single(a => a.Id == AlertBrokenApart2Id);

            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            alert.Status.ShouldBe(AlertStatus.Pending);
        }


        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymore_ShouldRejectAlert()
        {
            //arrange
            var reportId = DismissedReportInAlert1BrokenApartId;
            var alert = _nyssContextMock.Alerts.Single(a => a.Id == AlertBrokenApart1Id);

            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            alert.Status.ShouldBe(AlertStatus.Rejected);
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymore_ShouldCallSaveChanges3Times()
        {
            //arrange
            var reportId = DismissedReportInAlert1BrokenApartId;

            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            await _nyssContextMock.Received(3).SaveChangesAsync();
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert_AddedPointsToAnotherReport()
        {
            //arrange
            var reportId = DismissedReportInAlert1BrokenApartId;

            var alertBrokenApart = _nyssContextMock.Alerts.Single(a => a.Id == AlertBrokenApart1Id);
            var otherAlert = _nyssContextMock.Alerts.Single(a => a.Id == ExistingAlertId);

            var reportsInGroupOfAnotherAlert = _nyssContextMock.AlertReports
                .Where(ar => ar.Alert == alertBrokenApart)
                .Where(ar => ar.Report.ReportGroupLabel == _labelForGroup3)
                .Select(ar => ar.Report)
                .ToList();
            
            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            await _nyssContextMock.AlertReports.Received(1).AddRangeAsync(Arg.Is<IEnumerable<AlertReport>>(list =>
                list.Count() == 2
                && list.Any(ar => ar.Alert == otherAlert && ar.Report == reportsInGroupOfAnotherAlert[0])
                && list.Any(ar => ar.Alert == otherAlert && ar.Report == reportsInGroupOfAnotherAlert[1])
            ));
        }

        [Fact]
        public async Task ReportDismissed_WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert_MovedAcceptedReportsShouldNotChangeStatus()
        {
            //arrange
            var reportId = DismissedReportInAlert1BrokenApartId;

            var alertBrokenApart = _nyssContextMock.Alerts.Single(a => a.Id == AlertBrokenApart1Id);
            var otherAlert = _nyssContextMock.Alerts.Single(a => a.Id == ExistingAlertId);

            var reportsInGroupOfAnotherAlert = _nyssContextMock.AlertReports
                .Where(ar => ar.Alert == alertBrokenApart)
                .Where(ar => ar.Report.ReportGroupLabel == _labelForGroup3)
                .Select(ar => ar.Report)
                .ToList();

            var acceptedReport = reportsInGroupOfAnotherAlert.Single(r => r.Id == ReportInAlert1BrokenApartId1 && r.Status == ReportStatus.Accepted);

            //act
            await _alertService.ReportDismissed(reportId);

            //assert
            acceptedReport.Status.ShouldBe(ReportStatus.Accepted);;
        }
    }
}





