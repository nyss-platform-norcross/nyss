using System;
using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Services.ReportsDashboard.ReportsDashboardServiceTests;

public class ReportsDashboardServiceTests : ReportsDashboardServiceTestBase
{
    private readonly IReportsDashboardService _reportsDashboardService;

    public ReportsDashboardServiceTests()
    {
        IDateTimeProvider dateTimeProvider = new DateTimeProvider();
        _reportsDashboardService = new ReportsDashboardService(_nyssContext, dateTimeProvider);
    }

    private ReportsFilter GetReportFilters() =>
        new ReportsFilter // Filter from 01.01.2024 - 08.01.2024
        {
            StartDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
            EndDate = new DateTimeOffset(2024, 1, 8, 0, 0, 0, new TimeSpan(1, 0, 0)).AddDays(1),
            // Reports Filters EndDate gets added 1 day so we include all the reports that have come in on the last day
            // This should make it so that all reports registered by 08-01-2024 23:59:59 are included, but after 09-01-2024 0:0:0 are excluded
        };

    [Fact]
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldNotReturnReportsFromWrongNationalSocieties()
    {
        // Arrange test data
        ArrangeTwoKeptReportsInEscalatedAlertsInTwoDifferentNationalSocieties();
        var nationalSocietyIdCorrect = 1;
        var nationalSocietyIdWrong = 2;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyIdCorrect);

        // assert
        reportQuery.Any(r => r.ProjectHealthRisk.Project.NationalSociety.Id == nationalSocietyIdCorrect).ShouldBe(true);
        reportQuery.Any(r => r.ProjectHealthRisk.Project.NationalSociety.Id == nationalSocietyIdWrong).ShouldBe(false);
    }

    [Fact]
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldReturnReportsFromAllProjectsInNationalSocietyIfProjectIdNotSpecified()
    {
        // Arrange test data
        ArrangeTwoKeptReportsInEscalatedAlertsInTwoDifferentProjectsInTheSameNationalSociety();
        var nationalSocietyId = 1;
        var projectIdFirst = 1;
        var projectIdSecond = 2;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId);

        // assert
        reportQuery.Any(r => r.ProjectHealthRisk.Project.Id == projectIdFirst).ShouldBe(true);
        reportQuery.Any(r => r.ProjectHealthRisk.Project.Id == projectIdSecond).ShouldBe(true);
    }

    [Fact]
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldNotReturnReportsFromWrongProjectsIfProjectIdSpecified()
    {
        // Arrange test data
        ArrangeTwoKeptReportsInEscalatedAlertsInTwoDifferentProjectsInTheSameNationalSociety();
        var nationalSocietyId = 1;
        var projectIdCorrect = 1;
        var projectIdWrong = 2;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId, projectId: projectIdCorrect);

        // assert
        reportQuery.Any(r => r.ProjectHealthRisk.Project.Id == projectIdCorrect).ShouldBe(true);
        reportQuery.Any(r => r.ProjectHealthRisk.Project.Id == projectIdWrong).ShouldBe(false);
    }

    [Fact]
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldOnlyReturnKeptReports()
    {
        // Arrange test data
        ArrangeKeptDismissedAndNotCrossCheckedReportsInAnEscalatedAlert();
        var nationalSocietyId = 1;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId);

        // assert
        reportQuery.Any(r => r.Status == ReportStatus.Rejected).ShouldBe(false);
        reportQuery.Any(r => r.Status == ReportStatus.Pending).ShouldBe(false);
        reportQuery.All(r => r.Status == ReportStatus.Accepted).ShouldBe(true);
    }

    [Fact]
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldOnlyReturnReportsFromAnEscalatedOrClosedAlert()
    {
        // Arrange test data
        ArrangeKeptReportsInOpenClosedDismissedAndEscalatedAlerts();
        var nationalSocietyId = 1;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId);

        // assert
        reportQuery.Any(r => r.ReportAlerts.Any(ar => ar.Alert.Status == AlertStatus.Open)).ShouldBe(false);
        reportQuery.Any(r => r.ReportAlerts.Any(ar => ar.Alert.Status == AlertStatus.Dismissed)).ShouldBe(false);
        reportQuery.All(r => r.ReportAlerts.All(ar => ar.Alert.Status == AlertStatus.Escalated || ar.Alert.Status == AlertStatus.Closed)).ShouldBe(true);

    }

    [Fact]
    // Tests that all reports returned by GetKeptReportsInEscalatedAlertsQuery are within the filter time-range
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldReturnAllKeptReportsInEscalatedAlertsWithinTheFilterTimeRange()
    {
        // Arrange test data
        ArrangeKeptReportsInEscalatedAlertsInsideAndOutsideOfTimeRange();
        // Reports are inside and just outside of the time range 01.01.2024 - 08.01.2024
        var nationalSocietyId = 1;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId);

        // assert
        reportQuery.Count().ShouldBe(3); // We expect 3 reports given the test data
        reportQuery.All(r => r.ReceivedAt >= reportsFilter.StartDate.UtcDateTime && r.ReceivedAt < reportsFilter.EndDate.UtcDateTime).ShouldBe(true);
    }

    [Fact]
    // Tests that GetKeptReportsInEscalatedAlertsQuery only returns reports within the filter time-range, even if all the reports are
    // Included in the same escalated alert
    public void GetKeptReportsInEscalatedAlertsQuery_ShouldOnlyReturnKeptReportsInTheSameEscalatedAlertWithinTheFilterTimeRange()
    {
        // Arrange test data
        ArrangeKeptReportsInTheSameEscalatedAlertInsideAndOutsideOfTimeRange();
        // Reports are inside and just outside of the time range 01.01.2024 - 08.01.2024
        var nationalSocietyId = 1;

        var reportsFilter = GetReportFilters();

        // act
        var reportQuery = _reportsDashboardService.GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId);

        // assert
        reportQuery.Count().ShouldBe(3); // We expect 3 reports given the test data
        reportQuery.All(r => r.ReceivedAt >= reportsFilter.StartDate.UtcDateTime && r.ReceivedAt < reportsFilter.EndDate.UtcDateTime).ShouldBe(true);
    }

    // Comparison function between two lists of HealthRiskReportsGroupedByDateDtos
    private bool GroupedReportsListEquals(IEnumerable<HealthRiskReportsGroupedByDateDto> first, IEnumerable<HealthRiskReportsGroupedByDateDto> second) =>
        first.All(group => second.Any(group.ValueEquals));

    [Fact]
    public async void GroupReportsByHealthRiskAndDate_ShouldGroupReportQueriesToTheCorrectFormat()
    {
        var projectHealthRisks = _nyssContext.ProjectHealthRisks.ToList();
        var reportQuery = new List<Report>()
        {
            new Report
            {
                Id = 1,
                ReceivedAt = new DateTime(2023, 12, 31, 23, 0, 0),
                ProjectHealthRisk = projectHealthRisks[0] // Health risk 1 in project 1 in national society 1
            },
            new Report
            {
                Id = 2,
                ReceivedAt = new DateTime(2024, 1, 2, 0, 0, 0),
                ProjectHealthRisk = projectHealthRisks[0]
            },
            new Report
            {
                Id = 3,
                ReceivedAt = new DateTime(2024, 1, 2, 23, 0, 0),
                ProjectHealthRisk = projectHealthRisks[0]
            },
            new Report
            {
                Id = 4,
                ReceivedAt = new DateTime(2024, 1, 3, 22, 59, 59),
                ProjectHealthRisk = projectHealthRisks[0]
            },
            new Report
            {
                Id = 5,
                ReceivedAt = new DateTime(2024, 1, 1, 0, 0, 0),
                ProjectHealthRisk = projectHealthRisks[1] // HealthRisk 2 in project 1 in national society 1
            },
            new Report
            {
                Id = 6,
                ReceivedAt = new DateTime(2024, 1, 3, 0, 0, 0),
                ProjectHealthRisk = projectHealthRisks[1]
            },
        }.AsQueryable();

        // Considering the reportQuery input, and a utc offset of 1, this is the expected output of the grouping function
        var expectedReturnValue = new List<HealthRiskReportsGroupedByDateDto>
        {
            new HealthRiskReportsGroupedByDateDto
            {
                HealthRiskId = 1,
                HealthRiskName = "Fever",
                Data = new List<PeriodDto>
                {
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2024-01-01"
                    },
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2024-01-02"
                    },
                    new PeriodDto
                    {
                        Count = 2,
                        Period = "2024-01-03"
                    }
                }
            },
            new HealthRiskReportsGroupedByDateDto
            {
                HealthRiskId = 2,
                HealthRiskName = "Acute Watery Diarrhea (AWD)",
                Data = new List<PeriodDto>
                {
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2024-01-01"
                    },
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2024-01-03"
                    }
                }
            }
        };

        // Reports are inside and just outside of the time range 01.01.2024 - 08.01.2024
        //var nationalSocietyId = 1;

        // act
        var groupedReports = await _reportsDashboardService.GroupReportsByHealthRiskAndDate(reportQuery, 1);

        // assert
        GroupedReportsListEquals(groupedReports, expectedReturnValue).ShouldBe(true);
    }

    /*[Fact]
    public async void GroupReportsByHealthRiskAndEpiWeek_ShouldGroupReportQueriesToTheCorrectFormat()
    {
        var projectHealthRisks = _nyssContext.ProjectHealthRisks.ToList();
        // Dummy raw report with link to national society (reference is used in groupByEpiWeek)
        var dummyRawReport = new RawReport
        {
            NationalSociety = projectHealthRisks[0].Project.NationalSociety
        };

        var reportQuery = new List<Report>()
        {
            new Report
            {
                Id = 1,
                ReceivedAt = new DateTime(2023, 12, 30, 0, 0, 0),
                ProjectHealthRisk = projectHealthRisks[0], // Health risk 1 in project 1 in national society 1
                RawReport = dummyRawReport
            },
            new Report
            {
                Id = 2,
                ReceivedAt = new DateTime(2023, 12, 31, 23, 0, 0),
                ProjectHealthRisk = projectHealthRisks[0],
                RawReport = dummyRawReport
            },
            new Report
            {
                Id = 3,
                ReceivedAt = new DateTime(2024, 1, 6, 23, 0, 0).AddSeconds(-1),
                ProjectHealthRisk = projectHealthRisks[0],
                RawReport = dummyRawReport
            },
            new Report
            {
                Id = 4,
                ReceivedAt = new DateTime(2024, 1, 6, 23, 0, 0),
                ProjectHealthRisk = projectHealthRisks[0],
                RawReport = dummyRawReport
            },
            new Report
            {
                Id = 5,
                ReceivedAt = new DateTime(2024, 12, 28, 23, 0, 0).AddSeconds(-1),
                ProjectHealthRisk = projectHealthRisks[1], // HealthRisk 2 in project 1 in national society 1
                RawReport = dummyRawReport
            },
            new Report
            {
                Id = 6,
                ReceivedAt = new DateTime(2024, 12, 28, 23, 0, 0),
                ProjectHealthRisk = projectHealthRisks[1],
                RawReport = dummyRawReport
            },
        }.AsQueryable();

        // Considering the reportQuery input, and a utc offset of 1, this is the expected output of the grouping function
        var expectedReturnValue = new List<HealthRiskReportsGroupedByDateDto>
        {
            new HealthRiskReportsGroupedByDateDto
            {
                HealthRiskId = 1,
                HealthRiskName = "Fever",
                Data = new List<PeriodDto>
                {
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2023/52"
                    },
                    new PeriodDto
                    {
                        Count = 2,
                        Period = "2024/1"
                    },
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2024/2"
                    }
                }
            },
            new HealthRiskReportsGroupedByDateDto
            {
                HealthRiskId = 2,
                HealthRiskName = "Acute Watery Diarrhea (AWD)",
                Data = new List<PeriodDto>
                {
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2024/52"
                    },
                    new PeriodDto
                    {
                        Count = 1,
                        Period = "2025/1"
                    }
                }
            }
        };

        // Reports are inside and just outside of the time range 01.01.2024 - 08.01.2024
        //var nationalSocietyId = 1;

        // act
        var groupedReports = await _reportsDashboardService.GroupReportsByHealthRiskAndEpiWeek(reportQuery, 1);

        // assert
        GroupedReportsListEquals(groupedReports, expectedReturnValue).ShouldBe(true);
    }

*/}
