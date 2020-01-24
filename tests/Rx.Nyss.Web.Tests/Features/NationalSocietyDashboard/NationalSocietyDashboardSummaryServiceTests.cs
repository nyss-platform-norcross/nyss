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
        private readonly NationalSocietyDashboardSummaryService _nationalSocietyDashboardSummaryService;
        private readonly IReportService _reportService;
        private readonly List<DataCollector> _dataCollectors;
        private readonly List<NationalSociety> _nationalSocieties;

        private const int NationalSocietyId = 1;

        public NationalSocietyDashboardSummaryServiceTests()
        {
            _nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety { Id = NationalSocietyId }
            };

            var alerts = new List<Alert>();
            _dataCollectors = new List<DataCollector>();

            var nationalSocietiesDbSet = _nationalSocieties.AsQueryable().BuildMockDbSet();
            var alertsDbSet = alerts.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = _dataCollectors.AsQueryable().BuildMockDbSet();

            var reportsDashboardSummaryService = Substitute.For<IReportsDashboardSummaryService>();
            var nyssContext = Substitute.For<INyssContext>();
            nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
            nyssContext.Alerts.Returns(alertsDbSet);
            nyssContext.DataCollectors.Returns(dataCollectorsDbSet);

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
                new Report { ReportedCaseCount = 3 },
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
                new RawReport { DataCollector = new DataCollector { Id = 1 } },
                new RawReport { DataCollector = new DataCollector { Id = 1 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } },
                new RawReport { DataCollector = new DataCollector { Id = 3 } }
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
                new RawReport { DataCollector = new DataCollector { Id = 1 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } },
                new RawReport { DataCollector = new DataCollector { Id = 2 } }
            };

            _dataCollectors.AddRange(new []
            {
                new DataCollector { Id = 1, Project = new Project { NationalSociety = _nationalSocieties.First() } },
                new DataCollector { Id = 2, Project = new Project { NationalSociety = _nationalSocieties.First() } },
                new DataCollector { Id = 3, Project = new Project { NationalSociety = _nationalSocieties.First() } },
                new DataCollector { Id = 4, Project = new Project { NationalSociety = _nationalSocieties.First() } },
            });

            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.ActiveDataCollectorCount.ShouldBe(2);
        }

        [Fact]
        public async Task GetSummaryData_ReturnsCorrectErrorReportCount()
        {
            var filters = new ReportsFilter { NationalSocietyId = NationalSocietyId };

            var allReportsCount = 3;
            var validReportsCount = 2;
            var expectedErrorReportsCount = 1;

            var reports = Enumerable.Range(0, validReportsCount).Select(i => new Report());
            var rawReports = Enumerable.Range(0, allReportsCount).Select(i => new RawReport { DataCollector = new DataCollector() });

            _reportService.GetSuccessReportsQuery(filters).Returns(reports.AsQueryable());
            _reportService.GetRawReportsWithDataCollectorQuery(filters).Returns(rawReports.AsQueryable());

            var summaryData = await _nationalSocietyDashboardSummaryService.GetData(filters);

            summaryData.ErrorReportCount.ShouldBe(expectedErrorReportsCount);
        }
    }
}
