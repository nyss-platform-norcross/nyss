using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Reports
{
    public class ReportMessageServiceTests
    {
        private readonly IReportMessageService _reportMessageService;

        public ReportMessageServiceTests()
        {
            var nyssContextMock = Substitute.For<INyssContext>();
            _reportMessageService = new ReportMessageService(nyssContextMock);

            var healthRisks = new List<HealthRisk> { new HealthRisk { Id = 1, HealthRiskCode = 99, HealthRiskType = HealthRiskType.Activity } };
            var healthRisksDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            nyssContextMock.HealthRisks.Returns(healthRisksDbSet);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ParseReport_WhenEmptyReportSent_ShouldThrowException(string reportMessage)
        {
            // Assert
            Should.Throw<ReportValidationException>(() => _reportMessageService.ParseReport(reportMessage));
        }

        [Fact]
        public void ParseReport_WhenActivityCodeSent_ShouldParseCorrectly()
        {
            // Arrange
            var reportMessage = "99";

            // Act
            var parsedReport = _reportMessageService.ParseReport(reportMessage);

            // Assert
            parsedReport.ReportType.ShouldBe(ReportType.OneInteger);
            parsedReport.HealthRiskCode.ShouldBe(99);
            parsedReport.ReportedCase.CountMalesBelowFive.ShouldBeNull();
            parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBeNull();
            parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBeNull();
            parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBeNull();
            parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.DeathCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.FromOtherVillagesCount.ShouldBeNull();
        }

        [Fact]
        public void ParseReport_WhenNonHumanCodeSent_ShouldParseCorrectly()
        {
            // Arrange
            var reportMessage = "25";

            // Act
            var parsedReport = _reportMessageService.ParseReport(reportMessage);

            // Assert
            parsedReport.ReportType.ShouldBe(ReportType.OneInteger);
            parsedReport.HealthRiskCode.ShouldBe(25);
            parsedReport.ReportedCase.CountMalesBelowFive.ShouldBeNull();
            parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBeNull();
            parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBeNull();
            parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBeNull();
            parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.DeathCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.FromOtherVillagesCount.ShouldBeNull();
        }

        [Theory]
        [InlineData("10#1#1", 10, 1, 0, 0, 0)]
        [InlineData("10#1#2", 10, 0, 1, 0, 0)]
        [InlineData("10#2#1", 10, 0, 0, 1, 0)]
        [InlineData("10#2#2", 10, 0, 0, 0, 1)]
        [InlineData("10*1*1", 10, 1, 0, 0, 0)]
        [InlineData("10*1*2", 10, 0, 1, 0, 0)]
        [InlineData("10*2*1", 10, 0, 0, 1, 0)]
        [InlineData("10*2*2", 10, 0, 0, 0, 1)]
        public void ParseReport_WhenSingleReportSent_ShouldParseCorrectly(string reportMessage, int healthRiskCode,
            int malesBelowFive, int malesAtLeastFive, int femalesBelowFive, int femalesAtLeastFive)
        {
            // Act
            var parsedReport = _reportMessageService.ParseReport(reportMessage);

            // Assert
            parsedReport.ReportType.ShouldBe(ReportType.Single);
            parsedReport.HealthRiskCode.ShouldBe(healthRiskCode);
            parsedReport.ReportedCase.CountMalesBelowFive.ShouldBe(malesBelowFive);
            parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBe(malesAtLeastFive);
            parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBe(femalesBelowFive);
            parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBe(femalesAtLeastFive);;
            parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.DeathCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.FromOtherVillagesCount.ShouldBeNull();
        }

        [Theory]
        [InlineData("10#20#30#40#50", 10, 20, 30, 40, 50)]
        [InlineData("10*20*30*40*50", 10, 20, 30, 40, 50)]
        public void ParseReport_WhenAggregatedReportSent_ShouldParseCorrectly(string reportMessage, int healthRiskCode,
            int malesBelowFive, int malesAtLeastFive, int femalesBelowFive, int femalesAtLeastFive)
        {
            // Act
            var parsedReport = _reportMessageService.ParseReport(reportMessage);

            // Assert
            parsedReport.ReportType.ShouldBe(ReportType.Aggregate);
            parsedReport.HealthRiskCode.ShouldBe(healthRiskCode);
            parsedReport.ReportedCase.CountMalesBelowFive.ShouldBe(malesBelowFive);
            parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBe(malesAtLeastFive);
            parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBe(femalesBelowFive);
            parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBe(femalesAtLeastFive);;
            parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.DeathCount.ShouldBeNull();
            parsedReport.DataCollectionPointCase.FromOtherVillagesCount.ShouldBeNull();
        }

        [Theory]
        [InlineData("10#20#30#40#50#60#70#80", 10, 20, 30, 40, 50, 60, 70, 80)]
        [InlineData("10*20*30*40*50*60*70*80", 10, 20, 30, 40, 50, 60, 70, 80)]
        public void ParseReport_WhenDataCollectionPointReportSent_ShouldParseCorrectly(string reportMessage, int healthRiskCode,
            int malesBelowFive, int malesAtLeastFive, int femalesBelowFive, int femalesAtLeastFive, int referredCount, int deathCount, int fromOtherVillagesCount)
        {
            // Act
            var parsedReport = _reportMessageService.ParseReport(reportMessage);

            // Assert
            parsedReport.ReportType.ShouldBe(ReportType.DataCollectionPoint);
            parsedReport.HealthRiskCode.ShouldBe(healthRiskCode);
            parsedReport.ReportedCase.CountMalesBelowFive.ShouldBe(malesBelowFive);
            parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBe(malesAtLeastFive);
            parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBe(femalesBelowFive);
            parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBe(femalesAtLeastFive);;
            parsedReport.DataCollectionPointCase.ReferredCount.ShouldBe(referredCount);
            parsedReport.DataCollectionPointCase.DeathCount.ShouldBe(deathCount);
            parsedReport.DataCollectionPointCase.FromOtherVillagesCount.ShouldBe(fromOtherVillagesCount);
        }
    }
}
