using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NetTopologySuite.Geometries;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts;
using RX.Nyss.Web.Features.Reports.Commands;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Reports.Commands;

public class EditReportCommandTests
{
    [Fact]
    public async Task Handle_WhenCollectorHasMultipleLocationsAndLocationChanges_ShouldRecalculateAlerts()
    {
        var handler = CreateHandler(out var alertReportServiceMock, out var nyssContextMock);
        var scenario = ArrangeData(nyssContextMock);
        var command = CreateCommand(scenario.SecondLocation.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        scenario.RawReport.Zone!.Id.ShouldBe(scenario.SecondLocation.Zone!.Id);
        scenario.RawReport.Village!.Id.ShouldBe(scenario.SecondLocation.Village!.Id);
        scenario.RawReport.Report!.Location.ShouldNotBeNull();
        scenario.RawReport.Report.Location!.X.ShouldBe(scenario.SecondLocation.Location!.X);
        scenario.RawReport.Report.Location!.Y.ShouldBe(scenario.SecondLocation.Location!.Y);
        await alertReportServiceMock.Received(1).RecalculateAlertForReport(scenario.RawReport.Id);
    }

    [Fact]
    public async Task Handle_WhenCorrectionDoesNotChangeLocation_ShouldStillRecalculateForEligibleStatus()
    {
        var handler = CreateHandler(out var alertReportServiceMock, out var nyssContextMock);
        var scenario = ArrangeData(nyssContextMock);
        var command = CreateCommand(scenario.FirstLocation.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await alertReportServiceMock.Received(1).RecalculateAlertForReport(scenario.RawReport.Id);
    }

    [Fact]
    public async Task Handle_WhenSelectedLocationHasNoCoordinates_ShouldReturnErrorAndSkipRecalculation()
    {
        var handler = CreateHandler(out var alertReportServiceMock, out var nyssContextMock);
        var scenario = ArrangeData(nyssContextMock, withThirdLocationWithoutGeometry: true);
        var command = CreateCommand(scenario.ThirdLocationWithoutGeometry!.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        scenario.RawReport.Report!.Location.ShouldNotBeNull();
        await alertReportServiceMock.DidNotReceive().RecalculateAlertForReport(Arg.Any<int>());
    }

    private static EditReportCommand.Handler CreateHandler(
        out IAlertReportService alertReportServiceMock,
        out INyssContext nyssContextMock)
    {
        nyssContextMock = Substitute.For<INyssContext>();
        alertReportServiceMock = Substitute.For<IAlertReportService>();
        var authorizationServiceMock = Substitute.For<IAuthorizationService>();
        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        authorizationServiceMock.GetCurrentUserName().Returns("tester@nyss");
        dateTimeProviderMock.UtcNow.Returns(new DateTime(2026, 1, 10, 11, 0, 0));

        return new EditReportCommand.Handler(
            nyssContextMock,
            dateTimeProviderMock,
            authorizationServiceMock,
            alertReportServiceMock);
    }

    private static EditScenario ArrangeData(
        INyssContext nyssContextMock,
        bool withThirdLocationWithoutGeometry = false)
    {
        var project = new Project { Id = 1, Name = "testproject" };
        var projectHealthRisk = new ProjectHealthRisk
        {
            Id = 1,
            HealthRiskId = 1,
            HealthRisk = new HealthRisk { Id = 1 },
            Project = project
        };

        var district = new District { Id = 1, Name = "District" };
        var village = new Village { Id = 1, Name = "Village 1", District = district };
        var zone1 = new Zone { Id = 1, Name = "Zone 1", Village = village };
        var zone2 = new Zone { Id = 2, Name = "Zone 2", Village = village };

        var firstLocation = new DataCollectorLocation
        {
            Id = 1,
            DataCollectorId = 1,
            Village = village,
            Zone = zone1,
            Location = new Point(1, 1)
        };
        var secondLocation = new DataCollectorLocation
        {
            Id = 2,
            DataCollectorId = 1,
            Village = village,
            Zone = zone2,
            Location = new Point(2, 2)
        };

        var rawReport = new RawReport
        {
            Id = 1,
            Sender = "+47234234234",
            Village = village,
            Zone = zone1,
            ReceivedAt = new DateTime(2026, 1, 10, 10, 15, 0)
        };
        var report = new Report
        {
            Id = 1,
            Status = ReportStatus.New,
            ReportType = ReportType.Single,
            PhoneNumber = "+47234234234",
            ProjectHealthRisk = projectHealthRisk,
            RawReport = rawReport,
            ReportedCase = new ReportCase(),
            Location = new Point(1, 1),
            ReceivedAt = rawReport.ReceivedAt
        };
        rawReport.Report = report;

        var dataCollectorLocations = new List<DataCollectorLocation> { firstLocation, secondLocation };
        DataCollectorLocation thirdLocationWithoutGeometry = null;
        if (withThirdLocationWithoutGeometry)
        {
            thirdLocationWithoutGeometry = new DataCollectorLocation
            {
                Id = 3,
                DataCollectorId = 1,
                Village = village
            };
            dataCollectorLocations.Add(thirdLocationWithoutGeometry);
        }

        var dataCollector = new DataCollector
        {
            Id = 1,
            Name = "Data Collector",
            PhoneNumber = "+143123423525",
            DataCollectorLocations = dataCollectorLocations,
            Project = project
        };

        rawReport.DataCollector = dataCollector;
        report.DataCollector = dataCollector;

        var projectsDbSet = new List<Project> { project }.AsQueryable().BuildMockDbSet();
        var projectHealthRisksDbSet = new List<ProjectHealthRisk> { projectHealthRisk }.AsQueryable().BuildMockDbSet();
        var rawReportsDbSet = new List<RawReport> { rawReport }.AsQueryable().BuildMockDbSet();
        var reportsDbSet = new List<Report> { report }.AsQueryable().BuildMockDbSet();
        var dataCollectorsDbSet = new List<DataCollector> { dataCollector }.AsQueryable().BuildMockDbSet();
        var dataCollectorLocationsDbSet = dataCollectorLocations.AsQueryable().BuildMockDbSet();

        nyssContextMock.Projects.Returns(projectsDbSet);
        nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksDbSet);
        nyssContextMock.RawReports.Returns(rawReportsDbSet);
        nyssContextMock.Reports.Returns(reportsDbSet);
        nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);
        nyssContextMock.DataCollectorLocations.Returns(dataCollectorLocationsDbSet);

        return new EditScenario(rawReport, firstLocation, secondLocation, thirdLocationWithoutGeometry);
    }

    private static EditReportCommand CreateCommand(int dataCollectorLocationId) =>
        new(
            1,
            new ReportRequestDto
            {
                Date = new DateTime(2026, 1, 10),
                DataCollectorId = 1,
                DataCollectorLocationId = dataCollectorLocationId,
                ReportStatus = ReportStatus.New,
                HealthRiskId = 1,
                CountMalesBelowFive = 1,
                CountMalesAtLeastFive = 0,
                CountFemalesBelowFive = 0,
                CountFemalesAtLeastFive = 0,
                CountUnspecifiedSexAndAge = 0
            });

    private record EditScenario(
        RawReport RawReport,
        DataCollectorLocation FirstLocation,
        DataCollectorLocation SecondLocation,
        DataCollectorLocation? ThirdLocationWithoutGeometry);
}
