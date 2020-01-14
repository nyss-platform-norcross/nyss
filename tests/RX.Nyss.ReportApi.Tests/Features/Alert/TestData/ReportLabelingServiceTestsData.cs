using System;
using System.Linq;
using GeoCoordinatePortable;
using NetTopologySuite.Geometries;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.TestData.TestDataGeneration;

namespace RX.Nyss.ReportApi.Tests.Features.Alert.TestData
{
    public class ReportLabelingServiceTestsData
    {
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

        public class PointLabelingAdditionalData
        {
            public Report ReportBeingAdded { get; set; }
            public Guid LabelOfNewPointAfterAdding { get; set; }
            public Guid Group1Label { get; set; }
            public Guid Group2Label { get; set; }
            public Guid Group3Label { get; set; }
        }
        
        private readonly EntityNumerator _reportNumerator = new EntityNumerator();
        private readonly LabeledReportGroupGenerator _reportGroupGenerator = new LabeledReportGroupGenerator();
        private readonly ReportGenerator _reportGenerator = new ReportGenerator();
        private readonly TestCaseDataProvider _testCaseDataProvider;
        private readonly DataCollector _dataCollector = new DataCollector { DataCollectorType = DataCollectorType.Human };

        public ReportLabelingServiceTestsData(INyssContext nyssContextMock)
        {
            _testCaseDataProvider = new TestCaseDataProvider(nyssContextMock);
        }
        private Point GetMockPoint(double lat, double lon) =>
            new MockPoint(lon, lat);

        public TestCaseData WhenNoReportsInKilometerRange =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenNoReportsInKilometerRange), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var reportGroup = _reportGroupGenerator.Create("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.411269, 17.025807), DateTime.UtcNow);

                data.Reports = reportGroup.Reports;

                return data;
            });

        public TestCaseData WhenNoReportsInTimeRange =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenNoReportsInTimeRange), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var reportGroup = _reportGroupGenerator.Create("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.330898, 17.047525), DateTime.UtcNow)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.329331, 17.046055), DateTime.UtcNow.AddDays(-(double)projectHealthRiskWithKmThresholdOf1.AlertRule.DaysThreshold));

                data.Reports = reportGroup.Reports;

                return data;
            });

        public TestCaseData WhenTwoMatchingReportsInRange =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenTwoMatchingReportsInRange), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var reportGroup1 = _reportGroupGenerator.Create("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.350383, 16.837279), DateTime.UtcNow);

                var reportGroup2 = _reportGroupGenerator.Create("b4328e8f-d834-4d9e-a37c-9a6c1f11c25c")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.344401, 16.823278), DateTime.UtcNow);
                
                var reportGroup3 = _reportGroupGenerator.Create("b7e399bb-9618-4ac2-b3dc-2bf951d01a0c")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.355015, 16.827793), DateTime.UtcNow);


                data.Reports = reportGroup1.Reports.Concat(reportGroup2.Reports).Concat(reportGroup3.Reports).ToList();

                return data;
            });

         public TestCaseData<PointLabelingAdditionalData> WhenOneReportGroupInRange =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenOneReportGroupInRange), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var group1Label = "93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19";
                var reportGroup1 = _reportGroupGenerator.Create(group1Label)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.350383, 16.837279), DateTime.UtcNow)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.344401, 16.823278), DateTime.UtcNow);

                var addedPointGroup = _reportGroupGenerator.Create("b7e399bb-9618-4ac2-b3dc-2bf951d01a0c")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.355015, 16.827793), DateTime.UtcNow);

                data.Reports = reportGroup1.Reports.Concat(addedPointGroup.Reports).ToList();

                var additionalData = new PointLabelingAdditionalData { ReportBeingAdded = addedPointGroup.Reports.Single(), LabelOfNewPointAfterAdding = Guid.Parse(group1Label) };

                return (data, additionalData);
            });
         public TestCaseData<PointLabelingAdditionalData> WhenMoreReportGroupsInRange =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenMoreReportGroupsInRange), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var group1Label = "93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19";
                var reportGroup1 = _reportGroupGenerator.Create(group1Label)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389922, 16.795502), DateTime.UtcNow)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389923, 16.795503), DateTime.UtcNow);

                var group2Label = "326cc2ea-bb28-497d-bda2-e60eb7236266";
                var reportGroup2 = _reportGroupGenerator.Create(group2Label)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389924, 16.795504), DateTime.UtcNow)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389925, 16.795505), DateTime.UtcNow)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389926, 16.795506), DateTime.UtcNow);

                var group3Label = "9cc6e1fa-9660-4259-8fe9-77c103d11aac";
                var reportGroup3 = _reportGroupGenerator.Create(group3Label)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false,  GetMockPoint(52.389927, 16.795507), DateTime.UtcNow)
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389928, 16.795508), DateTime.UtcNow);

                var addedPointGroup = _reportGroupGenerator.Create("b7e399bb-9618-4ac2-b3dc-2bf951d01a0c")
                    .AddReport(ReportStatus.New, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.389921, 16.795501), DateTime.UtcNow);

                data.Reports = reportGroup1.Reports
                    .Concat(reportGroup2.Reports)
                    .Concat(reportGroup3.Reports)
                    .Concat(addedPointGroup.Reports)
                    .ToList();

                var additionalData = new PointLabelingAdditionalData
                {
                    ReportBeingAdded = addedPointGroup.Reports.Single(),
                    LabelOfNewPointAfterAdding = Guid.Parse(group1Label),
                    Group1Label = Guid.Parse(group1Label),
                    Group2Label = Guid.Parse(group2Label),
                    Group3Label = Guid.Parse(group3Label)
                };

                return (data, additionalData);
            });

        public TestCaseData<(Guid Label, int KilometerThreshold)> CalculateNewLabelsInLabelGroup_WhenReportsDontMeetDistanceThreshold =>
            _testCaseDataProvider.GetOrCreate(nameof(CalculateNewLabelsInLabelGroup_WhenReportsDontMeetDistanceThreshold), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var group1Label = "93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19";
                var reportGroup1 = _reportGroupGenerator.Create(group1Label)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.456677, 16.824973), DateTime.UtcNow)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.472505, 16.791912), DateTime.UtcNow)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.477636, 16.839442), DateTime.UtcNow);

                data.Reports = reportGroup1.Reports;

                (Guid, int) additionalData = (Guid.Parse(group1Label), projectHealthRiskWithKmThresholdOf1.AlertRule.KilometersThreshold.Value);
                return (data, additionalData);
            });

        public TestCaseData<(Guid Label, int KilometerThreshold, Report Report1InArea, Report Report2InArea, Report Report3InArea, Report ReportFarAway)> CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold =>
            _testCaseDataProvider.GetOrCreate(nameof(CalculateNewLabelsInLabelGroup_WhenReportsMeetDistanceThreshold), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var group1Label = "93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19";
                var reportGroup1 = _reportGroupGenerator.Create(group1Label)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.245277, 16.847248), DateTime.UtcNow)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.233088, 16.812553), DateTime.UtcNow)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.238923, 16.827520), DateTime.UtcNow)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.206755, 16.870095), DateTime.UtcNow);

                data.Reports = reportGroup1.Reports;

                (Guid, int, Report, Report, Report, Report) additionalData = (Guid.Parse(group1Label),
                    projectHealthRiskWithKmThresholdOf1.AlertRule.KilometersThreshold.Value,
                        reportGroup1.Reports[0],
                        reportGroup1.Reports[1],
                        reportGroup1.Reports[2],
                        reportGroup1.Reports[3]
                    );

                return (data, additionalData);
            });

        public TestCaseData<PointLabelingAdditionalData> WhenAddingTrainingReportBetweenPointsWithDifferentLabels =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenAddingTrainingReportBetweenPointsWithDifferentLabels), () =>
            {
                var data = new EntityData();

                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithKmThresholdOf1 = data.ProjectHealthRisks.FirstOrDefault(hr => hr.AlertRule.KilometersThreshold == 1);

                var group1Label = "93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19";
                var reportGroup1 = _reportGroupGenerator.Create(group1Label)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.245276, 16.847248), DateTime.UtcNow);

                var group2Label = "29acbeda-77d4-4914-8eab-8645fc7cf764";
                var reportGroup2 = _reportGroupGenerator.Create(group2Label)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithKmThresholdOf1, _dataCollector, false, GetMockPoint(52.245278, 16.847248), DateTime.UtcNow);

                var trainingReport = _reportGenerator.CreateNewReport(projectHealthRiskWithKmThresholdOf1, _dataCollector, true, GetMockPoint(52.245277, 16.847248), DateTime.UtcNow);

                data.Reports = reportGroup1.Reports.Concat(reportGroup2.Reports).ToList();
                data.Reports.Add(trainingReport);

                return (data, new PointLabelingAdditionalData{ ReportBeingAdded = trainingReport });
            });
    }
}
