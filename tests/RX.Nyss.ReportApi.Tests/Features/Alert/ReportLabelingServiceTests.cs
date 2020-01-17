using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Tests.Features.Alert.TestData;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Alert
{
    public class ReportLabelingServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IReportLabelingService _reportLabelingService;
        private readonly ReportLabelingServiceTestsData _testData;

        public ReportLabelingServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _reportLabelingService = new ReportLabelingService(_nyssContextMock);
            _testData = new ReportLabelingServiceTestsData(_nyssContextMock);
        }

        [Fact]
        public async Task FindReportsSatisfyingRangeAndTimeRequirements_WhenNoReportsInKilometerRange_ReturnsEmptyList()
        {
            //arrange
            _testData.WhenNoReportsInKilometerRange.GenerateData();
            var report = _testData.WhenNoReportsInKilometerRange.EntityData.Reports.Single();

            //act
            var reportsInRange = await _reportLabelingService.FindReportsSatisfyingRangeAndTimeRequirements(report, report.ProjectHealthRisk);

            //assert
            reportsInRange.Count.ShouldBe(0);
        }

        [Fact]
        public async Task FindReportsSatisfyingRangeAndTimeRequirements_WhenNoReportsInTimeRange_ReturnsEmptyList()
        {
            //arrange
            _testData.WhenNoReportsInTimeRange.GenerateData();
            var report = _testData.WhenNoReportsInTimeRange.EntityData.Reports.First();

            //act
            var reportsInRange = await _reportLabelingService.FindReportsSatisfyingRangeAndTimeRequirements(report, report.ProjectHealthRisk);

            //assert
            reportsInRange.Count.ShouldBe(0);
        }

        [Fact]
        public async Task FindReportsSatisfyingRangeAndTimeRequirements_WhenTwoMatchingReportsInRange_ShouldReturnListWithTwoReports()
        {
            //arrange
            _testData.WhenTwoMatchingReportsInRange.GenerateData();
            var report = _testData.WhenTwoMatchingReportsInRange.EntityData.Reports.First();

            //act
            var reportsInRange = await _reportLabelingService.FindReportsSatisfyingRangeAndTimeRequirements(report, report.ProjectHealthRisk);

            //assert
            reportsInRange.Count.ShouldBe(2);
        }

        [Fact]
        public async Task ResolveLabelsOnReportAdded_WhenOneReportGroupInRange_ShouldChangeTheLabelOfAddedPoint()
        {
            //arrange
            _testData.WhenOneReportGroupInRange.GenerateData();
            var report = _testData.WhenOneReportGroupInRange.EntityData.Reports.First();

            //act
            await _reportLabelingService.ResolveLabelsOnReportAdded(report, report.ProjectHealthRisk);

            //assert
            report.ReportGroupLabel.ShouldBe(_testData.WhenOneReportGroupInRange.AdditionalData.LabelOfNewPointAfterAdding);
        }

        [Fact]
        public async Task ResolveLabelsOnReportAdded_WhenMoreReportGroupsInRange_ShouldMergeReportGroups()
        {
            //arrange
            _testData.WhenMoreReportGroupsInRange.GenerateData();
            var report = _testData.WhenMoreReportGroupsInRange.AdditionalData.ReportBeingAdded;
            var group1Label = _testData.WhenMoreReportGroupsInRange.AdditionalData.Group1Label;
            var group2Label = _testData.WhenMoreReportGroupsInRange.AdditionalData.Group2Label;
            var group3Label = _testData.WhenMoreReportGroupsInRange.AdditionalData.Group3Label;

            //act
            await _reportLabelingService.ResolveLabelsOnReportAdded(report, report.ProjectHealthRisk);

            //assert
            var newReportGroup = report.ReportGroupLabel;
            var expectedUpdateCommandWithParametersFilled = $"UPDATE nyss.Reports SET ReportGroupLabel={newReportGroup} WHERE ReportGroupLabel IN ({group1Label}, {group2Label}, {group3Label})";

            await _nyssContextMock.Received(1).ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(x => x.ToString() == expectedUpdateCommandWithParametersFilled));
        }

        [Fact]
        public async Task ResolveLabelsOnReportAdded_WhenAddingTrainingReportBetweenPointsWithDifferentLabels_TrainingReportShouldBeIgnoredAndLabelsShouldNotBeJoined()
        {
            //arrange
            _testData.WhenAddingTrainingReportBetweenPointsWithDifferentLabels.GenerateData();
            var addedReport = _testData.WhenAddingTrainingReportBetweenPointsWithDifferentLabels.AdditionalData.ReportBeingAdded;
            var reports = _testData.WhenAddingTrainingReportBetweenPointsWithDifferentLabels.EntityData.Reports;
            //act
            await _reportLabelingService.ResolveLabelsOnReportAdded(addedReport, addedReport.ProjectHealthRisk);

            //assert
            reports[0].ReportGroupLabel.ShouldNotBe(reports[1].ReportGroupLabel);
            reports[1].ReportGroupLabel.ShouldNotBe(reports[2].ReportGroupLabel);
            reports[2].ReportGroupLabel.ShouldNotBe(reports[0].ReportGroupLabel);
        }

        [Fact]
        public async Task UpdateLabelsInDatabaseDirect_NyssContextShouldReceiveExpectedSqlCommands()
        {
            //arrange
            var label1 = Guid.Parse("9ce11e66-4a73-471e-8062-c1c197949b94");
            var label2 = Guid.Parse("e3081ed4-e61e-47c3-b620-0e679a5c9e3d");
            var pointWithLabels = new List<(int, Guid)>
            {
                (1, label1),
                (2, label1),
                (3, label2),
                (4, label2),
            };
         
            //act
            await _reportLabelingService.UpdateLabelsInDatabaseDirect(pointWithLabels);

            //assert
            await _nyssContextMock.Received(2).ExecuteSqlInterpolatedAsync(Arg.Any<FormattableString>());
            await _nyssContextMock.Received(1).ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(x =>
                x.ToString() == $"UPDATE nyss.Reports SET ReportGroupLabel={label1} WHERE Id IN (1, 2)"));
            await _nyssContextMock.Received(1).ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(x =>
                x.ToString() == $"UPDATE nyss.Reports SET ReportGroupLabel={label2} WHERE Id IN (3, 4)"));
        }

        [Fact]
        public async Task CalculateNewLabelsInLabelGroup_WhenReportsDontMeetDistanceThreshold_ShouldAssignDifferentLabels()
        {
            //arrange
            _testData.CalculateNewLabelsInLabelGroup_WhenReportsDontMeetDistanceThreshold.GenerateData();
            var distanceThreshold = _testData.CalculateNewLabelsInLabelGroup_WhenReportsDontMeetDistanceThreshold.AdditionalData.KilometerThreshold;
            var group1Label = _testData.CalculateNewLabelsInLabelGroup_WhenReportsDontMeetDistanceThreshold.AdditionalData.Label;

            //act
            var pointsWithNewLabels = (await _reportLabelingService.CalculateNewLabelsInLabelGroup(group1Label, distanceThreshold * 1000 * 2, null)).ToList();

            //assert
            pointsWithNewLabels.Count().ShouldBe(3);
            pointsWithNewLabels[0].Label.ShouldNotBe(group1Label);
            pointsWithNewLabels[1].Label.ShouldNotBe(group1Label);
            pointsWithNewLabels[2].Label.ShouldNotBe(group1Label);

            pointsWithNewLabels[0].Label.ShouldNotBe(pointsWithNewLabels[1].Label);
            pointsWithNewLabels[1].Label.ShouldNotBe(pointsWithNewLabels[2].Label);
            pointsWithNewLabels[2].Label.ShouldNotBe(pointsWithNewLabels[0].Label);
        }

        [Fact]
        public async Task CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold_ShouldAssignNewLabelToAllMeetingTheThreshold()
        {
            //arrange
            _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.GenerateData();
            var distanceThreshold = _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.AdditionalData.KilometerThreshold;
            var groupLabel = _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.AdditionalData.Label;
            var report1InArea = _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.AdditionalData.Report1InArea;
            var report2InArea = _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.AdditionalData.Report2InArea;
            var report3InArea = _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.AdditionalData.Report3InArea;
            var reportFarAway = _testData.CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold.AdditionalData.ReportFarAway;

            //act
            var pointsWithNewLabels = (await _reportLabelingService.CalculateNewLabelsInLabelGroup(groupLabel, distanceThreshold * 1000 * 2, null)).ToList();

            //assert
            var firstPointInGroup = pointsWithNewLabels.Single(x => x.ReportId == report1InArea.Id);
            var secondPointInGroup = pointsWithNewLabels.Single(x => x.ReportId == report2InArea.Id);
            var thirdPointInGroup = pointsWithNewLabels.Single(x => x.ReportId == report3InArea.Id);
            var pointInGroupButRemote = pointsWithNewLabels.Single(x => x.ReportId == reportFarAway.Id);

            pointsWithNewLabels.Count().ShouldBe(4);
            firstPointInGroup.Label.ShouldNotBe(groupLabel);
            secondPointInGroup.Label.ShouldNotBe(groupLabel);
            thirdPointInGroup.Label.ShouldNotBe(groupLabel);
            pointInGroupButRemote.Label.ShouldNotBe(groupLabel);

            firstPointInGroup.Label.ShouldBe(secondPointInGroup.Label);
            secondPointInGroup.Label.ShouldBe(thirdPointInGroup.Label);
            thirdPointInGroup.Label.ShouldBe(firstPointInGroup.Label);

            pointInGroupButRemote.Label.ShouldNotBe(firstPointInGroup.Label);
        }
    }
}
