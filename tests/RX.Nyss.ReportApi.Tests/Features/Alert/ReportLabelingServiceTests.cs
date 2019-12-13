using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using MockQueryable.NSubstitute;
using NetTopologySuite.Geometries;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Alerts;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Alert
{
    public class ReportLabelingServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IReportLabelingService _reportLabelingService;

        private const int ProjectHealthRiskId1 = 1;
        private const int ProjectHealthRiskId2 = 2;
        private const int AlertRuleId1 = 1;
        private const int AlertRuleId2 = 2;
        private const int AlertRuleDaysThreshold = 10;
        private const int AlertRuleKilometersThreshold = 1;

        private const int ReportInArea1Id = 1;
        private const int FreshReportInArea2Id = 2;
        private const int OldReportInArea2Id = 3;
        private const int ReportInArea3Id1 = 4;
        private const int ReportInArea3Id2 = 5;
        private const int ReportInArea3Id3 = 6;
        private const int ReportInArea3Id4 = 7;
        private const int ReportInArea3Id5 = 8;
        private const int ReportInArea3Id6 = 9;
        private const int ReportInArea3Id7 = 10;
        private const int ReportInArea4Id1 = 11;
        private const int ReportInArea4Id2 = 12;
        private const int ReportInArea4Id3 = 13;
        private const int ReportInArea4Id4 = 14;
        private const int ReportInArea4Id5 = 15;
        private const int ReportInArea4Id6 = 16;
        private const int ReportInArea4Id7 = 17;
        private const int ReportInArea4Id8 = 18;


        private readonly Guid _labelNotInAnyGroup1 = Guid.Parse("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19");
        private readonly Guid _labelForGroup1 = Guid.Parse("CBCA820B-CA76-4E67-9C08-2845B61CAA5B");
        private readonly Guid _labelForGroup2 = Guid.Parse("CF03F15E-96C4-4CAB-A33F-3E725CD057B5");
        private readonly Guid _labelForGroup3 = Guid.Parse("1de994ad-4aed-41c2-9717-9785ec9ed738");
        private readonly Guid _labelForGroup4 = Guid.Parse("17d23aa7-f357-4ae7-bd7d-0b9dcd54edc0");

        public ReportLabelingServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _reportLabelingService = new ReportLabelingService(_nyssContextMock);

            var alertRules = new List<AlertRule>
            {
                new AlertRule{ Id = AlertRuleId1, DaysThreshold = AlertRuleDaysThreshold, KilometersThreshold = AlertRuleKilometersThreshold},
                new AlertRule{ Id = AlertRuleId2, DaysThreshold = AlertRuleDaysThreshold, KilometersThreshold = AlertRuleKilometersThreshold},
            };

            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk { Id = ProjectHealthRiskId1, AlertRule = alertRules[0] },
                new ProjectHealthRisk { Id = ProjectHealthRiskId2, AlertRule = alertRules[0] },
            };

            var reports = new List<Report>
            {
                new Report{ Id = ReportInArea1Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(17.025807, 52.411269), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = FreshReportInArea2Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(17.047525, 52.330898), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = OldReportInArea2Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(17.046055, 52.329331), ReceivedAt = DateTime.UtcNow.AddDays(-AlertRuleDaysThreshold) },

                new Report{ Id = ReportInArea3Id1, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.837279,52.350383), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id2, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.823278, 52.344401), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id3, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.827793, 52.355015), ReceivedAt = DateTime.UtcNow.AddDays(-AlertRuleDaysThreshold).AddSeconds(1) },
                new Report{ Id = ReportInArea3Id4, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.Rejected , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.825793, 52.353015), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id5, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.Removed , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.824793, 52.354015), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id6, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , IsTraining = true, ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.828793, 52.356015), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id7, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[1], Location = GetMockPoint(16.828493, 52.356615), ReceivedAt = DateTime.UtcNow },

                new Report{ Id = ReportInArea4Id1, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795501, 52.389921), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id2, ReportGroupLabel = _labelForGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795502, 52.389922), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id3, ReportGroupLabel = _labelForGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795503, 52.389923), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id4, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795504, 52.389924), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id5, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795505, 52.389925), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id6, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795506, 52.389926), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id7, ReportGroupLabel = _labelForGroup4, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795507, 52.389927), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id8, ReportGroupLabel = _labelForGroup4, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(16.795508, 52.389928), ReceivedAt = DateTime.UtcNow },

            };

            var alertRulesDbSet = alertRules.AsQueryable().BuildMockDbSet();
            var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            var reportsDbSet = reports.AsQueryable().BuildMockDbSet();

            _nyssContextMock.AlertRules.Returns(alertRulesDbSet);
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);
            _nyssContextMock.Reports.Returns(reportsDbSet);
        }

        private Point GetMockPoint(double x, double y)
        {
            var point = Substitute.For<Point>(x, y);
            point.X = x;
            point.Y = y;

            
            point.Distance(Arg.Any<Point>()).Returns(ci =>
            {
                var firstCoordinate = new GeoCoordinate(y, x);

                var secondPoint = ci.Arg<Point>();
                var secondPointCoordinates = secondPoint.CoordinateSequence.ToCoordinateArray()[0];
                
                var secondCoordinate = new GeoCoordinate(secondPointCoordinates.Y, secondPointCoordinates.X);

                return firstCoordinate.GetDistanceTo(secondCoordinate);
            });

            return point;
        }


        [Fact]
        public async Task FindReportsSatisfyingRangeAndTimeRequirements_WhenNoReportsInKilometerRange_ReturnsEmptyList()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == ReportInArea1Id);

            //act
            var reportsInRange = await _reportLabelingService.FindReportsSatisfyingRangeAndTimeRequirements(report, report.ProjectHealthRisk);

            //assert
            reportsInRange.Count.ShouldBe(0);
        }

        [Fact]
        public async Task FindReportsSatisfyingRangeAndTimeRequirements_WhenNoReportsInTimeRange_ReturnsEmptyList()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == FreshReportInArea2Id);

            //act
            var reportsInRange = await _reportLabelingService.FindReportsSatisfyingRangeAndTimeRequirements(report, report.ProjectHealthRisk);

            //assert
            reportsInRange.Count.ShouldBe(0);
        }

        [Fact]
        public async Task FindReportsSatisfyingRangeAndTimeRequirements_WhenTwoMatchingReportsInRange_ShouldReturnListWithTwoReports()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == ReportInArea3Id1);

            //act
            var reportsInRange = await _reportLabelingService.FindReportsSatisfyingRangeAndTimeRequirements(report, report.ProjectHealthRisk);

            //assert
            reportsInRange.Count.ShouldBe(2);
        }

        [Fact]
        public async Task ResolveLabelsOnReportAdded_WhenOneReportGroupInRange_ShouldChangeTheLabelOfAddedPoint()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == ReportInArea3Id1);

            //act
            await _reportLabelingService.ResolveLabelsOnReportAdded(report, report.ProjectHealthRisk);

            //assert
            report.ReportGroupLabel.ShouldBe(_labelForGroup1);
        }
    }
}
