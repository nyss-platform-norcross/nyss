using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
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
                    DataCollectorLocations = new List<DataCollectorLocation>
                    {
                        new DataCollectorLocation
                        {
                            Village = _villages.First()
                        }
                    },
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

        /*[Fact]
        public async Task GetSummaryData_ReturnsCorrectTotalReportCount()
        {
            var filters = new ReportsFilter
            {
                ProjectId = ProjectId,
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = true,
                    Kept = true,
                    NotCrossChecked = true,
                }
            };
            var reports = new List<RawReport>
            {
                new RawReport
                {
                    DataCollector = _dataCollectors[0],
                    Village = new Village { District = new District() },
                    Report = new Report
                    {
                        ReportedCaseCount = 1,
                        Status = ReportStatus.Accepted,
                    }
                },
                new RawReport
                {
                    DataCollector = _dataCollectors[0],
                    Village = new Village { District = new District() },
                    Report = new Report
                    {
                        ReportedCaseCount = 1,
                        Status = ReportStatus.Rejected,
                    }
                },
                new RawReport
                {
                    DataCollector = _dataCollectors[0],
                    Village = new Village { District = new District() },
                    Report = new Report
                    {
                        ReportedCaseCount = 1,
                        Status = ReportStatus.New,
                    }
                },
                new RawReport
                {
                    DataCollector = _dataCollectors[0],
                    Village = new Village { District = new District() },
                    Report = new Report
                    {
                        ReportedCaseCount = 1,
                        Status = ReportStatus.Pending,
                    }
                }
            };

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(reports.AsQueryable());
            _reportService.GetDashboardHealthRiskEventReportsQuery(filters).Returns(reports.Select(r => r.Report).AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.TotalReportCount.ShouldBe(4);
        }


        [Fact]
        public async Task GetSummaryData_ReturnsCorrectActiveDataCollectorCount()
        {
            var filters = new ReportsFilter
            {
                ProjectId = ProjectId,
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = true,
                    Kept = true,
                    NotCrossChecked = true,
                }
            };

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

            _reportService.GetRawReportsWithDataCollectorAndActivityReportsQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.ActiveDataCollectorCount.ShouldBe(3);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectGeographicalCoverageCount()
        {
            var filters = new ReportsFilter
            {
                ProjectId = ProjectId,
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = true,
                    Kept = true,
                    NotCrossChecked = true,
                }
            };
            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = _villages.First()
                }
            };

            _reportService.GetRawReportsWithDataCollectorAndActivityReportsQuery(filters).Returns(rawReports.AsQueryable());
            var summaryData = await _projectDashboardDataService.GetData(filters);

            summaryData.NumberOfVillages.ShouldBe(1);
            summaryData.NumberOfDistricts.ShouldBe(1);
        }
    */}
}
