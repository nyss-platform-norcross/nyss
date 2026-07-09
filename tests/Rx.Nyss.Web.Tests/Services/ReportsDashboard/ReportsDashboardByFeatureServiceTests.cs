using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Services.ReportsDashboard
{
    public class ReportsDashboardByFeatureServiceTests
    {
        private readonly ReportsDashboardByVillageService _reportsDashboardByVillageService;
        private readonly INyssContext _nyssContext;
        private readonly INyssWebConfig _config;
        private readonly IReportService _reportService;
        private readonly IReportsDashboardByFeatureService _reportsDashboardByFeatureService;

        public ReportsDashboardByFeatureServiceTests()
        {
            IDateTimeProvider dateTimeProvider = new DateTimeProvider();
            var testData = new ReportsDashboardTestData(dateTimeProvider);

            _nyssContext = testData.GetNyssContextMock();
            _config = Substitute.For<INyssWebConfig>();

            _reportService = Substitute.For<IReportService>();
            _reportsDashboardByVillageService = new ReportsDashboardByVillageService(_reportService, dateTimeProvider, _config);
            _reportsDashboardByFeatureService = new ReportsDashboardByFeatureService(_reportService, dateTimeProvider);
        }

        [Fact]
        public async Task GetReportsGroupedByVillageAndDate_ShouldTakeOnlyMostActiveVillages()
        {
            var reports = _nyssContext.Reports;
            _reportService.GetDashboardHealthRiskEventReportsQuery(Arg.Any<ReportsFilter>()).Returns(reports);

            var numberOfGroupedVillagesInProjectDashboard = 3;

            _config.View.Returns(new ConfigSingleton.ViewOptions { NumberOfGroupedVillagesInProjectDashboard = numberOfGroupedVillagesInProjectDashboard });

            //act
            var result = await _reportsDashboardByVillageService.GetReportsGroupedByVillageAndDate(new ReportsFilter(), DatesGroupingType.Day, DayOfWeek.Sunday);

            //assert
            result.Villages.Count().ShouldBe(numberOfGroupedVillagesInProjectDashboard + 1);
            result.Villages.ElementAt(numberOfGroupedVillagesInProjectDashboard).Name.ShouldBe("(rest)");
        }

    }
}
