using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Extensions;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Services.ReportsDashboard
{
    public interface IReportsDashboardByHealthRiskService
    {
        Task<ReportByHealthRiskAndDateResponseDto> GetReportsGroupedByHealthRiskAndDate(ReportsFilter filters, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay);
    }

    public class ReportsDashboardByHealthRiskService : IReportsDashboardByHealthRiskService
    {
        private readonly IReportService _reportService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;


        public ReportsDashboardByHealthRiskService(
            IReportService reportService,
            IDateTimeProvider dateTimeProvider,
            INyssWebConfig config,
            INyssContext nyssContext)
        {
            _reportService = reportService;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
            _nyssContext = nyssContext;
        }

        public async Task<ReportByHealthRiskAndDateResponseDto> GetReportsGroupedByHealthRiskAndDate(ReportsFilter filters, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay)
        {
            var reports = _reportService.GetDashboardHealthRiskEventReportsQuery(filters);

            return groupingType switch
            {
                DatesGroupingType.Day =>
                await GroupReportsByHealthRiskAndDay(reports, filters.StartDate.DateTime.AddHours(filters.UtcOffset), filters.EndDate.DateTime.AddHours(filters.UtcOffset), filters.UtcOffset),

                DatesGroupingType.Week =>
                await GroupReportsByHealthRiskAndWeek(reports, filters.StartDate.DateTime.AddHours(filters.UtcOffset), filters.EndDate.DateTime.AddHours(filters.UtcOffset), epiWeekStartDay),

                _ =>
                throw new InvalidOperationException()
            };
        }

        private async Task<ReportByHealthRiskAndDateResponseDto> GroupReportsByHealthRiskAndDay(
            IQueryable<Report> reports,
            DateTime startDate,
            DateTime endDate,
            int utcOffset)
        {
            if (startDate > endDate)
            {
                return new ReportByHealthRiskAndDateResponseDto
                {
                    HealthRisks = Enumerable.Empty<ReportByHealthRiskAndDateResponseDto.ReportHealthRiskDto>(),
                    AllPeriods = new List<string>()
                };
            }
            //  SQL-safe data fetch
            var rawData = await reports
                .Where(r => r.ReceivedAt >= startDate && r.ReceivedAt <= endDate)
                .Select(r => new
                {
                    r.ReceivedAt,
                    r.ReportedCaseCount,
                    HealthRiskId = r.ProjectHealthRisk != null ? r.ProjectHealthRisk.HealthRiskId : 0, // Default to 0 if null, to avoid issues in join
                    ContentLanguageId = r.ProjectHealthRisk != null && r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage != null ? r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id : 1 //  Default to 1 if null, to avoid issues in join
                })
                .ToListAsync();

            //  Load health risk names ONCE
            var healthRiskNames = await _nyssContext.HealthRisks
                .SelectMany(hr => hr.LanguageContents.Select(lc => new
                {
                    hr.Id,
                    LanguageId = lc.ContentLanguage.Id,
                    lc.Name
                }))
                .ToListAsync();

            //  In-memory transformation (SAFE)
            var groupedReports = rawData
                .Select(r => new
                {
                    Period = r.ReceivedAt.AddHours(utcOffset).Date,
                    r.HealthRiskId,
                    HealthRiskName = healthRiskNames
                        .FirstOrDefault(n =>
                            n.Id == r.HealthRiskId &&
                            n.LanguageId == r.ContentLanguageId)?.Name,
                    Count = r.ReportedCaseCount
                })
                .Where(r => r.Count > 0)
                .GroupBy(r => new
                {
                    r.Period,
                    r.HealthRiskId,
                    r.HealthRiskName
                })
                .Select(g => new
                {
                    g.Key.Period,
                    g.Key.HealthRiskId,
                    g.Key.HealthRiskName,
                    Count = g.Sum(x => x.Count)
                })
                .ToList();

            //  Group by health risk
            var reportsGroupedByHealthRisk = groupedReports
                .GroupBy(r => new
                {
                    r.HealthRiskId,
                    r.HealthRiskName
                })
                .OrderByDescending(g => g.Sum(w => w.Count))
                .Select(g => new
                {
                    HealthRisk = g.Key,
                    Data = g.ToList()
                })
                .ToList();

            var maxHealthRiskCount = _config.View.NumberOfGroupedHealthRisksInDashboard;

            //  Truncate & merge "rest"
            var truncatedHealthRisksList = reportsGroupedByHealthRisk
                .Take(maxHealthRiskCount)
                .Concat(
                    reportsGroupedByHealthRisk
                        .Skip(maxHealthRiskCount)
                        .SelectMany(_ => _.Data)
                        .GroupBy(_ => true)
                        .Select(gr => new
                        {
                            HealthRisk = new
                            {
                                HealthRiskId = 0,
                                HealthRiskName = (string?)"(rest)"
                            },
                            Data = gr.ToList()
                        })
                )
                .Select(g => new ReportByHealthRiskAndDateResponseDto.ReportHealthRiskDto
                {
                    HealthRiskName = g.HealthRisk.HealthRiskName,
                    Periods = g.Data
                        .GroupBy(v => v.Period)
                        .OrderBy(v => v.Key)
                        .Select(v => new PeriodDto
                        {
                            Period = v.Key.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
                            Count = v.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = startDate
                .GetDaysRange(endDate)
                .Select(d => d.ToString("dd/MM/yy", CultureInfo.InvariantCulture))
                .ToList();

            return new ReportByHealthRiskAndDateResponseDto
            {
                HealthRisks = truncatedHealthRisksList,
                AllPeriods = allPeriods
            };
        }


        private async Task<ReportByHealthRiskAndDateResponseDto> GroupReportsByHealthRiskAndWeek(
            IQueryable<Report> reports,
            DateTime startDate,
            DateTime endDate,
            DayOfWeek epiWeekStartDay)
        {
            if (startDate > endDate)
            {
                return new ReportByHealthRiskAndDateResponseDto
                {
                    HealthRisks = Enumerable.Empty<ReportByHealthRiskAndDateResponseDto.ReportHealthRiskDto>(),
                    AllPeriods = new List<string>()
                };
            }
            //  SQL-safe projection ONLY
            var rawData = await reports
                .Where(r => r.ReceivedAt >= startDate && r.ReceivedAt <= endDate)
                .Select(r => new
                {
                    r.EpiYear,
                    r.EpiWeek,
                    r.ReportedCaseCount,
                    HealthRiskId = r.ProjectHealthRisk!.HealthRiskId,
                    ContentLanguageId = r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage!.Id
                })
                .ToListAsync();

            //  Load health risk names once
            var healthRiskNames = await _nyssContext.HealthRisks
                .SelectMany(hr => hr.LanguageContents.Select(lc => new
                {
                    hr.Id,
                    LanguageId = lc.ContentLanguage.Id,
                    lc.Name
                }))
                .ToListAsync();

            //  In-memory grouping (SAFE)
            var groupedReports = rawData
                .Where(r => r.ReportedCaseCount > 0)
                .Select(r => new
                {
                    Period = new
                    {
                        r.EpiYear,
                        r.EpiWeek
                    },
                    r.HealthRiskId,
                    HealthRiskName = healthRiskNames
                        .FirstOrDefault(n =>
                            n.Id == r.HealthRiskId &&
                            n.LanguageId == r.ContentLanguageId)?.Name,
                    Count = r.ReportedCaseCount
                })
                .GroupBy(r => new
                {
                    r.Period.EpiYear,
                    r.Period.EpiWeek,
                    r.HealthRiskId,
                    r.HealthRiskName
                })
                .Select(g => new
                {
                    Period = new
                    {
                        g.Key.EpiYear,
                        g.Key.EpiWeek
                    },
                    g.Key.HealthRiskId,
                    g.Key.HealthRiskName,
                    Count = g.Sum(x => x.Count)
                })
                .ToList();

            //  Group by health risk
            var reportsGroupedByHealthRisk = groupedReports
                .GroupBy(r => new
                {
                    r.HealthRiskId,
                    r.HealthRiskName
                })
                .OrderByDescending(g => g.Sum(w => w.Count))
                .Select(g => new
                {
                    HealthRisk = g.Key,
                    Data = g.ToList()
                })
                .ToList();

            var maxHealthRiskCount = _config.View.NumberOfGroupedHealthRisksInDashboard;

            //  Truncate & merge "rest"
            var truncatedHealthRisksList = reportsGroupedByHealthRisk
                .Take(maxHealthRiskCount)
                .Concat(
                    reportsGroupedByHealthRisk
                        .Skip(maxHealthRiskCount)
                        .SelectMany(_ => _.Data)
                        .GroupBy(_ => true)
                        .Select(g => new
                        {
                            HealthRisk = new
                            {
                                HealthRiskId = 0,
                                HealthRiskName = (string?)"(rest)"
                            },
                            Data = g.ToList()
                        })
                )
                .Select(x => new ReportByHealthRiskAndDateResponseDto.ReportHealthRiskDto
                {
                    HealthRiskName = x.HealthRisk.HealthRiskName,
                    Periods = x.Data
                        .GroupBy(v => new { v.Period.EpiYear, v.Period.EpiWeek })
                        .OrderBy(g => g.Key.EpiYear)
                        .ThenBy(g => g.Key.EpiWeek)
                        .Select(g => new PeriodDto
                        {
                            Period = $"{g.Key.EpiYear}/{g.Key.EpiWeek}",
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = _dateTimeProvider
                .GetEpiDateRange(startDate, endDate, epiWeekStartDay)
                .Select(d => $"{d.EpiYear}/{d.EpiWeek}")
                .ToList();

            return new ReportByHealthRiskAndDateResponseDto
            {
                HealthRisks = truncatedHealthRisksList,
                AllPeriods = allPeriods
            };
        }
    }
}
