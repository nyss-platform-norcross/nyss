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
        IList<Completeness> GetDataCollectorCompleteness(IList<DataCollectorWithRawReportData> dataCollectors, List<EpiDate> epiDateRange, DayOfWeek epiWeekStartDay);

        IList<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData, DateTime currentDate,
            List<EpiDate> epiDateRange, DayOfWeek epiWeekStartDay);

        Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IQueryable<DataCollector> dataCollectors, DateTime fromDate,
            DateTime toDate, CancellationToken cancellationToken, TrainingStatusDto dataCollectorTrainingStatus);

        Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IQueryable<DataCollector> dataCollectors, CancellationToken cancellationToken, TrainingStatusDto dataCollectorTrainingStatus);
    }

    public class DataCollectorPerformanceService : IDataCollectorPerformanceService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public DataCollectorPerformanceService(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(IQueryable<DataCollector> dataCollectors,
            DateTime fromDate, DateTime toDate, CancellationToken cancellationToken, TrainingStatusDto dataCollectorTrainingStatus) =>
            await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.DataCollectorLocations.First().Village.Name,
                    CreatedAt = dc.CreatedAt,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && r.IsTraining.Value == IsDataCollectorInTraining(dataCollectorTrainingStatus)
                            && r.ReceivedAt >= fromDate && r.ReceivedAt <= toDate)
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt,
                            EpiDate = _dateTimeProvider.GetEpiDate(r.ReceivedAt, r.DataCollector.Project.NationalSociety.EpiWeekStartDay)
                        }),
                    DatesNotDeployed = dc.DatesNotDeployed
                }).ToListAsync(cancellationToken);

        public async Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(
            IQueryable<DataCollector> dataCollectors, CancellationToken cancellationToken, TrainingStatusDto dataCollectorTrainingStatus) =>
            await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.DataCollectorLocations.First().Village.Name,
                    DistrictName = dc.DataCollectorLocations.First().Village.District.Name,
                    RegionName = dc.DataCollectorLocations.First().Village.District.Region.Name,
                    CreatedAt = dc.CreatedAt,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && r.IsTraining.Value == IsDataCollectorInTraining(dataCollectorTrainingStatus))
                        .Select(r => new RawReportData
                        {
                            IsValid =  r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt,
                            EpiDate = _dateTimeProvider.GetEpiDate(r.ReceivedAt, r.DataCollector.Project.NationalSociety.EpiWeekStartDay)
                        }),

                    DatesNotDeployed = dc.DatesNotDeployed
                }).ToListAsync(cancellationToken);

        public IList<Completeness> GetDataCollectorCompleteness(IList<DataCollectorWithRawReportData> dataCollectors, List<EpiDate> epiDateRange, DayOfWeek epiWeekStartDay)
        {
            if (!dataCollectors.Any())
            {
                return new List<Completeness>();
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
                TotalDataCollectors = TotalDataCollectorsDeployedInWeek(epiDate, epiWeekStartDay, dataCollectors),
                ActiveDataCollectors = dataCollectorCompleteness
                    .FirstOrDefault(dc => dc.EpiDate.EpiWeek == epiDate.EpiWeek && dc.EpiDate.EpiYear == epiDate.EpiYear)?.Active ?? 0
            }).ToList();
        }

        public IList<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData, DateTime currentDate,
            List<EpiDate> epiDateRange, DayOfWeek epiWeekStartDay) =>
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
                        DistrictName = dc.DistrictName,
                        RegionName = dc.RegionName,
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
                                ReportingStatus = DataCollectorExistedInWeek(epiDate, dc.CreatedAt, epiWeekStartDay) && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList(), epiWeekStartDay)
                                    ? GetDataCollectorStatus(reportsGroupedByWeek.FirstOrDefault(r => r.Key.EpiWeek == epiDate.EpiWeek && r.Key.EpiYear == epiDate.EpiYear))
                                    : null
                            }).Reverse().ToList()
                    };
                }).ToList();

        private int TotalDataCollectorsDeployedInWeek(EpiDate epiDate, DayOfWeek epiWeekStartDay, IEnumerable<DataCollectorWithRawReportData> dataCollectors) =>
            dataCollectors.Count(dc => DataCollectorExistedInWeek(epiDate, dc.CreatedAt, epiWeekStartDay)
                && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList(), epiWeekStartDay));

        private bool IsDataCollectorInTraining(TrainingStatusDto dataCollectorTrainingStatus) => dataCollectorTrainingStatus == TrainingStatusDto.InTraining;

        private ReportingStatus GetDataCollectorStatus(IGrouping<EpiDate, RawReportData> grouping) =>
            grouping != null && grouping.Any()
                ? grouping.All(x => x.IsValid ) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;

        private bool DataCollectorExistedInWeek(EpiDate date, DateTime dataCollectorCreated, DayOfWeek epiWeekStartDay)
        {
            var epiDataDataCollectorCreated = _dateTimeProvider.GetEpiDate(dataCollectorCreated, epiWeekStartDay);
            return epiDataDataCollectorCreated.EpiYear < date.EpiYear
                || (epiDataDataCollectorCreated.EpiYear == date.EpiYear && epiDataDataCollectorCreated.EpiWeek <= date.EpiWeek);
        }

        private bool DataCollectorWasDeployedInWeek(EpiDate date, List<DataCollectorNotDeployed> datesNotDeployed, DayOfWeek epiWeekStartDay)
        {
            if (!datesNotDeployed.Any())
            {
                return true;
            }

            var firstDayOfEpiWeek = _dateTimeProvider.GetFirstDateOfEpiWeek(date.EpiYear, date.EpiWeek, epiWeekStartDay);
            var lastDayOfEpiWeek = firstDayOfEpiWeek.AddDays(7);

            var dataCollectorNotDeployedInWeek = datesNotDeployed.Any(d => d.StartDate < lastDayOfEpiWeek
                && (!d.EndDate.HasValue || d.EndDate > lastDayOfEpiWeek));

            return !dataCollectorNotDeployedInWeek;
        }
    }
}
