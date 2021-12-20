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

        [Fact]
        public async Task GetReportsGroupedByFeaturesAndDate_ShouldReturnCorrectCount()
        {
            // arrange
            var reports = _nyssContext.Reports;
            _reportService.GetDashboardHealthRiskEventReportsQuery(Arg.Any<ReportsFilter>()).Returns(reports);
            var startDate = new DateTime(2018, 11, 28);
            var endDate = new DateTime(2019, 01, 01);

            // act
            var res = await _reportsDashboardByFeatureService.GetReportsGroupedByFeaturesAndDate(new ReportsFilter
            {
                StartDate = startDate,
                EndDate = endDate
            }, DatesGroupingType.Day, DayOfWeek.Sunday);

            var total = res
                .Select(r => r.CountFemalesBelowFive + r.CountUnspecifiedSexAndAge + r.CountMalesBelowFive + r.CountFemalesAtLeastFive + r.CountMalesAtLeastFive)
                .Aggregate(0, (a, c) => a + c);
            var countMaleBelowFive = res.Sum(r => r.CountMalesBelowFive);
            var countMaleAboveFive = res.Sum(r => r.CountMalesAtLeastFive);
            var countFemaleBelowFive = res.Sum(r => r.CountFemalesBelowFive);
            var countFemaleAboveFive = res.Sum(r => r.CountFemalesAtLeastFive);
            var countUnspecified = res.Sum(r => r.CountUnspecifiedSexAndAge);

            total.ShouldBe(32);
            countMaleBelowFive.ShouldBe(7);
            countMaleAboveFive.ShouldBe(7);
            countFemaleBelowFive.ShouldBe(7);
            countFemaleAboveFive.ShouldBe(7);
            countUnspecified.ShouldBe(4);
        }
    }
}
