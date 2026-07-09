using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Reports;

public class ReportMessageServiceTests
{
    private readonly IReportMessageService _reportMessageService;

    public ReportMessageServiceTests()
    {
        var nyssContextMock = Substitute.For<INyssContext>();
        _reportMessageService = new ReportMessageService(nyssContextMock);

        var healthRisks = new List<HealthRisk>
        {
            new HealthRisk
            {
                Id = 1,
                HealthRiskCode = 99,
                HealthRiskType = HealthRiskType.Activity
            },
            new HealthRisk
            {
                Id = 2,
                HealthRiskCode = 10,
                HealthRiskType = HealthRiskType.Human
            },
            new HealthRisk
            {
                Id = 3,
                HealthRiskCode = 25,
                HealthRiskType = HealthRiskType.NonHuman
            },
            new HealthRisk
            {
                Id = 4,
                HealthRiskCode = 15,
                HealthRiskType = HealthRiskType.UnusualEvent
            }
        };
        var healthRisksDbSet = healthRisks.AsQueryable().BuildMockDbSet();
        nyssContextMock.HealthRisks.Returns(healthRisksDbSet);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ParseReport_WhenEmptyReportSent_ShouldThrowException(string reportMessage)
    {
        // Assert
        await Should.ThrowAsync<ReportValidationException>(async () => await _reportMessageService.ParseReport(reportMessage));
    }

    [Fact]
    public async Task ParseReport_WhenActivityCodeSent_ShouldParseCorrectly()
    {
        // Arrange
        var reportMessage = "99";

        // Act
        var parsedReport = await _reportMessageService.ParseReport(reportMessage);

        // Assert
        parsedReport.ReportType.ShouldBe(ReportType.Event);
        parsedReport.HealthRiskCode.ShouldBe(99);
        parsedReport.ReportedCase.CountMalesBelowFive.ShouldBeNull();
        parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBeNull();
        parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBeNull();
        parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBeNull();
        parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
    }

    [Fact]
    public async Task ParseReport_WhenNonHumanCodeSent_ShouldParseCorrectly()
    {
        // Arrange
        var reportMessage = "25";

        // Act
        var parsedReport = await _reportMessageService.ParseReport(reportMessage);

        // Assert
        parsedReport.ReportType.ShouldBe(ReportType.Event);
        parsedReport.HealthRiskCode.ShouldBe(25);
        parsedReport.ReportedCase.CountMalesBelowFive.ShouldBeNull();
        parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBeNull();
        parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBeNull();
        parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBeNull();
        parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
    }

    [Theory]
    [InlineData("10#1#1", 10, 1, 0, 0, 0, 0)]
    [InlineData("10#1#2", 10, 0, 1, 0, 0, 0)]
    [InlineData("10#2#1", 10, 0, 0, 1, 0, 0)]
    [InlineData("10#2#2", 10, 0, 0, 0, 1, 0)]
    [InlineData("10*1*1", 10, 1, 0, 0, 0, 0)]
    [InlineData("10*1*2", 10, 0, 1, 0, 0, 0)]
    [InlineData("10*2*1", 10, 0, 0, 1, 0, 0)]
    [InlineData("10*2*2", 10, 0, 0, 0, 1, 0)]
    [InlineData("  10*2*2 ", 10, 0, 0, 0, 1, 0)]
    [InlineData("10", 10, 0, 0, 0, 0, 1)]
    public async Task ParseReport_WhenSingleReportSent_ShouldParseCorrectly(string reportMessage, int healthRiskCode,
        int malesBelowFive, int malesAtLeastFive, int femalesBelowFive, int femalesAtLeastFive, int unspecifiedSexAndAge)
    {
        // Act
        var parsedReport = await _reportMessageService.ParseReport(reportMessage.Trim());

        // Assert
        parsedReport.ReportType.ShouldBe(ReportType.Single);
        parsedReport.HealthRiskCode.ShouldBe(healthRiskCode);
        parsedReport.ReportedCase.CountMalesBelowFive.ShouldBe(malesBelowFive);
        parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBe(malesAtLeastFive);
        parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBe(femalesBelowFive);
        parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBe(femalesAtLeastFive);
        parsedReport.ReportedCase.CountUnspecifiedSexAndAge.ShouldBe(unspecifiedSexAndAge);
        parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
    }

    [Theory]
    [InlineData("10#20#30#40#50", 10, 20, 30, 40, 50)]
    [InlineData("10*20*30*40*50", 10, 20, 30, 40, 50)]
    [InlineData("  10*20*30*40*50 ", 10, 20, 30, 40, 50)]
    public async Task ParseReport_WhenAggregatedReportSent_ShouldParseCorrectly(string reportMessage, int healthRiskCode,
        int malesBelowFive, int malesAtLeastFive, int femalesBelowFive, int femalesAtLeastFive)
    {
        // Act
        var parsedReport = await _reportMessageService.ParseReport(reportMessage.Trim());

        // Assert
        parsedReport.ReportType.ShouldBe(ReportType.Aggregate);
        parsedReport.HealthRiskCode.ShouldBe(healthRiskCode);
        parsedReport.ReportedCase.CountMalesBelowFive.ShouldBe(malesBelowFive);
        parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBe(malesAtLeastFive);
        parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBe(femalesBelowFive);
        parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBe(femalesAtLeastFive);
        parsedReport.DataCollectionPointCase.ReferredCount.ShouldBeNull();
    }

    [Theory]
    [InlineData("10#20#30#40#50#60", 10, 20, 30, 40, 50, 60)]
    [InlineData("10*20*30*40*50*60", 10, 20, 30, 40, 50, 60)]
    public async Task ParseReport_WhenDataCollectionPointReportSent_ShouldParseCorrectly(string reportMessage, int healthRiskCode,
        int malesBelowFive, int malesAtLeastFive, int femalesBelowFive, int femalesAtLeastFive, int referredCount)
    {
        // Act
        var parsedReport = await _reportMessageService.ParseReport(reportMessage.Trim());

        // Assert
        parsedReport.ReportType.ShouldBe(ReportType.DataCollectionPoint);
        parsedReport.HealthRiskCode.ShouldBe(healthRiskCode);
        parsedReport.ReportedCase.CountMalesBelowFive.ShouldBe(malesBelowFive);
        parsedReport.ReportedCase.CountMalesAtLeastFive.ShouldBe(malesAtLeastFive);
        parsedReport.ReportedCase.CountFemalesBelowFive.ShouldBe(femalesBelowFive);
        parsedReport.ReportedCase.CountFemalesAtLeastFive.ShouldBe(femalesAtLeastFive);
        parsedReport.DataCollectionPointCase.ReferredCount.ShouldBe(referredCount);
    }

    [Theory]
    [InlineData("4")]
    [InlineData("11#1#2")]
    [InlineData("11")]
    public void ParseReport_WhenGlobalHealthRiskDoesNotExist_ShouldThrowException(string reportMessage) =>
        Should.Throw<ReportValidationException>(async () => await _reportMessageService.ParseReport(reportMessage));

    [Theory]
    [InlineData("10", ReportType.Single)]
    [InlineData("10#1#2", ReportType.Single)]
    [InlineData("25", ReportType.Event)]
    [InlineData("99", ReportType.Event)]
    public async Task ParseReport_WhenParsingSingleReportPattern_ShouldReturnCorrectReportType(string reportMessage, ReportType reportType)
    {
        // Act
        var parsedReport = await _reportMessageService.ParseReport(reportMessage);

        // Assert
        parsedReport.ReportType.ShouldBe(reportType);
    }

    [Theory]
    [InlineData("15#1#1")]
    [InlineData("25#1#1")]
    public void ParseReport_WhenParsingEventOrNonHumanReportPatternWithSexAndAge_ShouldThrowException(string reportMessage)
    {
        Should.Throw<ReportValidationException>(async () => await _reportMessageService.ParseReport(reportMessage));
    }
}
