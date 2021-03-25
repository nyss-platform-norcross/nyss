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
                .FilterByIsDeployed()
                .FilterByArea(dataCollectorsFilters.Area)
                .FilterByName(dataCollectorsFilters.Name)
                .FilterBySupervisor(dataCollectorsFilters.SupervisorId)
                .FilterByTrainingMode(dataCollectorsFilters.TrainingStatus)
                .Include(dc => dc.Village);

            var toDate = _dateTimeProvider.UtcNow;
            var fromDate = toDate.AddMonths(-2);
            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalRows = await dataCollectors.CountAsync();

            var dataCollectorsWithReportsData = await GetDataCollectorsWithReportData(dataCollectors, fromDate, toDate);

            var dataCollectorCompleteness = GetDataCollectorCompleteness(dataCollectorsFilters, dataCollectorsWithReportsData, totalRows, toDate);

            var paginatedDataCollectorsWithReportsData = dataCollectorsWithReportsData
                .Page(dataCollectorsFilters.PageNumber, rowsPerPage);

            var dataCollectorPerformances = GetDataCollectorPerformance(paginatedDataCollectorsWithReportsData, toDate)
                .FilterByStatusLastWeek(dataCollectorsFilters.LastWeek)
                .FilterByStatusTwoWeeksAgo(dataCollectorsFilters.TwoWeeksAgo)
                .FilterByStatusThreeWeeksAgo(dataCollectorsFilters.ThreeWeeksAgo)
                .FilterByStatusFourWeeksAgo(dataCollectorsFilters.FourWeeksAgo)
                .FilterByStatusFiveWeeksAgo(dataCollectorsFilters.FiveWeeksAgo)
                .FilterByStatusSixWeeksAgo(dataCollectorsFilters.SixWeeksAgo)
                .FilterByStatusSevenWeeksAgo(dataCollectorsFilters.SevenWeeksAgo)
                .FilterByStatusEightWeeksAgo(dataCollectorsFilters.EightWeeksAgo)
                .AsPaginatedList(dataCollectorsFilters.PageNumber, totalRows, rowsPerPage);

            var dataCollectorPerformanceDto = new DataCollectorPerformanceResponseDto
            {
                Completeness = dataCollectorCompleteness,
                Performance = dataCollectorPerformances
            };

            return Success(dataCollectorPerformanceDto);
        }

        private async Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IIncludableQueryable<DataCollector, Village> dataCollectors, DateTime fromDate, DateTime toDate) =>
            await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.Village.Name,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                            && r.ReceivedAt >= fromDate.Date && r.ReceivedAt < toDate.Date.AddDays(1))
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt.Date
                        })
                }).ToListAsync();

        private DataCollectorCompleteness GetDataCollectorCompleteness(DataCollectorPerformanceFiltersRequestDto filters, IEnumerable<DataCollectorWithRawReportData> dataCollectors, int totalDataCollectors, DateTime toDate)
        {
            if (IsWeekFiltersActive(filters) || totalDataCollectors == 0)
            {
                return null;
            }

            var dataCollectorCompleteness = dataCollectors
                .Select(dc =>
                {
                    var reportsGroupedByWeek = dc.ReportsInTimeRange
                        .GroupBy(report => (int)(toDate - report.ReceivedAt).TotalDays / 7)
                        .ToList();
                    return new
                    {
                        HasReportedLastWeek = reportsGroupedByWeek
                            .Where(g => g.Key == 0)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedTwoWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 1)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedThreeWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 2)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedFourWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 3)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedFiveWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 4)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedSixWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 5)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedSevenWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 6)
                            .SelectMany(g => g)
                            .Any(),
                        HasReportedEightWeeksAgo = reportsGroupedByWeek
                            .Where(g => g.Key == 7)
                            .SelectMany(g => g)
                            .Any()
                    };
                }).Aggregate(new
                {
                    ActiveLastWeek = 0,
                    ActiveTwoWeeksAgo = 0,
                    ActiveThreeWeeksAgo = 0,
                    ActiveFourWeeksAgo = 0,
                    ActiveFiveWeeksAgo = 0,
                    ActiveSixWeeksAgo = 0,
                    ActiveSevenWeeksAgo = 0,
                    ActiveEightWeeksAgo = 0
                }, (a, dc) => new
                {
                    ActiveLastWeek = a.ActiveLastWeek + (dc.HasReportedLastWeek ? 1 : 0),
                    ActiveTwoWeeksAgo = a.ActiveTwoWeeksAgo + (dc.HasReportedTwoWeeksAgo ? 1 : 0),
                    ActiveThreeWeeksAgo = a.ActiveThreeWeeksAgo + (dc.HasReportedThreeWeeksAgo ? 1 : 0),
                    ActiveFourWeeksAgo = a.ActiveFourWeeksAgo + (dc.HasReportedFourWeeksAgo ? 1 : 0),
                    ActiveFiveWeeksAgo = a.ActiveFiveWeeksAgo + (dc.HasReportedFiveWeeksAgo ? 1 : 0),
                    ActiveSixWeeksAgo = a.ActiveSixWeeksAgo + (dc.HasReportedSixWeeksAgo ? 1 : 0),
                    ActiveSevenWeeksAgo = a.ActiveSevenWeeksAgo + (dc.HasReportedSevenWeeksAgo ? 1 : 0),
                    ActiveEightWeeksAgo = a.ActiveEightWeeksAgo + (dc.HasReportedEightWeeksAgo ? 1 : 0)
                });

            return new DataCollectorCompleteness
            {
                LastWeek = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveLastWeek,
                    Percentage = (dataCollectorCompleteness.ActiveLastWeek * 100) / totalDataCollectors
                },
                TwoWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveTwoWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveTwoWeeksAgo * 100) / totalDataCollectors
                },
                ThreeWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveThreeWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveThreeWeeksAgo * 100) / totalDataCollectors
                },
                FourWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveFourWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveFourWeeksAgo * 100) / totalDataCollectors
                },
                FiveWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveFiveWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveFiveWeeksAgo * 100) / totalDataCollectors
                },
                SixWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveSixWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveSixWeeksAgo * 100) / totalDataCollectors
                },
                SevenWeeksAgo = new Completeness
                {

                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveSevenWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveSevenWeeksAgo * 100) / totalDataCollectors
                },
                EightWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveEightWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveEightWeeksAgo * 100) / totalDataCollectors
                }
            };
        }

        private IEnumerable<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData, DateTime toDate) =>
            dataCollectorsWithReportsData
                .Select(dc =>
                {
                    var reportsGroupedByWeek = dc.ReportsInTimeRange
                        .GroupBy(report => (int)(toDate - report.ReceivedAt).TotalDays / 7)
                        .ToList();
                    return new DataCollectorPerformance
                    {
                        Name = dc.Name,
                        PhoneNumber = dc.PhoneNumber,
                        VillageName = dc.VillageName,
                        DaysSinceLastReport = reportsGroupedByWeek.Any()
                            ? (int)(toDate - reportsGroupedByWeek
                                .SelectMany(g => g)
                                .OrderByDescending(r => r.ReceivedAt)
                                .First().ReceivedAt).TotalDays
                            : -1,
                        StatusLastWeek = GetDataCollectorStatus(0, reportsGroupedByWeek),
                        StatusTwoWeeksAgo = GetDataCollectorStatus(1, reportsGroupedByWeek),
                        StatusThreeWeeksAgo = GetDataCollectorStatus(2, reportsGroupedByWeek),
                        StatusFourWeeksAgo = GetDataCollectorStatus(3, reportsGroupedByWeek),
                        StatusFiveWeeksAgo = GetDataCollectorStatus(4, reportsGroupedByWeek),
                        StatusSixWeeksAgo = GetDataCollectorStatus(5, reportsGroupedByWeek),
                        StatusSevenWeeksAgo = GetDataCollectorStatus(6, reportsGroupedByWeek),
                        StatusEightWeeksAgo = GetDataCollectorStatus(7, reportsGroupedByWeek)
                    };
                });

        private ReportingStatus GetDataCollectorStatus(int week, IEnumerable<IGrouping<int, RawReportData>> grouping)
        {
            var reports = grouping.Where(g => g.Key == week).SelectMany(g => g);
            return reports.Any()
                ? reports.All(x => x.IsValid) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;
        }

        private bool IsWeekFiltersActive(DataCollectorPerformanceFiltersRequestDto filters) =>
            IsReportingStatusFilterActiveForWeek(filters.LastWeek)
            || IsReportingStatusFilterActiveForWeek(filters.TwoWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.ThreeWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.FourWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.FiveWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.SixWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.SevenWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.EightWeeksAgo);

        private bool IsReportingStatusFilterActiveForWeek(PerformanceStatusFilterDto weekFilter) =>
            !weekFilter.NotReporting
            || !weekFilter.ReportingCorrectly
            || !weekFilter.ReportingWithErrors;
    }
}
