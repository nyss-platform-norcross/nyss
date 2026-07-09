using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;
using Microsoft.EntityFrameworkCore;

namespace RX.Nyss.Web.Services.ReportsDashboard;

public interface IReportsDashboardService
{
    Task<ReportHistogramResponseDto> GetKeptReportsInEscalatedAlertsHistogramData(ReportsFilter reportsFilter, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay, int? nationalSocietyId = null, int? projectId = null);

    IQueryable<Report> GetKeptReportsInEscalatedAlertsQuery(ReportsFilter reportsFilter, int? nationalSocietyId = null, int? projectId = null);

    Task<IList<HealthRiskReportsGroupedByDateDto>> GroupReportsByHealthRiskAndDate(IQueryable<Report> reports, int utcOffset);

    Task<IList<HealthRiskReportsGroupedByDateDto>> GroupReportsByHealthRiskAndEpiWeek(IQueryable<Report> reports, int utcOffset);
}

public class ReportsDashboardService : IReportsDashboardService
{
    private readonly INyssContext _nyssContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReportsDashboardService(
        INyssContext nyssContext,
        IDateTimeProvider dateTimeProvider
        )
    {
        _nyssContext = nyssContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ReportHistogramResponseDto> GetKeptReportsInEscalatedAlertsHistogramData(ReportsFilter reportsFilter, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay, int? nationalSocietyId = null, int? projectId = null)
    {
        var reportsQuery = GetKeptReportsInEscalatedAlertsQuery(reportsFilter, nationalSocietyId, projectId);

        var groupedReports = groupingType switch
        {
            DatesGroupingType.Day => await GroupReportsByHealthRiskAndDate(reportsQuery, reportsFilter.UtcOffset),
            DatesGroupingType.Week => await GroupReportsByHealthRiskAndEpiWeek(reportsQuery, reportsFilter.UtcOffset),
            _ =>
                throw new InvalidOperationException()
        };

        // We add utcOffset so we return the local dateStrings
        var chartStartDateString = reportsFilter.StartDate.AddHours(reportsFilter.UtcOffset).Date.ToString("yyyy-MM-dd");
        // End date is increased by 1 day in the reports filters (In order to actually fetch all reports created on the last day)
        // Therefore we must subtract 1 day before returning the endDate, or the chart will display an extra day
        var chartEndDateString = reportsFilter.EndDate.AddDays(-1).AddHours(reportsFilter.UtcOffset).Date.ToString("yyyy-MM-dd");

        var epiStartDate = _dateTimeProvider.GetEpiDate(reportsFilter.StartDate.AddHours(reportsFilter.UtcOffset).Date, epiWeekStartDay);
        var epiEndDate = _dateTimeProvider.GetEpiDate(reportsFilter.EndDate.AddDays(-1).AddHours(reportsFilter.UtcOffset).Date, epiWeekStartDay);
        var chartStartDateEpiWeekString = $"{epiStartDate.EpiYear}/{epiStartDate.EpiWeek}";
        var chartEndDateEpiWeekString = $"{epiEndDate.EpiYear}/{epiEndDate.EpiWeek}";


        return new ReportHistogramResponseDto
        {
            chartStartDateString = chartStartDateString,
            chartEndDateString = chartEndDateString,
            chartStartDateEpiWeekString = chartStartDateEpiWeekString,
            chartEndDateEpiWeekString = chartEndDateEpiWeekString,
            DateGroupingType = groupingType,
            groupedReports = groupedReports,
        };
    }

    public IQueryable<Report> GetKeptReportsInEscalatedAlertsQuery(ReportsFilter reportsFilter, int? nationalSocietyId = null, int? projectId = null)
    {
        var baseQuery = _nyssContext.Alerts
            .FilterByNationalSociety(nationalSocietyId);
        baseQuery = baseQuery.FilterByProject(projectId);
        baseQuery = baseQuery.Where(a => a.Status == AlertStatus.Escalated || a.Status == AlertStatus.Closed); // Only Include escalated or closed alerts
        var arQuery = baseQuery.SelectMany(a => a.AlertReports);

        // Apply reportsFilter to the baseQuery if it exists
        arQuery = arQuery.FilterByDate(reportsFilter.StartDate.UtcDateTime, reportsFilter.EndDate.UtcDateTime);
        arQuery = arQuery.FilterByHealthRisks(reportsFilter.HealthRisks);
        arQuery = arQuery.FilterByArea(reportsFilter.Area);

        // Apply additional filters to select only kept reports that are included in an escalated alert
        var reports = arQuery
            .FilterByReportStatus(new ReportStatusFilterDto
            {
                Kept = true,
                Dismissed = false,
                NotCrossChecked = false
            }) // Filter out non-kept reports
            .Select(ar => ar.Report);

        return reports;
    }

    public async Task<IList<HealthRiskReportsGroupedByDateDto>> GroupReportsByHealthRiskAndDate(IQueryable<Report> reports, int utcOffset)
    {
        // Reduce Reports into only the necessary data
        var reducedReports = reports
            .Select(r => new
            {
                ReportId = r.Id,
                HealthRiskId = r.ProjectHealthRisk.HealthRiskId,
                HealthRiskName = r.ProjectHealthRisk.HealthRisk.LanguageContents
                    .Where(hr => r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage != null &&
                                 hr.ContentLanguage.LanguageCode == r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode)
                    .Select(hr => hr.Name)
                    .FirstOrDefault() ?? "Unknown",
                Date = r.ReceivedAt.AddHours(utcOffset).Date, //Apply utc offset to Date
            });

        // Group Report based on healthRiskId and healthRiskName
        // Must be converted to a List as further groupBy operations causes errors with Linq
        var reportsGroupedByHealthRisk = reducedReports.GroupBy(r => new
        {
            HealthRiskId = r.HealthRiskId,
            HealthRiskName = r.HealthRiskName,
        })
            .Select(group => new
            {
                HealthRiskId = group.Key.HealthRiskId,
                HealthRiskName = group.Key.HealthRiskName,
                Data = group.ToList()
            }).ToList();

        // Further group the entries by Date and count entries in each dateGroup
        var reportsGroupedByHealthRiskAndDate = reportsGroupedByHealthRisk.Select(groupedReport => new
        {
            HealthRiskId = groupedReport.HealthRiskId,
            HealthRiskName = groupedReport.HealthRiskName,
            Data = groupedReport.Data.GroupBy(d => d.Date.Date.ToString("yyyy-MM-dd")).Select(dateGroup => new PeriodDto
            {
                Period = dateGroup.Key,
                Count = dateGroup.Count(),
            }).ToList()
        }).Select(groupedReport => new HealthRiskReportsGroupedByDateDto
        {
            HealthRiskId = groupedReport.HealthRiskId,
            HealthRiskName = groupedReport.HealthRiskName,
            Data = groupedReport.Data.ToList(),
        }).ToList();

        return reportsGroupedByHealthRiskAndDate;
    }

    public async Task<IList<HealthRiskReportsGroupedByDateDto>> GroupReportsByHealthRiskAndEpiWeek(IQueryable<Report> reports, int utcOffset)
    {
        // Execute query with all necessary data
        var query = reports
            .Select(r => new
            {
                ReportId = r.Id,
                HealthRiskId = r.ProjectHealthRisk.HealthRiskId,
                HealthRiskName = r.ProjectHealthRisk.HealthRisk.LanguageContents
                    .Where(hr => r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage != null &&
                                 hr.ContentLanguage.LanguageCode == r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode)
                    .Select(hr => hr.Name)
                    .FirstOrDefault() ?? "Unknown",
                Date = r.ReceivedAt.AddHours(utcOffset).Date,
                EpiWeekStartDay = r.RawReport.NationalSociety.EpiWeekStartDay,
                EpiWeek = r.EpiWeek,
                EpiYear = r.EpiYear
            });

        var reducedReports = await EntityFrameworkQueryableExtensions.ToListAsync(query);

        // Group by health risk and epi week in memory
        var reportsGroupedByHealthRiskAndEpiWeek = reducedReports
            .GroupBy(r => new
            {
                r.HealthRiskId,
                r.HealthRiskName
            })
            .Select(healthRiskGroup => new HealthRiskReportsGroupedByDateDto
            {
                HealthRiskId = healthRiskGroup.Key.HealthRiskId,
                HealthRiskName = healthRiskGroup.Key.HealthRiskName,
                Data = healthRiskGroup
                    .GroupBy(r => new
                    {
                        r.EpiWeek,
                        r.EpiYear
                    })
                    .Select(dateGroup => new PeriodDto
                    {
                        Period = $"{dateGroup.Key.EpiYear}/{dateGroup.Key.EpiWeek}",
                        Count = dateGroup.Count()
                    })
                    .OrderBy(p => int.Parse((p.Period ?? "0/0").Split('/')[0]))
                    .ThenBy(p => int.Parse((p.Period ?? "0/0").Split('/')[1]))
                    .ToList()
            })
            .ToList();

        return reportsGroupedByHealthRiskAndEpiWeek;
    }
}
