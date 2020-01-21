using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Data
{
    public interface INationalSocietyDashboardReportsByVillageService
    {
        Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);
    }

    public class NationalSocietyDashboardReportsByVillageService : INationalSocietyDashboardReportsByVillageService
    {
        private readonly INyssContext _nyssContext;
        private readonly INyssWebConfig _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        public NationalSocietyDashboardReportsByVillageService(INyssContext nyssContext, INyssWebConfig config, IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _config = config;
            _dateTimeProvider = dateTimeProvider;
        }
        public async Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto)
        {
            var reports = NationalSocietyDashboardQueries.GetValidReports(_nyssContext.RawReports, nationalSocietyId, filtersDto);

            return filtersDto.GroupingType switch
            {
                NationalSocietyDashboardFiltersRequestDto.GroupingTypeDto.Day =>
                await GroupReportsByVillageAndDay(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                NationalSocietyDashboardFiltersRequestDto.GroupingTypeDto.Week =>
                await GroupReportsByVillageAndWeek(reports, filtersDto.StartDate.Date, filtersDto.EndDate.Date),

                _ =>
                throw new InvalidOperationException()
            };
        }

        private async Task<ReportByVillageAndDateResponseDto> GroupReportsByVillageAndDay(IQueryable<Report> reports, DateTime startDate, DateTime endDate)
        {
            var groupedReports = await reports
                .GroupBy(r => new { r.ReceivedAt.Date, VillageId = r.Village.Id, VillageName = r.Village.Name })
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
                .GroupBy(r => new { r.VillageId, r.VillageName })
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
                        Village = new { VillageId = 0, VillageName = "(rest)" },
                        Data = g.ToList()
                    })
                )
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(v => v.Key)
                        .Select(g => new ReportByVillageAndDateResponseDto.PeriodDto
                        {
                            Period = g.Key.ToString("dd/MM", CultureInfo.InvariantCulture),
                            Count = g.Sum(w => w.Count)
                        })
                        .ToList()
                })
                .ToList();

            var allPeriods = Enumerable
                .Range(0, endDate.Subtract(startDate).Days + 1)
                .Select(i => startDate.AddDays(i).ToString("dd/MM", CultureInfo.InvariantCulture))
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
                .GroupBy(r => new { r.EpiWeek, ReceivedAt = r.ReceivedAt.Date, VillageId = r.Village.Id, VillageName = r.Village.Name })
                .Select(grouping => new
                {
                    Period = new
                    {
                        grouping.Key.EpiWeek,
                        grouping.Key.ReceivedAt.Year
                    },
                    Count = grouping.Sum(g => g.ReportedCaseCount),
                    grouping.Key.VillageId,
                    grouping.Key.VillageName
                })
                .Where(g => g.Count > 0)
                .ToListAsync();

            var reportsGroupedByVillages = groupedReports
                .GroupBy(r => new { r.VillageId, r.VillageName })
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
                        Village = new { VillageId = 0, VillageName = "(rest)" },
                        Data = g.ToList()
                    })
                )
                .Select(x => new ReportByVillageAndDateResponseDto.VillageDto
                {
                    Name = x.Village.VillageName,
                    Periods = x.Data.GroupBy(v => v.Period).OrderBy(g => g.Key.Year).ThenBy(g => g.Key.EpiWeek)
                        .Select(g => new ReportByVillageAndDateResponseDto.PeriodDto
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
