using System.Collections.Generic;
using System.Linq;
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
                PhoneNumber = "+12345678"
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
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf1, _dataCollector);
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
                    .AddNReports(3, ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = reportGroup.Reports;
            });

        public TestCaseData WhenAddingToGroupWithAnExistingAlert =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenAddingToGroupWithAnExistingAlert), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var reportGroup = _reportGroupGenerator.Create("CF03F15E-96C4-4CAB-A33F-3E725CD057B5")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector)
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = reportGroup.Reports;

                var reportsToCreateAlertFor = data.Reports.Where(r => r.Status == ReportStatus.Pending).ToList();
                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(reportsToCreateAlertFor);
            });

        public TestCaseData WhenDismissingReportInAlertWithCountThreshold1 =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenDismissingReportInAlertWithCountThreshold1), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf1 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 1);

                var reportGroup = _reportGroupGenerator.Create("CF03F15E-96C4-4CAB-A33F-3E725CD057B5")
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithCountThresholdOf1, _dataCollector);
                data.Reports = reportGroup.Reports;

                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(data.Reports);
            });

        public TestCaseData WhenTheReportStillHasAGroupThatMeetsCountThreshold =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenTheReportStillHasAGroupThatMeetsCountThreshold), data =>

            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();

                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var group1 = _reportGroupGenerator.Create("652b2605-18e5-4ab6-9d4f-185b664a7517")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                var group2 = _reportGroupGenerator.Create("af1681b6-da9d-4790-81de-62584bcf835b")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = group1.Reports.Concat(group2.Reports).ToList();

                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(data.Reports);
            });

        public TestCaseData<DismissReportAdditionalData> WhenNoGroupInAlertSatisfiesCountThresholdAnymore =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenNoGroupInAlertSatisfiesCountThresholdAnymore), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var group1 = _reportGroupGenerator.Create("652b2605-18e5-4ab6-9d4f-185b664a7517")
                    .AddNReports(2, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                var group2 = _reportGroupGenerator.Create("af1681b6-da9d-4790-81de-62584bcf835b")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = group1.Reports.Concat(group2.Reports).ToList();

                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(data.Reports);

                var additionalData = new DismissReportAdditionalData { ReportBeingDismissed = group2.Reports.First() };
                return additionalData;
            });

        public TestCaseData<DismissReportAdditionalData> WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert =>
            _testCaseDataProvider.GetOrCreate(nameof(WhenNoGroupInAlertSatisfiesCountThresholdAnymoreButOneWentToGroupOfAnotherAlert), data =>
            {
                (data.AlertRules, data.HealthRisks, data.ProjectHealthRisks) = ProjectHealthRiskData.Create();
                var projectHealthRiskWithCountThresholdOf3 = data.ProjectHealthRisks.Single(hr => hr.AlertRule.CountThreshold == 3);

                var group1InInvalidatedAlert = _reportGroupGenerator.Create("652b2605-18e5-4ab6-9d4f-185b664a7517")
                    .AddNReports(2, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                var group2InInvalidatedAlert = _reportGroupGenerator.Create("af1681b6-da9d-4790-81de-62584bcf835b")
                    .AddNReports(2, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);

                var groupOfDismissedReport = _reportGroupGenerator.Create("945f1b55-4129-4829-9bd9-01e8323b564a")
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);

                var group4InBothAlerts = _reportGroupGenerator.Create("6b257702-57d1-45c0-9a0d-e02f61d20f00")
                    .AddReport(ReportStatus.Accepted, projectHealthRiskWithCountThresholdOf3, _dataCollector)
                    .AddReport(ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector)
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector);

                data.Reports = group1InInvalidatedAlert.Reports.Concat(group2InInvalidatedAlert.Reports).Concat(groupOfDismissedReport.Reports).Concat(group4InBothAlerts.Reports).ToList();

                var reportsBeingMoved = group4InBothAlerts.Reports.Take(2).ToList();
                var reportsInAlert1 = group1InInvalidatedAlert.Reports
                    .Concat(group2InInvalidatedAlert.Reports)
                    .Concat(groupOfDismissedReport.Reports)
                    .Concat(reportsBeingMoved)
                    .ToList();

                var reportsInAlert2 = group4InBothAlerts.Reports.Concat(group4InBothAlerts.Reports.Skip(2)).ToList();

                var (alerts1, alertReports1) = _alertGenerator.AddPendingAlertForReports(reportsInAlert1);
                var (alerts2, alertReports2) = _alertGenerator.AddPendingAlertForReports(reportsInAlert2);

                data.Alerts = alerts1.Concat(alerts2).ToList();
                data.AlertReports = alertReports1.Concat(alertReports2).ToList();

                var additionalData = new DismissReportAdditionalData
                {
                    ReportBeingDismissed = groupOfDismissedReport.Reports.Single(),
                    AlertThatReceivedNewReports = alerts2.Single(),
                    ReportsBeingMoved = reportsBeingMoved
                };
                return additionalData;
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
                projectHealthRiskWithCountThresholdOf3.Project = new Project
                {
                    NationalSociety = new NationalSociety
                    {
                        ContentLanguage = contentLanguage,
                        HeadManager = new ManagerUser
                        {
                            EmailAddress = "test@example.com",
                            Name = "HeadManager Name"
                        }
                    }
                };

                projectHealthRiskWithCountThresholdOf3.HealthRisk.LanguageContents = new List<HealthRiskLanguageContent> { new HealthRiskLanguageContent { ContentLanguage = contentLanguage } };

                var reportGroup = _reportGroupGenerator.Create("CF03F15E-96C4-4CAB-A33F-3E725CD057B5")
                    .AddNReports(3, ReportStatus.Pending, projectHealthRiskWithCountThresholdOf3, _dataCollector, village: new Village { Name = "VillageName" })
                    .AddReport(ReportStatus.New, projectHealthRiskWithCountThresholdOf3, _dataCollector);
                data.Reports = reportGroup.Reports;

                var reportsToCreateAlertFor = data.Reports.Where(r => r.Status == ReportStatus.Pending).ToList();
                (data.Alerts, data.AlertReports) = _alertGenerator.AddPendingAlertForReports(reportsToCreateAlertFor);
            });

        public AlertServiceTestData(INyssContext nyssContextMock)
        {
            _testCaseDataProvider = new TestCaseDataProvider(nyssContextMock);
        }

        public class DismissReportAdditionalData
        {
            public Report ReportBeingDismissed { get; set; }
            public Data.Models.Alert AlertThatReceivedNewReports { get; set; }
            public List<Report> ReportsBeingMoved { get; set; }
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
    }
}
