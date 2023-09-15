using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Extensions;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Services.ReportsDashboard
{
    public interface IReportsDashboardByDataCollectionPointService
    {
        Task<IEnumerable<DataCollectionPointsReportsByDateDto>> GetDataCollectionPointReports(ReportsFilter filters, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay);
    }

    public class ReportsDashboardByDataCollectionPointService : IReportsDashboardByDataCollectionPointService
    {
        private readonly IReportService _reportService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ReportsDashboardByDataCollectionPointService(
            IReportService reportService,
            IDateTimeProvider dateTimeProvider)
        {
            _reportService = reportService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IEnumerable<DataCollectionPointsReportsByDateDto>> GetDataCollectionPointReports(ReportsFilter filters, DatesGroupingType groupingType, DayOfWeek epiWeekStartDay)
        {
            var dataCollectionPointReports = _reportService.GetDashboardHealthRiskEventReportsQuery(filters)
                .Where(r => r.ReportType == ReportType.DataCollectionPoint);

            return groupingType switch
            {
                DatesGroupingType.Day =>
                await GroupReportsByDataCollectionPointFeaturesAndDay(dataCollectionPointReports, filters.StartDate.Date, filters.EndDate.Date),

                DatesGroupingType.Week =>
                await GroupReportsByDataCollectionPointFeaturesAndWeek(dataCollectionPointReports, filters.StartDate.Date, filters.EndDate.Date, epiWeekStartDay),

                _ =>
                throw new InvalidOperationException()
            };
        }

        private static async Task<IEnumerable<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.ReceivedAt,
                    r.DataCollectionPointCase.ReferredCount,
                    r.DataCollectionPointCase.DeathCount,
                    r.DataCollectionPointCase.FromOtherVillagesCount
                })
                .GroupBy(r => r.ReceivedAt.Date)
                .Select(grouping => new
                {
                    Period = grouping.Key,
                    ReferredCount = (int)grouping.Sum(g => g.ReferredCount),
                    DeathCount = (int)grouping.Sum(g => g.DeathCount),
                    FromOtherVillagesCount = (int)grouping.Sum(g => g.FromOtherVillagesCount)
                })
                .ToListAsync();

            var missingDays = startDate.GetDaysRange(endDate)
                .Except(groupedReports.Select(x => x.Period))
                .Select(day => new
                {
                    Period = day,
                    ReferredCount = 0,
                    DeathCount = 0,
                    FromOtherVillagesCount = 0
                });

            return groupedReports
                .Union(missingDays)
                .OrderBy(r => r.Period)
                .Select(x => new DataCollectionPointsReportsByDateDto
                {
                    Period = x.Period.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
                    ReferredCount = x.ReferredCount,
                    DeathCount = x.DeathCount,
                    FromOtherVillagesCount = x.FromOtherVillagesCount
                })
                .ToList();
        }

        private async Task<IEnumerable<DataCollectionPointsReportsByDateDto>> GroupReportsByDataCollectionPointFeaturesAndWeek(IQueryable<Report> reports, DateTime startDate, DateTime endDate, DayOfWeek epiWeekStartDay)
        {
            var groupedReports = await reports
                .Select(r => new
                {
                    r.EpiYear,
                    r.EpiWeek,
                    r.DataCollectionPointCase.ReferredCount,
                    r.DataCollectionPointCase.DeathCount,
                    r.DataCollectionPointCase.FromOtherVillagesCount
                })
                .GroupBy(r => new
                {
                    r.EpiYear,
                    r.EpiWeek
                })
                .Select(grouping => new
                {
                    EpiPeriod = grouping.Key,
                    ReferredCount = (int)grouping.Sum(g => g.ReferredCount),
                    DeathCount = (int)grouping.Sum(g => g.DeathCount),
                    FromOtherVillagesCount = (int)grouping.Sum(g => g.FromOtherVillagesCount)
                })
                .ToListAsync();

            var missingWeeks = _dateTimeProvider.GetEpiDateRange(startDate, endDate, epiWeekStartDay)
                .Where(epiDate => !groupedReports.Any(r => r.EpiPeriod.EpiYear == epiDate.EpiYear && r.EpiPeriod.EpiWeek == epiDate.EpiWeek))
                .Select(epiDate => new
                {
                    EpiPeriod = new
                    {
                        epiDate.EpiYear,
                        epiDate.EpiWeek
                    },
                    ReferredCount = 0,
                    DeathCount = 0,
                    FromOtherVillagesCount = 0
                });

            return groupedReports
                .Union(missingWeeks)
                .OrderBy(r => r.EpiPeriod.EpiYear)
                .ThenBy(r => r.EpiPeriod.EpiWeek)
                .Select(x => new DataCollectionPointsReportsByDateDto
                {
                    Period = x.EpiPeriod.EpiWeek.ToString(),
                    ReferredCount = x.ReferredCount,
                    DeathCount = x.DeathCount,
                    FromOtherVillagesCount = x.FromOtherVillagesCount
                })
                .ToList();
        }
    }
}
