using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Extensions;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Services.ReportsDashboard
{
    public interface IReportsDashboardByVillageService
    {
        Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(ReportsFilter filters, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay);
    }

    public class ReportsDashboardByVillageService : IReportsDashboardByVillageService
    {
        private readonly IReportService _reportService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INyssWebConfig _config;

        public ReportsDashboardByVillageService(
            IReportService reportService,
            IDateTimeProvider dateTimeProvider,
            INyssWebConfig config)
        {
            _reportService = reportService;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
        }

        public async Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(ReportsFilter filters, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay)
        {
            var reports = _reportService.GetDashboardHealthRiskEventReportsQuery(filters)
                .Where(r => r.Location != null && r.RawReport != null && r.RawReport.Village != null);

            return groupingType switch
            {
                DatesGroupingType.Day =>
                await GroupReportsByVillageAndDay(reports, filters.StartDate.DateTime.AddHours(filters.UtcOffset), filters.EndDate.DateTime.AddHours(filters.UtcOffset), filters.UtcOffset),

                DatesGroupingType.Week =>
                await GroupReportsByVillageAndWeek(reports, filters.StartDate.DateTime.AddHours(filters.UtcOffset), filters.EndDate.DateTime.AddHours(filters.UtcOffset), epiWeekStartDay),

                _ =>
                throw new InvalidOperationException()
            };
        }


        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate, int utcOffset)
        {
            var groupedReports = await reports
                .Where(r => r.ReportedCaseCount > 0) // Filter early for better performance
                .Where(r => r.RawReport != null && r.RawReport.Village != null) // Null check
                .GroupBy(r => new
                {
                    Date = r.ReceivedAt.AddHours(utcOffset).Date,
                    VillageId = r.RawReport!.Village!.Id,
                    VillageName = r.RawReport!.Village!.Name
                })
                .Select(grouping => new
                {
                    Period = grouping.Key.Date,
                    Count = grouping.Sum(g => g.ReportedCaseCount),
                    grouping.Key.VillageId,
                    grouping.Key.VillageName
                })
                .AsSplitQuery()
                .ToListAsync();

            var reportsGroupedByVillages = groupedReports
                .GroupBy(r => new
                {
                    r.VillageId,
                    r.VillageName
                })
                .OrderByDescending(g => g.Sum(w => w.Count))
                .Select(g => new
                {
                    Village = g.Key,
                    Data = g.ToList()
                })
                .ToList();

            var maxVillageCount = _config.View.NumberOfGroupedVillagesInProjectDashboard;

            var topVillages = reportsGroupedByVillages.Take(maxVillageCount);
            var restVillages = reportsGroupedByVillages.Skip(maxVillageCount);

            var truncatedVillagesList = topVillages
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data
                        .GroupBy(v => v.Period)
                        .OrderBy(v => v.Key)
                        .Select(g => new PeriodDto
                        {
                            Period = g.Key.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            // Add "(rest)" village if there are more villages than the max count
            if (restVillages.Any())
            {
                var restData = restVillages
                    .SelectMany(v => v.Data)
                    .GroupBy(d => d.Period)
                    .OrderBy(g => g.Key)
                    .Select(g => new PeriodDto
                    {
                        Period = g.Key.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
                        Count = g.Sum(w => w.Count)
                    })
                    .ToList();

                truncatedVillagesList.Add(new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = "(rest)",
                    Periods = restData
                });
            }

            var allPeriods = startDate.GetDaysRange(endDate)
                .Select(i => i.ToString("dd/MM/yy", CultureInfo.InvariantCulture))
                .ToList();

            return new ReportByVillageAndDateResponseDto
            {
                Villages = truncatedVillagesList,
                AllPeriods = allPeriods
            };
        }


        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate, DayOfWeek epiWeekStartDay)
        {
            var groupedReports = await reports
                .Where(r => r.ReportedCaseCount > 0) // Filter early for better performance
                .Where(r => r.RawReport != null && r.RawReport.Village != null) // Null check
                .GroupBy(r => new
                {
                    r.EpiWeek,
                    r.EpiYear,
                    VillageId = r.RawReport!.Village!.Id,
                    VillageName = r.RawReport!.Village!.Name
                })
                .Select(grouping => new
                {
                    Period = new
                    {
                        grouping.Key.EpiWeek,
                        grouping.Key.EpiYear
                    },
                    Count = grouping.Sum(g => g.ReportedCaseCount),
                    grouping.Key.VillageId,
                    grouping.Key.VillageName
                })
                .ToListAsync();

            var reportsGroupedByVillages = groupedReports
                .GroupBy(r => new
                {
                    r.VillageId,
                    r.VillageName
                })
                .OrderByDescending(g => g.Sum(w => w.Count))
                .Select(g => new
                {
                    Village = g.Key,
                    Data = g.ToList()
                })
                .ToList();

            var maxVillageCount = _config.View.NumberOfGroupedVillagesInProjectDashboard;

            var topVillages = reportsGroupedByVillages.Take(maxVillageCount);
            var restVillages = reportsGroupedByVillages.Skip(maxVillageCount);

            var truncatedVillagesList = topVillages
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data
                        .GroupBy(v => v.Period)
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

            // Add "(rest)" village if there are more villages than the max count
            if (restVillages.Any())
            {
                var restData = restVillages
                    .SelectMany(v => v.Data)
                    .GroupBy(d => d.Period)
                    .OrderBy(g => g.Key.EpiYear)
                    .ThenBy(g => g.Key.EpiWeek)
                    .Select(g => new PeriodDto
                    {
                        Period = $"{g.Key.EpiYear}/{g.Key.EpiWeek}",
                        Count = g.Sum(w => w.Count)
                    })
                    .ToList();

                truncatedVillagesList.Add(new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = "(rest)",
                    Periods = restData
                });
            }

            var allPeriods = _dateTimeProvider.GetEpiDateRange(startDate, endDate, epiWeekStartDay)
                .Select(day => $"{day.EpiYear}/{day.EpiWeek}")
                .ToList();

            return new ReportByVillageAndDateResponseDto
            {
                Villages = truncatedVillagesList,
                AllPeriods = allPeriods
            };
        }
    }
}
