using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.DataCollectors;

public interface IDataCollectorPerformanceService
{
    IList<Completeness> GetDataCollectorCompleteness(
        IList<DataCollectorWithRawReportData> dataCollectors,
        List<EpiDate> epiDateRange,
        DayOfWeek epiWeekStartDay);

    IList<DataCollectorPerformance> GetDataCollectorPerformance(
        IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData,
        DateTime currentDate,
        List<EpiDate> epiDateRange,
        DayOfWeek epiWeekStartDay);

    Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(
        IQueryable<DataCollector> dataCollectors,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken,
        TrainingStatusDto dataCollectorTrainingStatus,
        IReadOnlyCollection<int>? selectedHealthRiskIds = null,
        bool includeUncategorizedInvalidReports = true);

    Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(
        IQueryable<DataCollector> dataCollectors,
        CancellationToken cancellationToken,
        TrainingStatusDto dataCollectorTrainingStatus,
        IReadOnlyCollection<int>? selectedHealthRiskIds = null,
        bool includeUncategorizedInvalidReports = true);
}

public class DataCollectorPerformanceService : IDataCollectorPerformanceService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DataCollectorPerformanceService(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(
        IQueryable<DataCollector> dataCollectors,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken,
        TrainingStatusDto dataCollectorTrainingStatus,
        IReadOnlyCollection<int>? selectedHealthRiskIds = null,
        bool includeUncategorizedInvalidReports = true) =>
        FetchDataCollectors(
            dataCollectors,
            cancellationToken,
            dataCollectorTrainingStatus,
            fromDate,
            toDate,
            selectedHealthRiskIds,
            includeUncategorizedInvalidReports);

    public Task<List<DataCollectorWithRawReportData>> GetDataCollectorsWithReportData(
        IQueryable<DataCollector> dataCollectors,
        CancellationToken cancellationToken,
        TrainingStatusDto dataCollectorTrainingStatus,
        IReadOnlyCollection<int>? selectedHealthRiskIds = null,
        bool includeUncategorizedInvalidReports = true) =>
        FetchDataCollectors(
            dataCollectors,
            cancellationToken,
            dataCollectorTrainingStatus,
            selectedHealthRiskIds: selectedHealthRiskIds,
            includeUncategorizedInvalidReports: includeUncategorizedInvalidReports);

    private async Task<List<DataCollectorWithRawReportData>> FetchDataCollectors(
        IQueryable<DataCollector> dataCollectors,
        CancellationToken cancellationToken,
        TrainingStatusDto dataCollectorTrainingStatus,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        IReadOnlyCollection<int>? selectedHealthRiskIds = null,
        bool includeUncategorizedInvalidReports = true)
    {
        var inTraining = dataCollectorTrainingStatus == TrainingStatusDto.InTraining;
        var filterByHealthRisk = selectedHealthRiskIds is { Count: > 0 };

        var result = await dataCollectors
            .Select(dc => new DataCollectorWithRawReportData
            {
                Name = dc.Name ?? string.Empty,
                PhoneNumber = dc.PhoneNumber ?? string.Empty,
                VillageName = dc.DataCollectorLocations!
                    .Select(l => l.Village!.Name)
                    .FirstOrDefault() ?? string.Empty,
                DistrictName = dc.DataCollectorLocations!
                    .Select(l => l.Village!.District!.Name)
                    .FirstOrDefault() ?? string.Empty,
                RegionName = dc.DataCollectorLocations!
                    .Select(l => l.Village!.District!.Region!.Name)
                    .FirstOrDefault() ?? string.Empty,
                CreatedAt = dc.CreatedAt,
                EpiWeekStartDay = dc.Project!.NationalSociety.EpiWeekStartDay,
                ReportsInTimeRange = dc.RawReports!
                    .Where(r => r.IsTraining.HasValue
                        && r.IsTraining.Value == inTraining
                        && (fromDate == null || r.ReceivedAt >= fromDate)
                        && (toDate == null || r.ReceivedAt <= toDate)
                        && (!filterByHealthRisk
                            || (r.ReportId.HasValue
                                && r.Report!.ProjectHealthRisk != null
                                && selectedHealthRiskIds!.Contains(r.Report.ProjectHealthRisk.Id))
                            || (includeUncategorizedInvalidReports && !r.ReportId.HasValue)))
                    .Select(r => new RawReportData
                    {
                        IsValid = r.ReportId.HasValue,
                        ReceivedAt = r.ReceivedAt,
                    }),
                DatesNotDeployed = dc.DatesNotDeployed ?? new List<DataCollectorNotDeployed>()
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var dc in result)
        {
            var reports = dc.ReportsInTimeRange.ToList();
            foreach (var report in reports)
            {
                report.EpiDate = _dateTimeProvider.GetEpiDate(report.ReceivedAt, dc.EpiWeekStartDay);
            }
            dc.ReportsInTimeRange = reports;
        }

        return result;
    }

    public IList<Completeness> GetDataCollectorCompleteness(
        IList<DataCollectorWithRawReportData> dataCollectors,
        List<EpiDate> epiDateRange,
        DayOfWeek epiWeekStartDay)
    {
        if (!dataCollectors.Any())
        {
            return new List<Completeness>();
        }

        var activeByEpiDate = dataCollectors
            .SelectMany(dc => dc.ReportsInTimeRange
                .GroupBy(r => r.EpiDate)
                .Select(g => g.Key))
            .GroupBy(epiDate => epiDate)
            .ToDictionary(g => g.Key, g => g.Count());

        return epiDateRange
            .Select(epiDate => new Completeness
            {
                EpiWeek = epiDate.EpiWeek,
                TotalDataCollectors = TotalDataCollectorsDeployedInWeek(epiDate, epiWeekStartDay, dataCollectors),
                ActiveDataCollectors = activeByEpiDate.TryGetValue(epiDate, out var count) ? count : 0
            })
            .ToList();
    }

    public IList<DataCollectorPerformance> GetDataCollectorPerformance(
        IEnumerable<DataCollectorWithRawReportData> dataCollectorsWithReportsData,
        DateTime currentDate,
        List<EpiDate> epiDateRange,
        DayOfWeek epiWeekStartDay) =>
        dataCollectorsWithReportsData
            .Select(dc =>
            {
                var datesNotDeployed = dc.DatesNotDeployed.ToList();

                var reportsGroupedByWeek = dc.ReportsInTimeRange
                    .GroupBy(r => r.EpiDate)
                    .ToDictionary(g => g.Key);

                var lastReport = dc.ReportsInTimeRange
                    .OrderByDescending(r => r.ReceivedAt)
                    .FirstOrDefault();

                return new DataCollectorPerformance
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.VillageName,
                    DistrictName = dc.DistrictName,
                    RegionName = dc.RegionName,
                    DaysSinceLastReport = lastReport is not null
                        ? (int)(currentDate - lastReport.ReceivedAt).TotalDays
                        : null,
                    PerformanceInEpiWeeks = epiDateRange
                        .OrderByDescending(e => e.EpiYear)
                        .ThenByDescending(e => e.EpiWeek)
                        .Select(epiDate => new PerformanceInEpiWeek
                        {
                            EpiWeek = epiDate.EpiWeek,
                            EpiYear = epiDate.EpiYear,
                            ReportingStatus = DataCollectorExistedInWeek(epiDate, dc.CreatedAt, epiWeekStartDay)
                                && DataCollectorWasDeployedInWeek(epiDate, datesNotDeployed, epiWeekStartDay)
                                    ? GetDataCollectorStatus(
                                        reportsGroupedByWeek.TryGetValue(epiDate, out var g) ? g : null)
                                    : null
                        })
                        .ToList()
                };
            })
            .ToList();

    private int TotalDataCollectorsDeployedInWeek(
        EpiDate epiDate,
        DayOfWeek epiWeekStartDay,
        IEnumerable<DataCollectorWithRawReportData> dataCollectors) =>
        dataCollectors.Count(dc =>
            DataCollectorExistedInWeek(epiDate, dc.CreatedAt, epiWeekStartDay)
            && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList(), epiWeekStartDay));

    private static ReportingStatus GetDataCollectorStatus(IGrouping<EpiDate, RawReportData>? grouping) =>
        grouping is not null && grouping.Any()
            ? grouping.All(x => x.IsValid)
                ? ReportingStatus.ReportingCorrectly
                : ReportingStatus.ReportingWithErrors
            : ReportingStatus.NotReporting;

    private bool DataCollectorExistedInWeek(EpiDate date, DateTime dataCollectorCreated, DayOfWeek epiWeekStartDay)
    {
        var createdEpiDate = _dateTimeProvider.GetEpiDate(dataCollectorCreated, epiWeekStartDay);
        return createdEpiDate.EpiYear < date.EpiYear
            || (createdEpiDate.EpiYear == date.EpiYear && createdEpiDate.EpiWeek <= date.EpiWeek);
    }

    private bool DataCollectorWasDeployedInWeek(
        EpiDate date,
        List<DataCollectorNotDeployed> datesNotDeployed,
        DayOfWeek epiWeekStartDay)
    {
        if (!datesNotDeployed.Any())
        {
            return true;
        }

        var firstDayOfEpiWeek = _dateTimeProvider.GetFirstDateOfEpiWeek(date.EpiYear, date.EpiWeek, epiWeekStartDay);
        var lastDayOfEpiWeek = firstDayOfEpiWeek.AddDays(7);

        return !datesNotDeployed.Any(d =>
            d.StartDate < lastDayOfEpiWeek
            && (!d.EndDate.HasValue || d.EndDate.Value > firstDayOfEpiWeek));
    }
}