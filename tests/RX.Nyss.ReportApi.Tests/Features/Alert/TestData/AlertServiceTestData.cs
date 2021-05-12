using System.Collections.Generic;
using System.Linq;
using GeoCoordinatePortable;
using NetTopologySuite.Geometries;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.TestData.TestDataGeneration;

namespace RX.Nyss.ReportApi.Tests.Features.Alert.TestData
{
    public class AlertServiceTestData
    {
        private readonly LabeledReportGroupGenerator _reportGroupGenerator = new LabeledReportGroupGenerator();
        private readonly AlertGenerator _alertGenerator = new AlertGenerator();

        private readonly DataCollector _dataCollector = new DataCollector
        {
            DataCollectorType = DataCollectorType.Human,
            Supervisor = new SupervisorUser
            {
                Name = "TestSupervisor",
                PhoneNumber = "+12345678",
                UserNationalSocieties = new List<UserNationalSociety>()
            }
        };

        private readonly TestCaseDataProvider _testCaseDataProvider;


        public ProjectHealthRiskData ProjectHealthRisks { get; set; }

        public TestCaseData<SimpleTestCaseAdditionalData> SimpleCasesData =>
            _testCaseDataProvider.GetOrCreate(nameof(SimpleCasesData), data =>
            {
                var additionalData = new SimpleTestCaseAdditionalData
                {
                    HumanDataCollector = new DataCollector { DataCollectorType = DataCollectorType.Human },
                    CollectionPointDataCollector = new DataCollector { DataCollectorType = DataCollectorType.CollectionPoint }
                };

                var projectHealthRisk = new ProjectHealthRisk { HealthRisk = new HealthRisk() };

                additionalData.HumanDataCollectorReport = new Report
                {
                    ProjectHealthRisk = projectHealthRisk,
                    DataCollector = additionalData.HumanDataCollector
                };
                additionalData.DataCollectionPointReport = new Report
                {
                    ProjectHealthRisk = projectHealthRisk,
                    DataCollector = additionalData.CollectionPointDataCollector
                };
                additionalData.SingleReportWithoutHealthRisk = new Report
                {
                    ProjectHealthRisk = projectHealthRisk,
                    DataCollector = additionalData.HumanDataCollector,
                    ReportType = ReportType.Single
                };

                return additionalData;
            });


        public TestCaseData WhenCountThresholdIsOne =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenCountThresholdIsOne), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf1 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 1);

                var reportGroup = _reportGroupGenerator.Create("93DCD52C-4AD2-45F6-AED4-54CAB1DD3E19")
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf1, _dataCollector, location: GetMockPoint(52.330898, 17.047525));
                data.Reports = reportGroup.Reports;
            });

        public TestCaseData WhenCountThresholdIsThreeAndIsNotSatisfied =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenCountThresholdIsThreeAndIsNotSatisfied), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var reportGroup = _reportGroupGenerator.Create("19378b71-c7cd-43b3-b856-117dc30ee291")
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = reportGroup.Reports;
            });

        public TestCaseData WhenAddingToGroupWithNoAlertAndMeetingThreshold =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenAddingToGroupWithNoAlertAndMeetingThreshold), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var reportGroup = _reportGroupGenerator.Create("CF03F15E-96C4-4CAB-A33F-3E725CD057B5")
                    .AddNReports(3, ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector, location: GetMockPoint(52.329331, 17.046055));
                data.Reports = reportGroup.Reports;
            });

        public TestCaseData WhenAddingToGroupWithAnExistingAlert =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenAddingToGroupWithAnExistingAlert), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var reportGroup = _reportGroupGenerator.Create("CF03F15E-96C4-4CAB-A33F-3E725CD057B5")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector, location: GetMockPoint(52.329331, 17.046055))
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector, location: GetMockPoint(52.329331, 17.046055));
                data.Reports = reportGroup.Reports;

                var reportsToCreateAlertFor = data.Reports.Where(r => r.Status == ReportStatus.Pending).ToList();
                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(reportsToCreateAlertFor);
            });

        public TestCaseData WhenCountThresholdIsZero =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenCountThresholdIsZero), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf0 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 0);

                var groupWithSingleNewAlert = _reportGroupGenerator.Create("25a7b5fd-329e-44bd-8e0f-aea92e333d9b")
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf0, _dataCollector);

                data.Reports = groupWithSingleNewAlert.Reports;
            });

        public TestCaseData WhenThereAreTrainingReportsInLabelGroup =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenThereAreTrainingReportsInLabelGroup), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var groupWithRealAndTrainingReports = _reportGroupGenerator.Create("25a7b5fd-329e-44bd-8e0f-aea92e333d9b")
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector, true)
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector)
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector);

                data.Reports = groupWithRealAndTrainingReports.Reports;
            });

        public TestCaseData WhenAnAlertAreTriggered =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenAddingToGroupWithAnExistingAlert), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);
                var contentLanguage = new ContentLanguage { LanguageCode = "testLanguageCode" };
                var headManager1 = new ManagerUser
                {
                    EmailAddress = "test@org1.com",
                    Name = "HeadManager Organization 1"
                };
                var headManager2 = new ManagerUser
                {
                    EmailAddress = "test@org2.com",
                    Name = "HeadManager Organization 2"
                };

                var organization1 = new Organization
                {
                    Name = "Organization 1",
                    HeadManager = headManager1
                };

                var organization2 = new Organization
                {
                    Name = "Organization 2",
                    HeadManager = headManager2
                };

                var nationalSociety = new NationalSociety
                {
                    ContentLanguage = contentLanguage,
                    DefaultOrganization = organization1,
                    NationalSocietyUsers = new List<UserNationalSociety>
                    {
                        new UserNationalSociety
                        {
                            User = headManager1,
                            Organization = organization1
                        },
                        new UserNationalSociety
                        {
                            User = headManager2,
                            Organization = organization2
                        }
                    }
                };

                projectHealthRiskWithCountThresholdOf3.Project = new Project
                {
                    NationalSociety = nationalSociety,
                    AlertNotHandledNotificationRecipients = new List<AlertNotHandledNotificationRecipient>
                    {
                        new AlertNotHandledNotificationRecipient
                        {
                            Organization = organization1,
                            User = headManager1,
                        },
                        new AlertNotHandledNotificationRecipient
                        {
                            Organization = organization2,
                            User = headManager2
                        }
                    }
                };

                var supervisor1 = new SupervisorUser
                {
                    Name = "Supervisor Organization 1",
                    PhoneNumber = "+22345678",
                    UserNationalSocieties = new List<UserNationalSociety>
                    {
                        new UserNationalSociety
                        {
                            NationalSociety = nationalSociety,
                            Organization = organization1
                        }
                    }
                };

                supervisor1.UserNationalSocieties.First().User = supervisor1;

                var supervisor2 = new SupervisorUser
                {
                    Name = "Supervisor Organization 2",
                    PhoneNumber = "+32345678",
                    UserNationalSocieties = new List<UserNationalSociety>
                    {
                        new UserNationalSociety
                        {
                            NationalSociety = nationalSociety,
                            Organization = organization2
                        }
                    }
                };

                supervisor2.UserNationalSocieties.First().User = supervisor2;

                var dataCollector1 = new DataCollector
                {
                    DataCollectorType = DataCollectorType.Human,
                    Supervisor = supervisor1
                };

                var dataCollector2 = new DataCollector
                {
                    DataCollectorType = DataCollectorType.Human,
                    Supervisor = supervisor2
                };

                projectHealthRiskWithCountThresholdOf3.HealthRisk.LanguageContents = new List<HealthRiskLanguageContent> { new HealthRiskLanguageContent { ContentLanguage = contentLanguage } };

                var reportGroup = _reportGroupGenerator.Create("CF03F15E-96C4-4CAB-A33F-3E725CD057B5")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, dataCollector1, village: new Village { Name = "VillageName" })
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, dataCollector2, village: new Village { Name = "VillageName" })
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, dataCollector1);

                data.Reports = reportGroup.Reports;

                var reportsToCreateAlertFor = data.Reports.Where(r => r.Status == ReportStatus.Pending).ToList();
                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(reportsToCreateAlertFor);
            });

        public TestCaseData WhenAReportIsAddedToExistingAlertLinkedToSupervisorNotAlreadyInTheAlert =>
            _testCaseDataProvider.GetOrCreate((nameof(WhenAReportIsAddedToExistingAlertLinkedToSupervisorNotAlreadyInTheAlert)), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var dc = new DataCollector
                {
                    Id = 2,
                    DataCollectorType = DataCollectorType.Human,
                    Supervisor = new SupervisorUser
                    {
                        Name = "TestSupervisor2",
                        PhoneNumber = "+123456789",
                        UserNationalSocieties = new List<UserNationalSociety>()
                    }
                };
                data.DataCollectors.Add(dc);

                var reportGroup = _reportGroupGenerator.Create("c10c7325-3b43-480a-af0b-2dc83ddb5412")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector, village: new Village { Name = "VillageName" });

                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(reportGroup.Reports);

                reportGroup.AddReport(ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, dc, location: GetMockPoint(52.329331, 17.046055));
                data.Reports = reportGroup.Reports;
            });

        public TestCaseData WhenReportIsAddedAndEscalatedAlertExists =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenReportIsAddedAndEscalatedAlertExists), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var existingReports = _reportGroupGenerator.Create("AAF7C49B-DCC8-4DCD-BC8F-B8BA226BE19C")
                    .AddNReports(3, ReportStatus.Accepted, projectHealthRiskWithCountThresholdOf3, _dataCollector);

                (data.Alerts, data.AlertReports) = _alertGenerator.AddEscalatedAlertForReports(existingReports.Reports);

                existingReports.AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = existingReports.Reports;
            });

        public TestCaseData WhenReportsAreAddedMeetingThresholdAndEscalatedAlertExists =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenReportsAreAddedMeetingThresholdAndEscalatedAlertExists), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var existingReports = _reportGroupGenerator.Create("AAF7C49B-DCC8-4DCD-BC8F-B8BA226BE19C")
                    .AddNReports(3, ReportStatus.Accepted, projectHealthRiskWithCountThresholdOf3, _dataCollector, location: GetMockPoint(52.329331, 17.046055));

                (data.Alerts, data.AlertReports) = _alertGenerator.AddEscalatedAlertForReports(existingReports.Reports);

                existingReports.AddNReports(3, ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector, location: GetMockPoint(52.329331, 17.046055));
                data.Reports = existingReports.Reports;
            });

        public AlertServiceTestData(INyssContext nyssContextMock)
        {
            _testCaseDataProvider = new TestCaseDataProvider(nyssContextMock);
        }

        public class SimpleTestCaseAdditionalData
        {
            public const int NotExistingReportId = 9999;
            public DataCollector HumanDataCollector { get; set; }
            public DataCollector CollectionPointDataCollector { get; set; }
            public Report HumanDataCollectorReport { get; set; }
            public Report DataCollectionPointReport { get; set; }
            public Report SingleReportWithoutHealthRisk { get; set; }
        }

        private Point GetMockPoint(double lat, double lon) =>
            new MockPoint(lon, lat);

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
    }
}
