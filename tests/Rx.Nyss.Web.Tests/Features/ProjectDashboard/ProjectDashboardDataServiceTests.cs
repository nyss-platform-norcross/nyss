using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
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

        public ProjectDashboardDataServiceTests()
        {
            _dateTimeProvider = new DateTimeProvider();
            _testData = new ProjectDashboardTestData(_dateTimeProvider);

            _nyssContext = _testData.GetNyssContextMock();
            _projectDashboardDataService = new ProjectDashboardDataService(_nyssContext,_dateTimeProvider);
        }

        [Theory]
        [InlineData(true, 8, 4, 0)]
        [InlineData(false, 24, 12, 0)]
        public async Task GetSummaryData_FilterReportsOnlyOnTraining_ShouldShowAllReportsAccordingToTrainingStatus(bool isTraining, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
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
        }

        [Theory]
        [InlineData(DataCollectorType.Human, false, 16, 8, 0)]
        [InlineData(DataCollectorType.CollectionPoint, false, 8, 4, 0)]
        [InlineData(DataCollectorType.Human, true, 4, 2, 0)]
        [InlineData(DataCollectorType.CollectionPoint, true, 4, 2, 0)]
        public async Task GetSummaryData_FilterReportsOnDataCollectorTypeAndTrainingStatus_ShouldShowAllReportsAccordingToDataCollectorType(DataCollectorType dataCollectorType, bool isTraining, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
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
        }

        [Theory]
        [InlineData(1, true, 3, 3, 1)]
        [InlineData(1, false, 8, 8, 4)]
        [InlineData(2, true, 5, 4, 0)]
        [InlineData(2, false, 16, 12, 0)]
        public async Task GetSummaryData_FilterReportsOnHealthRisks_ShouldReturnCorrectNumbers(int healthRiskId, bool isTraining, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
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
        }

        [Theory]
        [InlineData(1, false, 12, 6, 6)]
        [InlineData(2, false, 12, 6, 6)]
        [InlineData(1, true, 4, 2, 2)]
        [InlineData(2, true, 4, 2, 2)]
        public async Task GetSummaryData_FilterReportsOnRegion_ShouldReturnCorrectNumbers(int regionId, bool isTraining, int expectedReportsCount, int expectedActiveCollectorsCount, int expectedInactiveCollectorsCount)
        {
            //arrange
            var filters = new FiltersRequestDto
            {
                StartDate = new DateTime(2000, 01, 01),
                EndDate = new DateTime(2100, 01, 01),
                HealthRiskId = null,
                Area = new FiltersRequestDto.AreaDto { Id = 1, Type = FiltersRequestDto.AreaTypeDto.Region },
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
    }
}
