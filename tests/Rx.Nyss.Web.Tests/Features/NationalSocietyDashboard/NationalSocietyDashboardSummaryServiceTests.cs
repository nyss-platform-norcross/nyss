using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSocietyDashboard;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocietyDashboard
{
    public class NationalSocietyDashboardSummaryServiceTests
    {
        private const int NationalSocietyId = 1;
        private readonly NationalSocietyDashboardSummaryService _nationalSocietyDashboardSummaryService;
        private readonly IReportService _reportService;
        private readonly List<DataCollector> _dataCollectors;
        private readonly List<NationalSociety> _nationalSocieties;
        private readonly List<Village> _villages;
        private readonly List<District> _districts;
        private readonly List<Project> _projects;

        public NationalSocietyDashboardSummaryServiceTests()
        {
            _nationalSocieties = new List<NationalSociety> { new NationalSociety { Id = NationalSocietyId } };
            _projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    NationalSociety = _nationalSocieties.First(),
                    NationalSocietyId = NationalSocietyId
                }
            };
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

            var nationalSocietiesDbSet = _nationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsDbSet = _projects.AsQueryable().BuildMockDbSet();
            var alertsDbSet = alerts.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = _dataCollectors.AsQueryable().BuildMockDbSet();
            var districtsDbSet = _districts.AsQueryable().BuildMockDbSet();
            var villagesDbSet = _villages.AsQueryable().BuildMockDbSet();

            var reportsDashboardSummaryService = Substitute.For<IReportsDashboardSummaryService>();
            var nyssContext = Substitute.For<INyssContext>();
            nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
            nyssContext.Projects.Returns(projectsDbSet);
            nyssContext.Alerts.Returns(alertsDbSet);
            nyssContext.DataCollectors.Returns(dataCollectorsDbSet);
            nyssContext.DataCollectors.Returns(dataCollectorsDbSet);
            nyssContext.Villages.Returns(villagesDbSet);
            nyssContext.Districts.Returns(districtsDbSet);

            _reportService = Substitute.For<IReportService>();
            _nationalSocietyDashboardSummaryService = new NationalSocietyDashboardSummaryService(_reportService, nyssContext, reportsDashboardSummaryService);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectReportCount()
        {
            var filters = new ReportsFilter { NationalSocietyId = NationalSocietyId };
            var reports = new List<Report>
            {
                new Report { ReportedCaseCount = 2 },
                new Report { ReportedCaseCount = 3 }
            };

            _reportService.GetHealthRiskEventReportsQuery(filters).Returns(reports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.ReportCount.ShouldBe(5);
        }


        [Fact]
        public async Task GetSummaryData_ReturnsCorrectActiveDataCollectorCount()
        {
            var filters = new ReportsFilter { NationalSocietyId = NationalSocietyId };

            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = _villages.First()
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = _villages.First()
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = _villages.First()
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = _villages.First()
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 3 },
                    Village = _villages.First()
                }
            };

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.ActiveDataCollectorCount.ShouldBe(3);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectInactiveDataCollectorCount()
        {
            var filters = new ReportsFilter { NationalSocietyId = NationalSocietyId };

            var rawReports = new List<RawReport>
            {
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 1 },
                    Village = _villages.First()
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = _villages.First()
                },
                new RawReport
                {
                    DataCollector = new DataCollector { Id = 2 },
                    Village = _villages.First()
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

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.InactiveDataCollectorCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectErrorReportCount()
        {
            var filters = new ReportsFilter { NationalSocietyId = NationalSocietyId };

            var allReportsCount = 3;
            var validReportsCount = 2;
            var expectedErrorReportsCount = 1;

            var reports = Enumerable.Range(0, validReportsCount).Select(i => new Report());
            var rawReports = Enumerable.Range(0, allReportsCount).Select(i => new RawReport
            {
                DataCollector = new DataCollector(),
                Village = _villages.First()
            });

            _reportService.GetSuccessReportsQuery(filters).Returns(reports.AsQueryable());
            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.ErrorReportCount.ShouldBe(expectedErrorReportsCount);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectGeographicalCoverageCount()
        {
            var filters = new ReportsFilter { NationalSocietyId = NationalSocietyId };
            var rawReports = Enumerable.Range(0, 1).Select(i => new RawReport
            {
                DataCollector = new DataCollector(),
                Village = _villages.First()
            });
            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.NumberOfVillages.ShouldBe(1);
            summaryData.NumberOfDistricts.ShouldBe(1);
        }
    }
}
