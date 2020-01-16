using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int ReportInArea5Id1 = 19;
        private const int ReportInArea5Id2 = 20;
        private const int ReportInArea5Id3 = 21;
        private const int ReportInArea6Id1 = 22;
        private const int ReportInArea6Id2 = 23;
        private const int ReportInArea6Id3 = 24;
        private const int ReportInArea6ButRemoteId = 25;

        private readonly Guid _labelNotInAnyGroup1 = Guid.Parse("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19");
        private readonly Guid _labelForGroup1 = Guid.Parse("CBCA820B-CA76-4E67-9C08-2845B61CAA5B");
        private readonly Guid _labelForGroup2 = Guid.Parse("CF03F15E-96C4-4CAB-A33F-3E725CD057B5");
        private readonly Guid _labelForGroup3 = Guid.Parse("1de994ad-4aed-41c2-9717-9785ec9ed738");
        private readonly Guid _labelForGroup4 = Guid.Parse("17d23aa7-f357-4ae7-bd7d-0b9dcd54edc0");
        private readonly Guid _labelForGroup5 = Guid.Parse("52e9c6ab-a353-4691-8699-4571592b3486");
        private readonly Guid _labelForGroup6 = Guid.Parse("80b14cce-0555-4eaa-9853-1e2342a9212e");

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
                new Report{ Id = ReportInArea1Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.411269, 17.025807), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = FreshReportInArea2Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.330898, 17.047525), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = OldReportInArea2Id, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.329331, 17.046055), ReceivedAt = DateTime.UtcNow.AddDays(-AlertRuleDaysThreshold) },

                new Report{ Id = ReportInArea3Id1, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.350383, 16.837279), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id2, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.344401, 16.823278), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id3, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.355015, 16.827793), ReceivedAt = DateTime.UtcNow.AddDays(-AlertRuleDaysThreshold).AddSeconds(1) },
                new Report{ Id = ReportInArea3Id4, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.Rejected , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.353015, 16.825793), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id5, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.Removed , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.354015, 16.824793), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id6, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , IsTraining = true, ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.356015, 16.828793), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea3Id7, ReportGroupLabel = _labelForGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[1], Location = GetMockPoint(52.356615, 16.828493), ReceivedAt = DateTime.UtcNow },

                new Report{ Id = ReportInArea4Id1, ReportGroupLabel = _labelNotInAnyGroup1, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389921, 16.795501), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id2, ReportGroupLabel = _labelForGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389922, 16.795502), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id3, ReportGroupLabel = _labelForGroup2, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389923, 16.795503), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id4, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389924, 16.795504), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id5, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389925, 16.795505), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id6, ReportGroupLabel = _labelForGroup3, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389926, 16.795506), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id7, ReportGroupLabel = _labelForGroup4, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389927, 16.795507), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea4Id8, ReportGroupLabel = _labelForGroup4, Status = ReportStatus.New , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.389928, 16.795508), ReceivedAt = DateTime.UtcNow },

                new Report{ Id = ReportInArea5Id1, ReportGroupLabel = _labelForGroup5, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.456677,16.824973), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea5Id2, ReportGroupLabel = _labelForGroup5, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.472505, 16.791912), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea5Id3, ReportGroupLabel = _labelForGroup5, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.477636, 16.839442), ReceivedAt = DateTime.UtcNow },

                new Report{ Id = ReportInArea6Id1, ReportGroupLabel = _labelForGroup6, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.245277, 16.847248), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea6Id2, ReportGroupLabel = _labelForGroup6, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.233088, 16.812553 ), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea6Id3, ReportGroupLabel = _labelForGroup6, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.238923, 16.827520), ReceivedAt = DateTime.UtcNow },
                new Report{ Id = ReportInArea6ButRemoteId, ReportGroupLabel = _labelForGroup6, Status = ReportStatus.Pending , ProjectHealthRisk = projectHealthRisks[0], Location = GetMockPoint(52.206755, 16.870095), ReceivedAt = DateTime.UtcNow },
            };

            var alertRulesDbSet = alertRules.AsQueryable().BuildMockDbSet();
            var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            var reportsDbSet = reports.AsQueryable().BuildMockDbSet();

            _nyssContextMock.AlertRules.Returns(alertRulesDbSet);
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);
            _nyssContextMock.Reports.Returns(reportsDbSet);
        }

        private class MockPoint : Point
        {
            public MockPoint(double x, double y)
                : base(x, y)
            {
            }

            public override double Distance(Geometry g)
            {
                var firstCoordinate = new GeoCoordinate(Y, X);
                var secondCoordinate = new GeoCoordinate(g.Coordinate.Y, g.Coordinate.X);
                return firstCoordinate.GetDistanceTo(secondCoordinate);
            }
        }

        private Point GetMockPoint(double lat, double lon) =>
            new MockPoint(lon, lat);


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

        [Fact]
        public async Task ResolveLabelsOnReportAdded_WhenMoreReportGroupsInRange_ShouldMergeReportGroups()
        {
            //arrange
            var report = _nyssContextMock.Reports.Single(r => r.Id == ReportInArea4Id1);
            var reportsThatShouldBeAffected = _nyssContextMock.Reports
                .Where(r => r.ReportGroupLabel == _labelForGroup2 || r.ReportGroupLabel == _labelForGroup3 || r.ReportGroupLabel == _labelForGroup4);

            //act
            await _reportLabelingService.ResolveLabelsOnReportAdded(report, report.ProjectHealthRisk);

            //assert
            var newReportGroup = report.ReportGroupLabel;
            var expectedUpdateCommandWithParametersFilled = $"UPDATE nyss.Reports SET ReportGroupLabel={newReportGroup} WHERE ReportGroupLabel IN ({_labelForGroup2}, {_labelForGroup3}, {_labelForGroup4})";

            await _nyssContextMock.Received(1).ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(x => x.ToString() == expectedUpdateCommandWithParametersFilled));
        }

        [Fact]
        public async Task UpdateLabelsInDatabaseDirect_NyssContextShouldReceiveExpectedSqlCommands()
        {
            //arrange
            var pointWithLabels = new List<(int, Guid)>
            {
                (1, _labelForGroup1),
                (2, _labelForGroup1),
                (3, _labelForGroup2),
                (4, _labelForGroup2),
            };
         
            //act
            await _reportLabelingService.UpdateLabelsInDatabaseDirect(pointWithLabels);

            //assert
            await _nyssContextMock.Received(2).ExecuteSqlInterpolatedAsync(Arg.Any<FormattableString>());
            await _nyssContextMock.Received(1).ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(x =>
                x.ToString() == $"UPDATE nyss.Reports SET ReportGroupLabel={_labelForGroup1} WHERE Id IN (1, 2)"));
            await _nyssContextMock.Received(1).ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(x =>
                x.ToString() == $"UPDATE nyss.Reports SET ReportGroupLabel={_labelForGroup2} WHERE Id IN (3, 4)"));
        }

        [Fact]
        public async Task CalculateNewLabelsInLabelGroup_IfReportsDontMeetDistanceThreshold_ShouldAssignDifferentLabels()
        {
            //arrange
            var distanceThreshold = AlertRuleKilometersThreshold;

            //act
            var pointsWithNewLabels = (await _reportLabelingService.CalculateNewLabelsInLabelGroup(_labelForGroup5, distanceThreshold * 1000 * 2, null)).ToList();

            //assert
            pointsWithNewLabels.Count().ShouldBe(3);
            pointsWithNewLabels[0].Label.ShouldNotBe(_labelForGroup5);
            pointsWithNewLabels[1].Label.ShouldNotBe(_labelForGroup5);
            pointsWithNewLabels[2].Label.ShouldNotBe(_labelForGroup5);

            pointsWithNewLabels[0].Label.ShouldNotBe(pointsWithNewLabels[1].Label);
            pointsWithNewLabels[1].Label.ShouldNotBe(pointsWithNewLabels[2].Label);
            pointsWithNewLabels[2].Label.ShouldNotBe(pointsWithNewLabels[0].Label);
        }


        [Fact]
        public async Task CalculateNewLabelsInLabelGroup_IfReportsMeetDistanceThreshold_ShouldAssignNewLabelToAllMeetingTheThreshold()
        {
            //arrange
            var distanceThreshold = AlertRuleKilometersThreshold;

            //act
            var pointsWithNewLabels = (await _reportLabelingService.CalculateNewLabelsInLabelGroup(_labelForGroup6, distanceThreshold * 1000 * 2, null)).ToList();

            //assert
            var firstPointInGroup = pointsWithNewLabels.Single(x => x.ReportId == ReportInArea6Id1);
            var secondPointInGroup = pointsWithNewLabels.Single(x => x.ReportId == ReportInArea6Id2);
            var thirdPointInGroup = pointsWithNewLabels.Single(x => x.ReportId == ReportInArea6Id3);
            var pointInGroupButRemote = pointsWithNewLabels.Single(x => x.ReportId == ReportInArea6ButRemoteId);

            pointsWithNewLabels.Count().ShouldBe(4);
            firstPointInGroup.Label.ShouldNotBe(_labelForGroup6);
            secondPointInGroup.Label.ShouldNotBe(_labelForGroup6);
            thirdPointInGroup.Label.ShouldNotBe(_labelForGroup6);
            pointInGroupButRemote.Label.ShouldNotBe(_labelForGroup6);

            firstPointInGroup.Label.ShouldBe(secondPointInGroup.Label);
            secondPointInGroup.Label.ShouldBe(thirdPointInGroup.Label);
            thirdPointInGroup.Label.ShouldBe(firstPointInGroup.Label);

            pointInGroupButRemote.Label.ShouldNotBe(firstPointInGroup.Label);
        }
    }
}
