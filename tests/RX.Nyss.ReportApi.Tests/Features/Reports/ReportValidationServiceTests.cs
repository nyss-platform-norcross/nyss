using System;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Reports;

public class ReportValidationServiceTests
{
    private readonly IReportValidationService _reportValidationService;
    private readonly INyssContext _nyssContextMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly ILoggerAdapter _loggerAdapterMock;

    public ReportValidationServiceTests()
    {
        _nyssContextMock = Substitute.For<INyssContext>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _loggerAdapterMock = Substitute.For<ILoggerAdapter>();

        _reportValidationService = new ReportValidationService(_nyssContextMock, _dateTimeProviderMock, _loggerAdapterMock);
    }

    [Theory]
    [InlineData(ReportType.Single, DataCollectorType.Human, HealthRiskType.Human)]
    [InlineData(ReportType.Aggregate, DataCollectorType.Human, HealthRiskType.Human)]
    [InlineData(ReportType.Event, DataCollectorType.Human, HealthRiskType.Activity)]
    [InlineData(ReportType.Event, DataCollectorType.Human, HealthRiskType.NonHuman)]
    [InlineData(ReportType.Event, DataCollectorType.Human, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.Event, DataCollectorType.CollectionPoint, HealthRiskType.Activity)]
    [InlineData(ReportType.Event, DataCollectorType.CollectionPoint, HealthRiskType.NonHuman)]
    [InlineData(ReportType.Event, DataCollectorType.CollectionPoint, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.CollectionPoint, HealthRiskType.Human)]
    public void ValidateReport_WhenReportIsCorrect_ShouldNotThrowException(ReportType reportType, DataCollectorType dataCollectorType, HealthRiskType healthRiskType)
    {
        // Arrange
        var projectId = 1;
        var nationalSocietyId = 1;
        var healthRiskCode = 1;
        var projectHealthRisks = new List<ProjectHealthRisk>
        {
            new ProjectHealthRisk
            {
                Project = new Project { Id = projectId, NationalSocietyId = nationalSocietyId},
                HealthRisk = new HealthRisk
                {
                    HealthRiskType = healthRiskType,
                    HealthRiskCode = healthRiskCode
                }
            }
        };
        var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
        _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);

        var parsedReport = new ParsedReport
        {
            ReportType = reportType,
            HealthRiskCode = healthRiskCode
        };
        var dataCollector = new DataCollector
        {
            DataCollectorType = dataCollectorType,
            Project = new Project { Id = projectId }
        };

        // Assert
        Should.NotThrow(async () => await _reportValidationService.ValidateReport(parsedReport, dataCollector, nationalSocietyId));
    }

    [Theory]
    [InlineData(ReportType.Single, DataCollectorType.Human, HealthRiskType.Activity)]
    [InlineData(ReportType.Single, DataCollectorType.Human, HealthRiskType.NonHuman)]
    [InlineData(ReportType.Single, DataCollectorType.Human, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.Single, DataCollectorType.CollectionPoint, HealthRiskType.Human)]
    [InlineData(ReportType.Single, DataCollectorType.CollectionPoint, HealthRiskType.Activity)]
    [InlineData(ReportType.Single, DataCollectorType.CollectionPoint, HealthRiskType.NonHuman)]
    [InlineData(ReportType.Single, DataCollectorType.CollectionPoint, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.Aggregate, DataCollectorType.Human, HealthRiskType.Activity)]
    [InlineData(ReportType.Aggregate, DataCollectorType.Human, HealthRiskType.NonHuman)]
    [InlineData(ReportType.Aggregate, DataCollectorType.Human, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.Aggregate, DataCollectorType.CollectionPoint, HealthRiskType.Human)]
    [InlineData(ReportType.Aggregate, DataCollectorType.CollectionPoint, HealthRiskType.Activity)]
    [InlineData(ReportType.Aggregate, DataCollectorType.CollectionPoint, HealthRiskType.NonHuman)]
    [InlineData(ReportType.Aggregate, DataCollectorType.CollectionPoint, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.Event, DataCollectorType.Human, HealthRiskType.Human)]
    [InlineData(ReportType.Event, DataCollectorType.CollectionPoint, HealthRiskType.Human)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.Human, HealthRiskType.Human)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.Human, HealthRiskType.Activity)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.Human, HealthRiskType.NonHuman)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.Human, HealthRiskType.UnusualEvent)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.CollectionPoint, HealthRiskType.Activity)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.CollectionPoint, HealthRiskType.NonHuman)]
    [InlineData(ReportType.DataCollectionPoint, DataCollectorType.CollectionPoint, HealthRiskType.UnusualEvent)]
    public void ValidateReport_WhenReportIsNotCorrect_ShouldThrowException(ReportType reportType, DataCollectorType dataCollectorType, HealthRiskType healthRiskType)
    {
        // Arrange
        var projectId = 1;
        var nationalSocietyId = 1;
        var healthRiskCode = 1;
        var projectHealthRisks = new List<ProjectHealthRisk>
        {
            new ProjectHealthRisk
            {
                Project = new Project { Id = projectId, NationalSocietyId = nationalSocietyId},
                HealthRisk = new HealthRisk
                {
                    HealthRiskType = healthRiskType,
                    HealthRiskCode = healthRiskCode
                }
            }
        };
        var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
        _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);

        var parsedReport = new ParsedReport
        {
            ReportType = reportType,
            HealthRiskCode = healthRiskCode
        };
        var dataCollector = new DataCollector
        {
            DataCollectorType = dataCollectorType,
            Project = new Project { Id = projectId }
        };

        // Assert
        Should.Throw<ReportValidationException>(async () => await _reportValidationService.ValidateReport(parsedReport, dataCollector, nationalSocietyId));
    }

    [Fact]
    public void ValidateReport_WhenProjectHealthRiskDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var nationalSocietyId = 1;
        var projectHealthRisks = new List<ProjectHealthRisk>();
        var projectHealthRisksDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
        _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);

        var parsedReport = new ParsedReport { HealthRiskCode = 1 };
        var dataCollector = new DataCollector { Project = new Project { Id = 1, NationalSocietyId = nationalSocietyId } };

        // Assert
        Should.Throw<ReportValidationException>(async () => await _reportValidationService.ValidateReport(parsedReport, dataCollector, nationalSocietyId));
    }

    [Theory]
    [InlineData("20190101093456", "2019-01-01 09:34:56")]
    [InlineData("20200615112233", "2020-06-15 11:22:33")]
    [InlineData("20201125234510", "2020-11-25 23:45:10")]
    public void ParseTimestamp_WhenTimestampIsCorrect_ShouldNotThrowException(string timestamp, string parsedDateTimeString)
    {
        // Arrange
        DateTime parsedTimestamp = default;

        // Act
        Should.NotThrow(() => parsedTimestamp = _reportValidationService.ParseTimestamp(timestamp));

        // Assert
        parsedTimestamp.ShouldBe(DateTime.Parse(parsedDateTimeString));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("123456")]
    [InlineData("00000101000000")]
    [InlineData("20201301000000")]
    [InlineData("20200140000000")]
    [InlineData("20200101250000")]
    [InlineData("20200101007000")]
    [InlineData("20200101000070")]
    [InlineData("31012020000000")]
    public void ParseTimestamp_WhenTimestampCannotBeParsed_ShouldThrowException(string timestamp)
    {
        // Act
        Should.Throw<ReportValidationException>(() => _reportValidationService.ParseTimestamp(timestamp));
    }


    [Theory]
    [InlineData("20190101103456", "2019-01-01 09:36:56", "2019-01-01 09:34:56")]
    [InlineData("20200615122233", "2020-06-15 11:22:33", "2020-06-15 11:22:33")]
    [InlineData("20201126004510", "2020-11-25 23:45:10", "2020-11-25 23:45:10")]
    public void ParseTimestamp_WhenTimeIsOneHourFromTheFuture_ShouldAdjustItToNow(string incomingTimestamp, string utcNow, string outgoingTimestamp)
    {
        // Arrange
        _dateTimeProviderMock.UtcNow.Returns(DateTime.Parse(utcNow));

        // Act
        var parsedTimestamp = _reportValidationService.ParseTimestamp(incomingTimestamp);

        // Assert
        parsedTimestamp.ShouldBe(DateTime.Parse(outgoingTimestamp));
        _loggerAdapterMock.ReceivedWithAnyArgs().Warn("Timestamp is 1 hour into the future, likely due to wrong timezone settings, please check eagle!");
    }

    [Fact]
    public void ValidateReceivalTime_WhenTimeIsFromThePast_ShouldNotThrowException()
    {
        // Arrange
        var receivalTime = DateTime.Parse("2020-01-01 00:00:00");
        _dateTimeProviderMock.UtcNow.Returns(new DateTime(2020, 1, 1, 12, 0, 0));

        // Assert
        Should.NotThrow(() => _reportValidationService.ValidateReceivalTime(receivalTime));
    }

    [Fact]
    public void ValidateReceivalTime_WhenTimeIsWithinAllowedPrecedence_ShouldNotThrowException()
    {
        // Arrange
        var receivalTime = DateTime.Parse("2020-01-01 12:02:00");
        _dateTimeProviderMock.UtcNow.Returns(new DateTime(2020, 1, 1, 12, 0, 0));

        // Assert
        Should.NotThrow(() => _reportValidationService.ValidateReceivalTime(receivalTime));
    }

    [Fact]
    public void ValidateReceivalTime_WhenTimeIsFromTheFuture_ShouldThrowException()
    {
        // Arrange
        var receivalTime = DateTime.Parse("2020-01-01 12:05:00");
        _dateTimeProviderMock.UtcNow.Returns(new DateTime(2020, 1, 1, 12, 0, 0));

        // Assert
        Should.Throw<ReportValidationException>(() => _reportValidationService.ValidateReceivalTime(receivalTime));
    }

    [Fact]
    public void ValidateGatewaySetting_WhenApiKeyExists_ShouldNotThrowException()
    {
        // Arrange
        var apiKey = "api-key";
        var gatewaySettings = new List<GatewaySetting>
        {
            new GatewaySetting
            {
                ApiKey = apiKey,
                GatewayType = GatewayType.SmsGateway
            }
        };
        var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
        _nyssContextMock.GatewaySettings.Returns(gatewaySettingsDbSet);

        // Assert
        Should.NotThrow(() => _reportValidationService.ValidateGatewaySetting(apiKey));
    }

    [Fact]
    public void ValidateGatewaySetting_WhenGatewayTypeDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var apiKey = "api-key";
        var gatewaySettings = new List<GatewaySetting>();
        var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
        _nyssContextMock.GatewaySettings.Returns(gatewaySettingsDbSet);

        // Assert
        Should.Throw<ReportValidationException>(() => _reportValidationService.ValidateGatewaySetting(apiKey));
    }

    [Fact]
    public void ValidateGatewaySetting_WhenGatewayTypeIsNotSmsEagle_ShouldThrowException()
    {
        // Arrange
        var apiKey = "api-key";
        var gatewaySettings = new List<GatewaySetting>
        {
            new GatewaySetting
            {
                ApiKey = apiKey,
                GatewayType = GatewayType.Unknown
            }
        };
        var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
        _nyssContextMock.GatewaySettings.Returns(gatewaySettingsDbSet);

        // Assert
        Should.Throw<ReportValidationException>(() => _reportValidationService.ValidateGatewaySetting(apiKey));
    }
}
