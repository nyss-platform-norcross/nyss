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
        private const int ProjectId = 1;
        private readonly ProjectDashboardSummaryService _projectDashboardDataService;
        private readonly IReportService _reportService;
        private readonly List<DataCollector> _dataCollectors;
        private readonly List<Project> _projects;
        private readonly List<Village> _villages;
        private readonly List<District> _districts;
        private readonly INyssContext _nyssContext;

        public ProjectDashboardSummaryServiceTests()
        {
            _projects = new List<Project> { new Project { Id = ProjectId } };
            _districts = new List<District> { new District { Id = 1 } };
            _villages = new List<Village>
            {
                new Village
                {
                    Id = 1,
                    District = _districts.First()
                }
            };

            var alerts = new List<Alert>();
            _dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = 1,
                    Village = _villages.First(),
                    Project = _projects.First()
                }
            };

            var projectsDbSet = _projects.AsQueryable().BuildMockDbSet();
            var alertsDbSet = alerts.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = _dataCollectors.AsQueryable().BuildMockDbSet();
            var districtsDbSet = _districts.AsQueryable().BuildMockDbSet();
            var villagesDbSet = _villages.AsQueryable().BuildMockDbSet();

            var reportsDashboardSummaryService = Substitute.For<IReportsDashboardSummaryService>();
            _nyssContext = Substitute.For<INyssContext>();
            _nyssContext.Projects.Returns(projectsDbSet);
            _nyssContext.Alerts.Returns(alertsDbSet);
            _nyssContext.DataCollectors.Returns(dataCollectorsDbSet);
            _nyssContext.Villages.Returns(villagesDbSet);
            _nyssContext.Districts.Returns(districtsDbSet);

            _reportService = Substitute.For<IReportService>();
            _projectDashboardDataService = new ProjectDashboardSummaryService(_reportService, _nyssContext, reportsDashboardSummaryService);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectReportCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };
            var reports = new List<Report>
            {
                new Report { ReportedCaseCount = 2 },
                new Report { ReportedCaseCount = 3 }
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
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = new Village { District = new District() }
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = new Village { District = new District() }
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = new Village { District = new District() }
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = new Village { District = new District() }
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 3 },
                    Village = new Village { District = new District() }
                }
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
                new RawReport
                {
                    DataCollector = _dataCollectors.First(),
                    Village = new Village { District = new District() }
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = new Village { District = new District() }
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = new Village { District = new District() }
                }
            };

            _dataCollectors.AddRange(new[]
            {
                new DataCollector
                {
                    Id = 2,
                    Project = _projects.First(),
                    Village = _villages.First()
                },
                new DataCollector
                {
                    Id = 3,
                    Project = _projects.First(),
                    Village = _villages.First()
                },
                new DataCollector
                {
                    Id = 4,
                    Project = _projects.First(),
                    Village = _villages.First()
                }
            });

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.InactiveDataCollectorCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectErrorReportCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };

            var allReportsCount = 3;
            var validReportsCount = 2;
            var expectedErrorReportsCount = 1;

            var reports = Enumerable.Range(0, validReportsCount).Select(i => new Report());
            var rawReports = Enumerable.Range(0, allReportsCount).Select(i => new RawReport
            {
                DataCollector = new DataCollector(),
                Village = new Village { District = new District() }
            });

            _reportService.GetSuccessReportsQuery(filters).Returns(reports.AsQueryable());
            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.ErrorReportCount.ShouldBe(expectedErrorReportsCount);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectGeographicalCoverageCount()
        {
            var filters = new ReportsFilter { ProjectId = ProjectId };
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = _villages.First()
                }
            };

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());
            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.NumberOfVillages.ShouldBe(1);
            summaryData.NumberOfDistricts.ShouldBe(1);
        }
    }
}
