using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorPerformanceService
    {
        IList<Completeness> GetDataCollectorCompleteness(DataCollectorPerformanceFiltersRequestDto filters, IList<DataCollectorWithRawReportData> dataCollectors, List<EpiDate> epiDateRange);

        IList<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData, DateTime currentDate,
            List<EpiDate> epiDateRange);

        Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IQueryable<DataCollector> dataCollectors, DateTime fromDate,
            DateTime toDate, CancellationToken cancellationToken);

        Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IQueryable<DataCollector> dataCollectors, CancellationToken cancellationToken);
    }

    public class DataCollectorPerformanceService : IDataCollectorPerformanceService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public DataCollectorPerformanceService(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IQueryable<DataCollector> dataCollectors,
            DateTime fromDate, DateTime toDate, CancellationToken cancellationToken) =>
            await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.DataCollectorLocations.First().Village.Name,
                    CreatedAt = dc.CreatedAt,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                            && r.ReceivedAt >= fromDate && r.ReceivedAt <= toDate)
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt,
                            EpiDate = _dateTimeProvider.GetEpiDate(r.ReceivedAt)
                        }),
                    DatesNotDeployed = dc.DatesNotDeployed
                }).ToListAsync(cancellationToken);

        public async Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(
            IQueryable<DataCollector> dataCollectors, CancellationToken cancellationToken) =>
            await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.DataCollectorLocations.First().Village.Name,
                    CreatedAt = dc.CreatedAt,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value)
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt,
                            EpiDate = _dateTimeProvider.GetEpiDate(r.ReceivedAt)
                        }),
                    DatesNotDeployed = dc.DatesNotDeployed
                }).ToListAsync(cancellationToken);

        public IList<Completeness> GetDataCollectorCompleteness(DataCollectorPerformanceFiltersRequestDto filters, IList<DataCollectorWithRawReportData> dataCollectors, List<EpiDate> epiDateRange)
        {
            if (IsWeekFiltersActive(filters) || !dataCollectors.Any())
            {
                return null;
            }

            var dataCollectorCompleteness = dataCollectors
                .Select(dc => dc.ReportsInTimeRange
                    .GroupBy(report => report.EpiDate)
                    .Select(g => new
                    {
                        EpiDate = g.Key,
                        HasReported = g.Any()
                    })).SelectMany(x => x)
                .GroupBy(x => x.EpiDate)
                .Select(g => new
                {
                    EpiDate = g.Key,
                    Active = g.Sum(x => x.HasReported
                        ? 1
                        : 0)
                }).ToList();

            return epiDateRange.Select(epiDate => new Completeness
            {
                EpiWeek = epiDate.EpiWeek,
                TotalDataCollectors = TotalDataCollectorsDeployedInWeek(epiDate, dataCollectors),
                ActiveDataCollectors = dataCollectorCompleteness
                    .FirstOrDefault(dc => dc.EpiDate.EpiWeek == epiDate.EpiWeek && dc.EpiDate.EpiYear == epiDate.EpiYear)?.Active ?? 0
            }).ToList();
        }

        public IList<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData, DateTime currentDate,
            List<EpiDate> epiDateRange) =>
            dataCollectorsWithReportsData
                .Select(dc =>
                {
                    var reportsGroupedByWeek = dc.ReportsInTimeRange
                        .GroupBy(report => report.EpiDate)
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
                            : null,
                        PerformanceInEpiWeeks = epiDateRange
                            .Select(epiDate => new PerformanceInEpiWeek
                            {
                                EpiWeek = epiDate.EpiWeek,
                                EpiYear = epiDate.EpiYear,
                                ReportingStatus = DataCollectorExistedInWeek(epiDate, dc.CreatedAt) && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList())
                                    ? GetDataCollectorStatus(reportsGroupedByWeek.FirstOrDefault(r => r.Key.EpiWeek == epiDate.EpiWeek && r.Key.EpiYear == epiDate.EpiYear))
                                    : null
                            }).Reverse().ToList()
                    };
                }).ToList();

        private int TotalDataCollectorsDeployedInWeek(EpiDate epiDate, IEnumerable<DataCollectorWithRawReportData> dataCollectors) =>
            dataCollectors.Count(dc => DataCollectorExistedInWeek(epiDate, dc.CreatedAt)
                && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList()));

        private ReportingStatus GetDataCollectorStatus(IGrouping<EpiDate, RawReportData> grouping) =>
            grouping != null && grouping.Any()
                ? grouping.All(x => x.IsValid) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;

        private bool IsWeekFiltersActive(DataCollectorPerformanceFiltersRequestDto filters) =>
            filters.EpiWeekFilters.Any(IsReportingStatusFilterActiveForWeek);

        private bool IsReportingStatusFilterActiveForWeek(PerformanceStatusFilterDto weekFilter) =>
            !weekFilter.NotReporting
            || !weekFilter.ReportingCorrectly
            || !weekFilter.ReportingWithErrors;

        private bool DataCollectorExistedInWeek(EpiDate date, DateTime dataCollectorCreated)
        {
            var epiDataDataCollectorCreated = _dateTimeProvider.GetEpiDate(dataCollectorCreated);
            return epiDataDataCollectorCreated.EpiYear <= date.EpiYear && epiDataDataCollectorCreated.EpiWeek <= date.EpiWeek;
        }

        private bool DataCollectorWasDeployedInWeek(EpiDate date, List<DataCollectorNotDeployed> datesNotDeployed)
        {
            if (!datesNotDeployed.Any())
            {
                return true;
            }

            var firstDayOfEpiWeek = _dateTimeProvider.GetFirstDateOfEpiWeek(date.EpiYear, date.EpiWeek);
            var lastDayOfEpiWeek = firstDayOfEpiWeek.AddDays(7);

            var dataCollectorNotDeployedInWeek = datesNotDeployed.Any(d => d.StartDate < firstDayOfEpiWeek
                && (!d.EndDate.HasValue || d.EndDate > lastDayOfEpiWeek));

            return !dataCollectorNotDeployedInWeek;
        }
    }
}
