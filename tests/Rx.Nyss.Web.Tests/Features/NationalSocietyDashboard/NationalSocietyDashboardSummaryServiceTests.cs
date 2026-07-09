using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
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
            nyssContext.Villages.Returns(villagesDbSet);
            nyssContext.Districts.Returns(districtsDbSet);

            _reportService = Substitute.For<IReportService>();
            _nationalSocietyDashboardSummaryService = new NationalSocietyDashboardSummaryService(_reportService, nyssContext, reportsDashboardSummaryService);
        }

        /*[Fact]
        public async Task GetSummaryData_ReturnsCorrectTotalReportCount()
        {
            var filters = new ReportsFilter
            {
                NationalSocietyId = NationalSocietyId,
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = false,
                    Kept = true,
                    NotCrossChecked = false,
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
                        ReportedCaseCount = 2,
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
                },
            };

            _reportService.GetRawReportsWithDataCollectorQuery(filters)
                .Returns(reports.AsQueryable());
            _reportService.GetDashboardHealthRiskEventReportsQuery(filters)
                .Returns(reports.Select(r => r.Report)
                    .AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.TotalReportCount.ShouldBe(5);
        }


        [Fact]
        public async Task GetSummaryData_ReturnsCorrectActiveDataCollectorCount()
        {
            var filters = new ReportsFilter
            {
                NationalSocietyId = NationalSocietyId,
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = false,
                    Kept = true,
                    NotCrossChecked = false,
                }
            };

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

            _reportService.GetRawReportsWithDataCollectorAndActivityReportsQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.ActiveDataCollectorCount.ShouldBe(3);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectGeographicalCoverageCount()
        {
            var filters = new ReportsFilter
            {
                NationalSocietyId = NationalSocietyId,
                ReportStatus = new ReportStatusFilterDto
                {
                    Dismissed = false,
                    Kept = true,
                    NotCrossChecked = false,
                }
            };
            var reports = new List<RawReport>
            {
                new RawReport
                {
                    DataCollector = _dataCollectors[0],
                    Village = new Village { District = _districts[0] },
                    Report = new Report
                    {
                        ReportedCaseCount = 2,
                        Status = ReportStatus.New,
                    }
                },
                new RawReport
                {
                    DataCollector = _dataCollectors[0],
                    Village = new Village { District = _districts[0] },
                    Report = new Report
                    {
                        ReportedCaseCount = 3,
                        Status = ReportStatus.New,
                    }
                }
            };

            _reportService.GetRawReportsWithDataCollectorAndActivityReportsQuery(filters).Returns(reports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.NumberOfVillages.ShouldBe(2);
            summaryData.NumberOfDistricts.ShouldBe(1);
        }
    */}
}
