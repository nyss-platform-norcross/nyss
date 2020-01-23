using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Projects.Dto;
using RX.Nyss.Web.Features.Reports;

namespace RX.Nyss.Web.Services.ReportsDashboard
{
    public interface IReportsDashboardByDateService
    {
        Task<IList<ReportByDateResponseDto>> GetReportsGroupedByDate(ReportsFilter filters, DatesGroupingType groupingType);
    }

    public class ReportsDashboardByDateService : IReportsDashboardByDateService
    {
        private readonly IReportService _reportService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ReportsDashboardByDateService(
            IReportService reportService,
            IDateTimeProvider dateTimeProvider)
        {
            _reportService = reportService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IList<ReportByDateResponseDto>> GetReportsGroupedByDate(ReportsFilter filters, DatesGroupingType groupingType)
        {
            var reports = _reportService.GetHealthRiskEventReportsQuery(filters);

            return groupingType switch
            {
                DatesGroupingType.Day =>
                await GroupReportsByDay(reports, filters.StartDate.Date, filters.EndDate.Date),

                DatesGroupingType.Week =>
                await GroupReportsByWeek(reports, filters.StartDate.Date, filters.EndDate.Date),

                _ =>
                throw new InvalidOperationException()
            };
        }

        private static async Task<IList<ReportByDateResponseDto>> GroupReportsByDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt,
                    Total = r.ReportedCaseCount,
                })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key,
                    Count = grouping.Sum(r => r.Total)
                })
                .ToListAsync();

            var missingDays = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i))
                .Where(day => !groupedReports.Any(r => r.Period == day))
                .Select(day => new
                {
                    Period = day,
                    Count = 0
                });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new ReportByDateResponseDto
                {
                    Period = x.Period.ToString("dd/MM", CultureInfo.InvariantCulture),
                    Count = x.Count
                })
                .ToList();
        }

        private async Task<IList<ReportByDateResponseDto>> GroupReportsByWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new { r.EpiYear, r.EpiWeek, Total = r.ReportedCaseCount })
                .GroupBy(r => new { r.EpiYear, r.EpiWeek })
                .Select(grouping => new { EpiPeriod = grouping.Key, Count = grouping.Sum(r => r.Total) })
                .ToListAsync();

            var missingWeeks = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate)
                .Where(epiDate => !groupedReports.Any(r => r.EpiPeriod.EpiYear == epiDate.EpiYear && r.EpiPeriod.EpiWeek == epiDate.EpiWeek))
                .Select(epiDate => new { EpiPeriod = new { epiDate.EpiYear, epiDate.EpiWeek }, Count = 0 });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.EpiYear)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new ReportByDateResponseDto { Period = x.EpiPeriod.EpiWeek.ToString(), Count = x.Count })
                .ToList();
        }
    }
}
