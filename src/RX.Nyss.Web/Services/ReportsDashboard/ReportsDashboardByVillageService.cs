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
        Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(ReportsFilter filters, DatesGroupingType groupingType);
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

        public async Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(ReportsFilter filters, DatesGroupingType groupingType)
        {
            var reports = _reportService.GetHealthRiskEventReportsQuery(filters);

            return groupingType switch
            {
                DatesGroupingType.Day =>
                await GroupReportsByVillageAndDay(reports, filters.StartDate.DateTime.AddHours(filters.TimezoneOffset), filters.EndDate.DateTime.AddHours(filters.TimezoneOffset), filters.TimezoneOffset),

                DatesGroupingType.Week =>
                await GroupReportsByVillageAndWeek(reports, filters.StartDate.DateTime.AddHours(filters.TimezoneOffset), filters.EndDate.DateTime.AddHours(filters.TimezoneOffset)),

                _ =>
                throw new InvalidOperationException()
            };
        }

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate, int timezoneOffset)
        {
            var groupedReports = await reports
                .GroupBy(r => new
                {
                    Date = r.ReceivedAt.AddHours(timezoneOffset).Date,
                    VillageId = r.RawReport.Village.Id,
                    VillageName = r.RawReport.Village.Name
                })
                .Select(grouping => new
                {
                    Period = grouping.Key.Date,
                    Count = grouping.Sum(g => g.ReportedCaseCount),
                    grouping.Key.VillageId,
                    grouping.Key.VillageName
                })
                .Where(g => g.Count > 0)
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

            var truncatedVillagesList = reportsGroupedByVillages
                .Take(maxVillageCount)
                .Union(reportsGroupedByVillages
                    .Skip(maxVillageCount)
                    .SelectMany(_ => _.Data)
                    .GroupBy(_ => true)
                    .Select(g => new
                    {
                        Village = new
                        {
                            VillageId = 0,
                            VillageName = "(rest)"
                        },
                        Data = g.ToList()
                    })
                )
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(v => v.Key)
                        .Select(g => new PeriodDto
                        {
                            Period = g.Key.ToString("dd/MM", CultureInfo.InvariantCulture),
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = startDate.GetDaysRange(endDate)
                .Select(i => i.ToString("dd/MM", CultureInfo.InvariantCulture))
                .ToList();

            return new ReportByVillageAndDateResponseDto
            {
                Villages = truncatedVillagesList,
                AllPeriods = allPeriods
            };
        }

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => new
                {
                    r.EpiWeek,
                    r.EpiYear,
                    VillageId = r.RawReport.Village.Id,
                    VillageName = r.RawReport.Village.Name
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
                .Where(g => g.Count > 0)
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

            var truncatedVillagesList = reportsGroupedByVillages
                .Take(maxVillageCount)
                .Union(reportsGroupedByVillages
                    .Skip(maxVillageCount)
                    .SelectMany(_ => _.Data)
                    .GroupBy(_ => true)
                    .Select(g => new
                    {
                        Village = new
                        {
                            VillageId = 0,
                            VillageName = "(rest)"
                        },
                        Data = g.ToList()
                    })
                )
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(g => g.Key.EpiYear).ThenBy(g => g.Key.EpiWeek)
                        .Select(g => new PeriodDto
                        {
                            Period = g.Key.EpiWeek.ToString(),
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate)
                .Select(day => day.EpiWeek.ToString());

            return new ReportByVillageAndDateResponseDto
            {
                Villages = truncatedVillagesList,
                AllPeriods = allPeriods
            };
        }
    }
}
