using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorPerformanceService
    {
        Task<Result<DataCollectorPerformanceResponseDto>> Performance(int projectId, DataCollectorPerformanceFiltersRequestDto dataCollectorsFilters);
    }

    public class DataCollectorPerformanceService : IDataCollectorPerformanceService
    {
        private readonly INyssWebConfig _config;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDataCollectorService _dataCollectorService;

        public DataCollectorPerformanceService(INyssWebConfig config, IDateTimeProvider dateTimeProvider, IDataCollectorService dataCollectorService)
        {
            _config = config;
            _dateTimeProvider = dateTimeProvider;
            _dataCollectorService = dataCollectorService;
        }

        public async Task<Result<DataCollectorPerformanceResponseDto>> Performance(int projectId, DataCollectorPerformanceFiltersRequestDto dataCollectorsFilters)
        {
            var dataCollectors = (await _dataCollectorService.GetDataCollectorsForCurrentUserInProject(projectId))
                .FilterOnlyNotDeleted()
                .FilterOnlyDeployed()
                .FilterByArea(dataCollectorsFilters.Locations)
                .FilterByName(dataCollectorsFilters.Name)
                .FilterBySupervisor(dataCollectorsFilters.SupervisorId)
                .FilterByTrainingMode(dataCollectorsFilters.TrainingStatus)
                .Include(dc => dc.DataCollectorLocations);

            var currentDate = _dateTimeProvider.UtcNow;
            var fromEpiWeek = dataCollectorsFilters.EpiWeekFilters.First().EpiWeek;
            var toEpiWeek = dataCollectorsFilters.EpiWeekFilters.Last().EpiWeek;
            var fromDate = _dateTimeProvider.GetFirstDateOfEpiWeek(currentDate.AddDays(-8 * 7).Year, fromEpiWeek);
            var previousEpiWeekDate = _dateTimeProvider.GetFirstDateOfEpiWeek(currentDate.AddDays(-7).Year, toEpiWeek);
            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalRows = await dataCollectors.CountAsync();
            var epiDateRange = _dateTimeProvider.GetEpiDateRange(fromDate, previousEpiWeekDate).ToList();

            var dataCollectorsWithReportsData = await GetDataCollectorsWithReportData(dataCollectors, fromDate, currentDate);

            var dataCollectorCompleteness = GetDataCollectorCompleteness(dataCollectorsFilters, dataCollectorsWithReportsData, totalRows, epiDateRange);

            var paginatedDataCollectorsWithReportsData = dataCollectorsWithReportsData
                .Page(dataCollectorsFilters.PageNumber, rowsPerPage);

            var dataCollectorPerformances = GetDataCollectorPerformance(paginatedDataCollectorsWithReportsData, currentDate, epiDateRange)
                .FilterByStatusForEpiWeeks(dataCollectorsFilters.EpiWeekFilters)
                .AsPaginatedList(dataCollectorsFilters.PageNumber, totalRows, rowsPerPage);

            var dataCollectorPerformanceDto = new DataCollectorPerformanceResponseDto
            {
                Completeness = dataCollectorCompleteness,
                Performance = dataCollectorPerformances
            };

            return Success(dataCollectorPerformanceDto);
        }

        private async Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IIncludableQueryable<DataCollector, IEnumerable<DataCollectorLocation>> dataCollectors, DateTime fromDate, DateTime toDate) =>
            await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.DataCollectorLocations.First().Village.Name,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                            && r.ReceivedAt >= fromDate && r.ReceivedAt <= toDate)
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt,
                            EpiWeek = _dateTimeProvider.GetEpiWeek(r.ReceivedAt)
                        })
                }).ToListAsync();

        private List<Completeness> GetDataCollectorCompleteness(DataCollectorPerformanceFiltersRequestDto filters, IEnumerable<DataCollectorWithRawReportData> dataCollectors, int totalDataCollectors, List<EpiDate> epiDateRange)
        {
            if (IsWeekFiltersActive(filters) || totalDataCollectors == 0)
            {
                return null;
            }

            var dataCollectorCompleteness = dataCollectors
                .Select(dc => dc.ReportsInTimeRange
                    .GroupBy(report => report.EpiWeek)
                    .Select(g => new
                    {
                        EpiWeek = g.Key,
                        HasReported = g.Any()
                    })).SelectMany(x => x)
                .GroupBy(x => x.EpiWeek)
                .Select(g => new
                {
                    EpiWeek = g.Key,
                    Active = g.Sum(x => x.HasReported
                        ? 1
                        : 0)
                }).ToList();

            return epiDateRange.Select(epiDate => new Completeness
            {
                EpiWeek = epiDate.EpiWeek,
                TotalDataCollectors = totalDataCollectors,
                ActiveDataCollectors = dataCollectorCompleteness.FirstOrDefault(dc => dc.EpiWeek == epiDate.EpiWeek)?.Active ?? 0,
                Percentage = (dataCollectorCompleteness.FirstOrDefault(dc => dc.EpiWeek == epiDate.EpiWeek)?.Active ?? 0) * 100 / totalDataCollectors
            }).Reverse().ToList();
        }

        private IEnumerable<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData, DateTime currentDate, List<EpiDate> epiDateRange) =>
            dataCollectorsWithReportsData
                .Select(dc =>
                {
                    var reportsGroupedByWeek = dc.ReportsInTimeRange
                        .GroupBy(report => report.EpiWeek)
                        .ToList();
                    return new DataCollectorPerformance
                    {
                        Name = dc.Name,
                        PhoneNumber = dc.PhoneNumber,
                        VillageName = dc.VillageName,
                        DaysSinceLastReport = reportsGroupedByWeek.Any()
                            ? (int)(currentDate - reportsGroupedByWeek
                                .SelectMany(g => g)
                                .OrderByDescending(r => r.ReceivedAt)
                                .First().ReceivedAt).TotalDays
                            : -1,
                        PerformanceInEpiWeeks = epiDateRange
                            .Select(epiDate => new PerformanceInEpiWeek
                            {
                                EpiWeek = epiDate.EpiWeek,
                                ReportingStatus = GetDataCollectorStatus(reportsGroupedByWeek.FirstOrDefault(r => r.Key == epiDate.EpiWeek))
                            }).Reverse().ToList()
                    };
                });

        private ReportingStatus GetDataCollectorStatus(IGrouping<int, RawReportData> grouping) =>
            grouping != null && grouping.Any()
                ? grouping.All(x => x.IsValid) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;

        private bool IsWeekFiltersActive(DataCollectorPerformanceFiltersRequestDto filters) =>
            filters.EpiWeekFilters.Any(IsReportingStatusFilterActiveForWeek);

        private bool IsReportingStatusFilterActiveForWeek(PerformanceStatusFilterDto weekFilter) =>
            !weekFilter.NotReporting
            || !weekFilter.ReportingCorrectly
            || !weekFilter.ReportingWithErrors;
    }
}
