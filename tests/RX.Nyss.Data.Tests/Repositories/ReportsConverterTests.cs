using NSubstitute;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Repositories;
using Xunit;

namespace RX.Nyss.Data.Tests.Repositories;

public class ReportsConverterTests
{
    private readonly IReportsConverter _reportConverter;

    public ReportsConverterTests()
    {
        _reportConverter = new ReportsConverter(Substitute.For<ILoggerAdapter>());
    }

    [Fact]
    public async Task CreateRawReport_ValidRawReports_ShouldConvert()
    {
        // arrange
        var rawReport = ReportsData.CreateRawReport();
        rawReport
            .AddLocation("Whiterun", "Skyrim", "ABC", "Tamriel")
            .AddReport("123", new DateTime(2022, 11, 24), true)
            .AddHealthRisk(true)
            .AddSuspectedDiseases(1, new List<string> { "stomachache", "headache" });

        var rawReport2 = ReportsData.CreateRawReport();
        rawReport2
            .AddLocation("Soltitude", "Skyrim", "ABC", "Tamriel")
            .AddReport("123", new DateTime(2022, 11, 24), true)
            .AddHealthRisk(true)
            .AddSuspectedDiseases(1, new List<string> { "stomachache", "headache" });

        var rawReport3 = ReportsData.CreateRawReport();
        rawReport3
            .AddLocation("Soltitude", "Skyrim", "ABC", "Tamriel")
            .AddReport(null, new DateTime(2022, 11, 24), false)
            .AddHealthRisk(true)
            .AddSuspectedDiseases(1, new List<string> { "stomachache", "headache" });

        var rawReport4 = ReportsData.CreateRawReport();
        rawReport4
            .AddLocation(null, "Cyrodil", "XYZ", "Tamriel")
            .AddReport(null, new DateTime(2022, 11, 25), null);

        // act
        var result = _reportConverter.ConvertReports(
            new List<RawReport> { rawReport, rawReport2, rawReport3, rawReport4 },
            new DateTime(2022, 11, 20),
 1);

        // assert
        Assert.Equal(2, result.Count);

        Assert.Equal("female (2), male", result[0].Gender);
        Assert.Equal("Tamriel/Skyrim/Soltitude (2), Tamriel/Skyrim/Whiterun", result[0].Location);
        Assert.Equal("2022-11-20", result[0].EventDate);
        Assert.Equal("Human (3)", result[0].EventType);
        Assert.Equal("ABC", result[0].OrgUnit);
        Assert.Equal("123 (2)", result[0].PhoneNumber);
        Assert.Equal("stomachache/headache (3)", result[0].SuspectedDisease);
        Assert.Equal("2022-11-24 00:00:00Z (3)", result[0].DateOfOnset);

        Assert.Equal("", result[1].Gender);
        Assert.Equal("Tamriel/Cyrodil/", result[1].Location);
        Assert.Equal("2022-11-20", result[1].EventDate);
        Assert.Equal("", result[1].EventType);
        Assert.Equal("XYZ", result[1].OrgUnit);
        Assert.Equal("", result[1].PhoneNumber);
        Assert.Equal("", result[1].SuspectedDisease);
        Assert.Equal("2022-11-25 00:00:00Z", result[1].DateOfOnset);
    }

    [Fact]
    public async Task CreateRawReport_InValidRawReports_ShouldConvertPartially()
    {
        // arrange
        var rawReport = ReportsData.CreateRawReport();
        rawReport
            .AddLocation("Kvatch", "Cyrodil", null, "Tamriel");

        var rawReport2 = ReportsData.CreateRawReport();
        rawReport2
            .AddLocation("Whiterun", "Skyrim", "ABC", "Tamriel");

        var rawReport3 = ReportsData.CreateRawReport();

        var rawReport4 = ReportsData.CreateRawReport();

        // act
        var result = _reportConverter.ConvertReports(
            new List<RawReport> { rawReport, rawReport2, rawReport3, rawReport4 },
            new DateTime(2022, 11, 20),
 1);

        // assert
        Assert.Single(result);

        Assert.Equal("", result[0].Gender);
        Assert.Equal("Tamriel/Skyrim/Whiterun", result[0].Location);
        Assert.Equal("2022-11-20", result[0].EventDate);
        Assert.Equal("ABC", result[0].OrgUnit);
    }
}
