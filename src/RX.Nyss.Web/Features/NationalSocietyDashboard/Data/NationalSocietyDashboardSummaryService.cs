using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Data
{
    public interface INationalSocietyDashboardSummaryService
    {
        Task<NationalSocietySummaryResponseDto> GetSummaryData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);
        Task<ReportByVillageAndDateResponseDto> GetReportsGroupedByVillageAndDate(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);
    }

    public class NationalSocietyDashboardSummaryService : INationalSocietyDashboardSummaryService
    {
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public NationalSocietyDashboardSummaryService(INyssWebConfig config, INyssContext nyssContext, IDateTimeProvider dateTimeProvider)
        {
            _config = config;
            _nyssContext = nyssContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public Task<NationalSocietySummaryResponseDto> GetSummaryData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto)
        {
            var assignedRawReports = NationalSocietyDashboardQueries.GetAssignedRawReports(_nyssContext.RawReports, nationalSocietyId, filtersDto);
            var validReports = NationalSocietyDashboardQueries.GetValidReports(_nyssContext.RawReports, nationalSocietyId, filtersDto);
            var dataCollectionPointReports = validReports.Where(r => r.DataCollectionPointCase != null);

            return _nyssContext.Projects
                .Where(ph => ph.NationalSocietyId == nationalSocietyId)
                .Select(ph => new
                {
                    allDataCollectorCount = AllDataCollectorCount(filtersDto, nationalSocietyId),
                    activeDataCollectorCount = assignedRawReports.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new NationalSocietySummaryResponseDto
                {
                    ReportCount = validReports.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.activeDataCollectorCount,
                    InactiveDataCollectorCount = data.allDataCollectorCount - data.activeDataCollectorCount,
                    DataCollectionPointSummary = new NationalSocietyDataCollectionPointsSummaryResponse
                    {
                        FromOtherVillagesCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.FromOtherVillagesCount ?? 0),
                        ReferredToHospitalCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.ReferredCount ?? 0),
                        DeathCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.DeathCount ?? 0),
                    }
                })
                .FirstOrDefaultAsync();
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

        private int AllDataCollectorCount(NationalSocietyDashboardFiltersRequestDto filtersDto, int nationalSocietyId) =>
            _nyssContext.DataCollectors
                .FilterByArea(filtersDto.Area)
                .FilterByType(NationalSocietyDashboardQueries.MapToDataCollectorType(filtersDto.ReportsType))
                .FilterByNationalSociety(nationalSocietyId)
                .FilterByTrainingMode(filtersDto.IsTraining)
                .FilterOnlyNotDeletedBefore(filtersDto.StartDate)
                .Count();
    }
}
