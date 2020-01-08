using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.ProjectDashboard;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Utils;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.ProjectDashboard
{
    public class ProjectDashboardDataServiceTests
    {
        private readonly IProjectDashboardDataService _projectDashboardDataService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INyssContext _nyssContext;
        private readonly ProjectDashboardTestData _testData;
        private IConfig _config;

        public ProjectDashboardDataServiceTests()
        {
            _dateTimeProvider = new DateTimeProvider();
            _testData = new ProjectDashboardTestData(_dateTimeProvider);

            _nyssContext = _testData.GetNyssContextMock();
            _config = Substitute.For<IConfig>();
            _projectDashboardDataService = new ProjectDashboardDataService(_nyssContext,_dateTimeProvider, _config);
        }

        [Theory]
        [InlineData(true, 8, 4, 0, 2, 1 ,1)]
        [InlineData(false, 24, 12, 0, 2, 3, 3)]
        public async Task GetSummaryData_FilterReportsOnlyOnTraining_ShouldShowAllReportsAccordingToTrainingStatus(
            bool isTraining,
            int expectedReportsCount,
            int expectedActiveCollectorsCount,
            int expectedInactiveCollectorsCount,
            int expectedFromOtherVillagesCount,
            int expectedReferredToHospitalCount,
            int expectedDeathCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000,01,01),
                EndDate = new DateTime(2100,01,01),
                HealthRiskId = null,
                Area = null,
                IsTraining = isTraining,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
            summaryData.DataCollectionPointSummary.FromOtherVillagesCount.ShouldBe(expectedFromOtherVillagesCount);
            summaryData.DataCollectionPointSummary.ReferredToHospitalCount.ShouldBe(expectedReferredToHospitalCount);
            summaryData.DataCollectionPointSummary.DeathCount.ShouldBe(expectedDeathCount);
        }

        [Theory]
        [InlineData(DataCollectorType.Human, false, 16, 8, 0, 0, 0, 0)]
        [InlineData(DataCollectorType.CollectionPoint, false, 8, 4, 0, 2, 3, 3)]
        [InlineData(DataCollectorType.Human, true, 4, 2, 0, 0, 0, 0)]
        [InlineData(DataCollectorType.CollectionPoint, true, 4, 2, 0, 2, 1, 1)]
        public async Task GetSummaryData_FilterReportsOnDataCollectorTypeAndTrainingStatus_ShouldShowAllReportsAccordingToDataCollectorType(
            DataCollectorType dataCollectorType,
            bool isTraining,
            int expectedReportsCount,
            int expectedActiveCollectorsCount,
            int expectedInactiveCollectorsCount,
            int expectedFromOtherVillagesCount,
            int expectedReferredToHospitalCount,
            int expectedDeathCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = null,
                IsTraining = isTraining,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = dataCollectorType == DataCollectorType.Human
                    ? FiltersRequestDto.ReportsTypeDto.DataCollector
                    : FiltersRequestDto.ReportsTypeDto.DataCollectionPoint
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
            summaryData.DataCollectionPointSummary.FromOtherVillagesCount.ShouldBe(expectedFromOtherVillagesCount);
            summaryData.DataCollectionPointSummary.ReferredToHospitalCount.ShouldBe(expectedReferredToHospitalCount);
            summaryData.DataCollectionPointSummary.DeathCount.ShouldBe(expectedDeathCount);
        }

        [Theory]
        [InlineData(1, true, 3, 4, 0, 0, 1, 0)]
        [InlineData(1, false, 8, 12, 0, 0, 3, 0)]
        [InlineData(2, true, 5, 4, 0, 2, 0, 1)]
        [InlineData(2, false, 16, 12, 0, 2, 0, 3)]
        public async Task GetSummaryData_FilterReportsOnHealthRisks_ShouldReturnCorrectNumbers(
            int healthRiskId,
            bool isTraining,
            int expectedReportsCount,
            int expectedActiveCollectorsCount,
            int expectedInactiveCollectorsCount,
            int expectedFromOtherVillagesCount,
            int expectedReferredToHospitalCount,
            int expectedDeathCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = healthRiskId,
                Area = null,
                IsTraining = isTraining,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
            summaryData.DataCollectionPointSummary.FromOtherVillagesCount.ShouldBe(expectedFromOtherVillagesCount);
            summaryData.DataCollectionPointSummary.ReferredToHospitalCount.ShouldBe(expectedReferredToHospitalCount);
            summaryData.DataCollectionPointSummary.DeathCount.ShouldBe(expectedDeathCount);
        }

        [Theory]
        [InlineData(1, false, 12, 6, 0, 0, 0, 0)]
        [InlineData(2, false, 12, 6, 0, 2, 3, 3)]
        [InlineData(1, true, 4, 2, 0, 0, 0, 0)]
        [InlineData(2, true, 4, 2, 0, 2, 1, 1)]
        public async Task GetSummaryData_FilterReportsOnRegion_ShouldReturnCorrectNumbers(
            int regionId,
            bool isTraining,
            int expectedReportsCount,
            int expectedActiveCollectorsCount,
            int expectedInactiveCollectorsCount,
            int expectedFromOtherVillagesCount,
            int expectedReferredToHospitalCount,
            int expectedDeathCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = new AreaDto { Id = regionId, Type = AreaDto.AreaTypeDto.Region },
                IsTraining = isTraining,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
            summaryData.DataCollectionPointSummary.FromOtherVillagesCount.ShouldBe(expectedFromOtherVillagesCount);
            summaryData.DataCollectionPointSummary.ReferredToHospitalCount.ShouldBe(expectedReferredToHospitalCount);
            summaryData.DataCollectionPointSummary.DeathCount.ShouldBe(expectedDeathCount);
        }

        [Theory]
        [InlineData(1, false, 4, 2, 0)]
        [InlineData(2, false, 8, 4, 0)]
        [InlineData(3, false, 4, 2, 0)]
        [InlineData(4, false, 8, 4, 0)]
        [InlineData(1, true, 4, 2, 0)]
        [InlineData(2, true, 0, 0, 0)]
        [InlineData(3, true, 4, 2, 0)]
        [InlineData(4, true, 0, 0, 0)]
        public async Task GetSummaryData_FilterReportsOnDistrict_ShouldReturnCorrectNumbers(int districtId, bool isTraining, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = new AreaDto { Id = districtId, Type = AreaDto.AreaTypeDto.District },
                IsTraining = isTraining,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
        }


        [Theory]
        [InlineData(1, 4, 2, 0)]
        [InlineData(2, 0, 0, 0)]
        [InlineData(3, 0, 0, 0)]
        [InlineData(4, 0, 0, 0)]
        [InlineData(5, 0, 0, 0)]
        [InlineData(6, 4, 2, 0)]
        [InlineData(7, 0, 0, 0)]
        [InlineData(8, 0, 0, 0)]
        public async Task GetSummaryData_FilterMainReportsOnVillage_ShouldReturnCorrectNumbers(int villageId, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = new AreaDto { Id = villageId, Type = AreaDto.AreaTypeDto.Village },
                IsTraining = true,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
        }


        [Theory]
        [InlineData(1, 2, 1, 0)]
        [InlineData(2, 2, 1, 0)]
        [InlineData(3, 0, 0, 0)]
        [InlineData(4, 0, 0, 0)]
        [InlineData(5, 0, 0, 0)]
        [InlineData(6, 0, 0, 0)]
        [InlineData(7, 0, 0, 0)]
        [InlineData(8, 0, 0, 0)]
        [InlineData(9, 0, 0, 0)]
        [InlineData(10, 0, 0, 0)]
        [InlineData(11, 2, 1, 0)]
        [InlineData(12, 2, 1, 0)]
        [InlineData(13, 0, 0, 0)]
        [InlineData(14, 0, 0, 0)]
        [InlineData(15, 0, 0, 0)]
        [InlineData(16, 0, 0, 0)]
        public async Task GetSummaryData_FilterTrainingReportsOnZone_ShouldReturnCorrectNumbers(int zoneId, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = new AreaDto { Id = zoneId, Type = AreaDto.AreaTypeDto.Zone },
                IsTraining = true,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
        }


        [Theory]
        [InlineData(1, 0, 0, 0)]
        [InlineData(2, 0, 0, 0)]
        [InlineData(3, 2, 1, 0)]
        [InlineData(4, 2, 1, 0)]
        [InlineData(5, 2, 1, 0)]
        [InlineData(6, 2, 1, 0)]
        [InlineData(7, 2, 1, 0)]
        [InlineData(8, 2, 1, 0)]
        [InlineData(9, 2, 1, 0)]
        [InlineData(10, 2, 1, 0)]
        [InlineData(11, 0, 0, 0)]
        [InlineData(12, 0, 0, 0)]
        [InlineData(13, 2, 1, 0)]
        [InlineData(14, 2, 1, 0)]
        [InlineData(15, 2, 1, 0)]
        [InlineData(16, 2, 1, 0)]
        public async Task GetSummaryData_FilterMainReportsOnZone_ShouldReturnCorrectNumbers(int zoneId, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = new AreaDto { Id = zoneId, Type = AreaDto.AreaTypeDto.Zone },
                IsTraining = false,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
        }


        [Theory]
        [InlineData("2019-01-01","2019-01-05", 1, 1, 11)]
        [InlineData("2019-01-01","2019-01-10", 6, 3, 9)]
        [InlineData("2019-01-07","2019-12-31", 22, 11, 1)]
        public async Task GetSummaryData_FilterMainReportsByDate_ShouldReturnCorrectNumbers(string startDate, string endDate, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                HealthRiskId = null,
                Area = null,
                IsTraining = false,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
        }

        [Theory]
        [InlineData("2019-01-01", "2019-01-05", 4, 2, 2)]
        [InlineData("2019-01-01", "2019-01-21", 5, 3, 1)]
        [InlineData("2019-01-21", "2019-01-21", 1, 1, 3)]
        public async Task GetSummaryData_FilterTrainingReportsByDate_ShouldReturnCorrectNumbers(string startDate, string endDate, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                HealthRiskId = null,
                Area = null,
                IsTraining = true,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var summaryData = await _projectDashboardDataService.GetSummaryData(_testData.ProjectId, filters);

            //assert
            summaryData.ReportCount.ShouldBe(expectedReportsCount);
            summaryData.ActiveDataCollectorCount.ShouldBe(expectedActiveCollectorsCount);
            summaryData.InactiveDataCollectorCount.ShouldBe(expectedInactiveCollectorsCount);
        }

        [Theory]
        [InlineData(FiltersRequestDto.ReportsTypeDto.All)]
        [InlineData(FiltersRequestDto.ReportsTypeDto.DataCollector)]
        public async Task GetDataCollectionPointReports_WhenReportTypeIsNotDataCollectionPoint_ShouldReturnEmptyList(FiltersRequestDto.ReportsTypeDto reportsTypeFilter)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = null,
                IsTraining = false,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = reportsTypeFilter
            };

            //act
            var result = await _projectDashboardDataService.GetDataCollectionPointReports(_testData.ProjectId, filters);

            //assert
            result.Count.ShouldBe(0);
        }

        [Theory]
        [InlineData("2019-01-01", "2019-01-05", 0)]
        [InlineData("2019-01-25", "2019-01-25", 1)]
        [InlineData("2019-01-25", "2019-01-28", 4)]
        [InlineData("2019-01-25", "2019-12-31", 8)]
        public async Task GetDataCollectionPointReports_WhenFilteringDataCollectionPointsByDate_ShouldReturnCorrectNumberOfDataCollectionPointReports(string startDate, string endDate, int expectedCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate),
                HealthRiskId = null,
                Area = null,
                IsTraining = false,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.DataCollectionPoint
            };

            //act
            var result = await _projectDashboardDataService.GetDataCollectionPointReports(_testData.ProjectId, filters);

            //assert
            result.Count(r => r.FromOtherVillagesCount!= 0 || r.DeathCount != 0 || r.ReferredCount != 0)
                .ShouldBe(expectedCount);
        }

        [Theory]
        [InlineData(3, 0, 0, 0)]
        [InlineData(4, 0, 1, 1)]
        [InlineData(5, 2, 2, 2)]
        [InlineData(6, 0, 0, 0)]
        public async Task GetDataCollectionPointReports_WhenGroupingByWeekAndFilteringForAllDates_ShouldReturnCorrectNumbers(int epiWeek,
            int fromOtherVillagesCount,
            int referredToHospitalCount,
            int deathCount )
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2019, 01, 01),
                EndDate = new DateTime(2019, 02, 15),
                HealthRiskId = null,
                Area = null,
                IsTraining = false,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Week,
                ReportsType = FiltersRequestDto.ReportsTypeDto.DataCollectionPoint
            };

            //act
            var result = await _projectDashboardDataService.GetDataCollectionPointReports(_testData.ProjectId, filters);

            //assert
            result.Where(x => x.Period == epiWeek.ToString())
                .Select(x => x.FromOtherVillagesCount).Single().ShouldBe(fromOtherVillagesCount);

            result.Where(x => x.Period == epiWeek.ToString())
                .Select(x => x.ReferredCount).Single().ShouldBe(referredToHospitalCount);

            result.Where(x => x.Period == epiWeek.ToString())
                .Select(x => x.DeathCount).Single().ShouldBe(deathCount);

        }

        [Theory]
        [InlineData("21/01", true, 1, 0, 0)]
        [InlineData("22/01", true, 0, 1, 0)]
        [InlineData("23/01", true, 0, 0, 1)]
        [InlineData("24/01", true, 1, 0, 0)]
        [InlineData("25/01", false, 0, 1, 0)]
        [InlineData("26/01", false, 0, 0, 1)]
        [InlineData("27/01", false, 1, 0, 0)]
        [InlineData("28/01", false, 0, 1, 0)]
        [InlineData("29/01", false, 0, 0, 1)]
        [InlineData("30/01", false, 1, 0, 0)]
        [InlineData("31/01", false, 0, 1, 0)]
        [InlineData("01/02", false, 0, 0, 1)]
        public async Task GetDataCollectionPointReports_WhenGroupingByDayAndFilteringForAllDates_ShouldReturnCorrectNumbers(string day,
            bool isTraining,
            int fromOtherVillagesCount,
            int referredToHospitalCount,
            int deathCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2019, 01, 01),
                EndDate = new DateTime(2019, 02, 15),
                HealthRiskId = null,
                Area = null,
                IsTraining = isTraining,
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.DataCollectionPoint
            };

            //act
            var result = await _projectDashboardDataService.GetDataCollectionPointReports(_testData.ProjectId, filters);

            //assert
            result.Where(x => x.Period == day)
                .Select(x => x.FromOtherVillagesCount).Single().ShouldBe(fromOtherVillagesCount);

            result.Where(x => x.Period == day)
                .Select(x => x.ReferredCount).Single().ShouldBe(referredToHospitalCount);

            result.Where(x => x.Period == day)
                .Select(x => x.DeathCount).Single().ShouldBe(deathCount);

        }

        [Fact]
        public async Task GetReportsGroupedByFeaturesAndDate_ShouldTakeOnlyMostActiveVillages()
        {
            var numberOfGroupedVillagesInProjectDashboard = 3;

            _config.View.Returns(new NyssConfig.ViewOptions
            {
                NumberOfGroupedVillagesInProjectDashboard = numberOfGroupedVillagesInProjectDashboard
            });

            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2019, 01, 01),
                EndDate = new DateTime(2019, 02, 15),
                GroupingType = FiltersRequestDto.GroupingTypeDto.Day,
                ReportsType = FiltersRequestDto.ReportsTypeDto.All
            };

            //act
            var reportData = await _projectDashboardDataService.GetReportsGroupedByVillageAndDate(_testData.ProjectId, filters);

            //assert
            reportData.Villages.Count().ShouldBe(numberOfGroupedVillagesInProjectDashboard + 1);
            reportData.Villages.ElementAt(numberOfGroupedVillagesInProjectDashboard).Name.ShouldBe("(rest)");
        }
    }
}
