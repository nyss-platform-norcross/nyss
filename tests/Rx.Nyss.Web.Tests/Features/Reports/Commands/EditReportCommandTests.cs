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
    private readonly INyssContext _nyssContextMock;
    private readonly IAuthorizationService _authorizationServiceMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly IAlertReportService _alertReportServiceMock;
    private readonly EditReportCommand.Handler _editReportCommandHandler;

    public EditReportCommandTests()
    {
        _nyssContextMock = Substitute.For<INyssContext>();
        _authorizationServiceMock = Substitute.For<IAuthorizationService>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _alertReportServiceMock = Substitute.For<IAlertReportService>();

        _editReportCommandHandler = new EditReportCommand.Handler(
            _nyssContextMock,
            _dateTimeProviderMock,
            _authorizationServiceMock,
            _alertReportServiceMock);
    }

    [Fact]
    public async Task EditReport_WhenUnknownSender_ShouldSucceed()
    {
        // Arrange
        ArrangeData();

        var reportRequestDto = new ReportRequestDto
        {
            HealthRiskId = 1,
            Date = new DateTime(2020, 1, 1),
            CountFemalesBelowFive = 1,
            CountFemalesAtLeastFive = 0,
            CountMalesBelowFive = 0,
            CountMalesAtLeastFive = 0,
            CountUnspecifiedSexAndAge = 0,
            DataCollectorId = 1,
            ReportStatus = ReportStatus.Accepted,
            DataCollectorLocationId = 1
        };

        var editReportCommand = new EditReportCommand(1, reportRequestDto);

        // Act
        var result = await _editReportCommandHandler.Handle(editReportCommand, CancellationToken.None);
        var reportAfterEdit = _nyssContextMock.Reports
            .First(rr => rr.Id == 1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        reportAfterEdit.DataCollector.Id.ShouldBe(1);
        reportAfterEdit.ReportedCase.CountFemalesBelowFive.ShouldBe(1);
        reportAfterEdit.ReportedCase.CountMalesAtLeastFive.ShouldBe(0);
        reportAfterEdit.ReportedCase.CountFemalesAtLeastFive.ShouldBe(0);
        reportAfterEdit.ReportedCase.CountMalesAtLeastFive.ShouldBe(0);
        reportAfterEdit.ReportedCase.CountUnspecifiedSexAndAge.ShouldBe(0);
        reportAfterEdit.Status.ShouldBe(ReportStatus.Accepted);
        reportAfterEdit.Location.X.ShouldBe(1);
        reportAfterEdit.Location.Y.ShouldBe(1);
        reportAfterEdit.RawReport.Village.Id.ShouldBe(1);
    }

    private void ArrangeData()
    {
        var project = new Project
        {
            Id = 1,
            Name = "testproject"
        };

        var projectHealthRisk = new ProjectHealthRisk
        {
            Id = 1,
            HealthRisk = new HealthRisk { Id = 1 }
        };

        var rawReport = new RawReport()
        {
            Id = 1,
            Sender = "+47234234234"
        };

        var report = new Report
        {
            Id = 1,
            Status = ReportStatus.New,
            PhoneNumber = "+47234234234",
            ProjectHealthRisk = projectHealthRisk,
            RawReport = rawReport,
            ReportedCase = new ReportCase()
        };

        rawReport.Report = report;

        var dataCollectorLocation = new DataCollectorLocation
        {
            Id = 1,
            Location = new Point(1, 1),
            Village = new Village
            {
                Id = 1,
                Name = "village"
            },
            DataCollectorId = 1
        };


        var dataCollector = new DataCollector
        {
            Id = 1,
            Name = "Data Collector",
            PhoneNumber = "+143123423525",
            DataCollectorLocations = new List<DataCollectorLocation> { dataCollectorLocation },
            Project = project
        };



        var projects = new List<Project> { project };
        var projectHealthRisks = new List<ProjectHealthRisk> { projectHealthRisk };
        var rawReports = new List<RawReport> { rawReport };
        var reports = new List<Report> { rawReport.Report };
        var dataCollectors = new List<DataCollector> { dataCollector };
        var dataCollectorLocations = new List<DataCollectorLocation> { dataCollectorLocation };

        var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
        var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
        var rawReportsMockDbSet = rawReports.AsQueryable().BuildMockDbSet();
        var reportsMockDbSet = reports.AsQueryable().BuildMockDbSet();
        var dataCollectorsMockDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
        var dataCollectorLocationsMockDbSet = dataCollectorLocations.AsQueryable().BuildMockDbSet();

        _nyssContextMock.Projects.Returns(projectsMockDbSet);
        _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);
        _nyssContextMock.RawReports.Returns(rawReportsMockDbSet);
        _nyssContextMock.Reports.Returns(reportsMockDbSet);
        _nyssContextMock.DataCollectors.Returns(dataCollectorsMockDbSet);
        _nyssContextMock.DataCollectorLocations.Returns(dataCollectorLocationsMockDbSet);
    }
}
