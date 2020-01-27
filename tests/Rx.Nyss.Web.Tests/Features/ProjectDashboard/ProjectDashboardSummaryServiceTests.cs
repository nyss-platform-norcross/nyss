using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectDashboard;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.ProjectDashboard
{
    public class ProjectDashboardSummaryServiceTests
    {
        private readonly ProjectDashboardSummaryService _projectDashboardDataService;
        private readonly IReportService _reportService;
        private readonly List<DataCollector> _dataCollectors;
        private readonly List<Project> _projects;

        private const int ProjectId = 1;

        public ProjectDashboardSummaryServiceTests()
        {
            _projects = new List<Project>
            {
                new Project { Id = ProjectId }
            };

            var alerts = new List<Alert>();
            _dataCollectors = new List<DataCollector>();

            var projectsDbSet = _projects.AsQueryable().BuildMockDbSet();
            var alertsDbSet = alerts.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = _dataCollectors.AsQueryable().BuildMockDbSet();

            var reportsDashboardSummaryService = Substitute.For<IReportsDashboardSummaryService>();
            var nyssContext = Substitute.For<INyssContext>();
            nyssContext.Projects.Returns(projectsDbSet);
            nyssContext.Alerts.Returns(alertsDbSet);
            nyssContext.DataCollectors.Returns(dataCollectorsDbSet);

            _reportService = Substitute.For<IReportService>();
            _projectDashboardDataService = new ProjectDashboardSummaryService(_reportService, nyssContext, reportsDashboardSummaryService);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectReportCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };
            var reports = new List<Report>
            {
                new Report { ReportedCaseCount = 2 },
                new Report { ReportedCaseCount = 3 },
            };

            _reportService.GetHealthRiskEventReportsQuery(filters).Returns(reports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.ReportCount.ShouldBe(5);
        }


        [Fact]
        public async Task GetSummaryData_ReturnsCorrectActiveDataCollectorCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };

            var rawReports = new List<RawReport>
            {
                new RawReport { DataCollector = new DataCollector { Id = 1 } },
                new RawReport { DataCollector = new DataCollector { Id = 1 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } },
                new RawReport { DataCollector = new DataCollector { Id = 3 } }
            };

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.ActiveDataCollectorCount.ShouldBe(3);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectInactiveDataCollectorCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };

            var rawReports = new List<RawReport>
            {
                new RawReport { DataCollector = new DataCollector { Id = 1 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } }
            };

            _dataCollectors.AddRange(new []
            {
                new DataCollector { Id = 1, Project = _projects.First() },
                new DataCollector { Id = 2, Project = _projects.First() },
                new DataCollector { Id = 3, Project = _projects.First() },
                new DataCollector { Id = 4, Project = _projects.First() },
            });

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.ActiveDataCollectorCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectErrorReportCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };

            var allReportsCount = 3;
            var validReportsCount = 2;
            var expectedErrorReportsCount = 1;

            var reports = Enumerable.Range(0, validReportsCount).Select(i => new Report());
            var rawReports = Enumerable.Range(0, allReportsCount).Select(i => new RawReport { DataCollector = new DataCollector() });

            _reportService.GetSuccessReportsQuery(filters).Returns(reports.AsQueryable());
            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.ErrorReportCount.ShouldBe(expectedErrorReportsCount);
        }
    }
}
