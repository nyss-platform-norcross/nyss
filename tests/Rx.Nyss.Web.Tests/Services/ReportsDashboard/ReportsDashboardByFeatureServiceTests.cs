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
        private readonly ReportsDashboardByVillageService _reportsDashboardByDataCollectionPointService;
        private readonly INyssContext _nyssContext;
        private readonly INyssWebConfig _config;
        private readonly IReportService _reportService;

        public ReportsDashboardByFeatureServiceTests()
        {
            IDateTimeProvider dateTimeProvider = new DateTimeProvider();
            var testData = new ReportsDashboardTestData(dateTimeProvider);

            _nyssContext = testData.GetNyssContextMock();
            _config = Substitute.For<INyssWebConfig>();

            _reportService = Substitute.For<IReportService>();
            _reportsDashboardByDataCollectionPointService = new ReportsDashboardByVillageService(_reportService, dateTimeProvider, _config);
        }

        [Fact]
        public async Task GetReportsGroupedByFeaturesAndDate_ShouldTakeOnlyMostActiveVillages()
        {
            var reports = _nyssContext.Reports;
            _reportService.GetHealthRiskEventReportsQuery(Arg.Any<ReportsFilter>()).Returns(reports);

            var numberOfGroupedVillagesInProjectDashboard = 3;

            _config.View.Returns(new ConfigSingleton.ViewOptions { NumberOfGroupedVillagesInProjectDashboard = numberOfGroupedVillagesInProjectDashboard });

            //act
            var result = await _reportsDashboardByDataCollectionPointService.GetReportsGroupedByVillageAndDate(new ReportsFilter(), DatesGroupingType.Day);

            //assert
            result.Villages.Count().ShouldBe(numberOfGroupedVillagesInProjectDashboard + 1);
            result.Villages.ElementAt(numberOfGroupedVillagesInProjectDashboard).Name.ShouldBe("(rest)");
        }
    }
}
