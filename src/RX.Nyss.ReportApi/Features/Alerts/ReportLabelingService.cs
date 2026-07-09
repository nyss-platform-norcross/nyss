using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Features.Alerts;

public interface IReportLabelingService
{
    /// <Summary>
    ///  <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with
    /// care.
    /// </Summary>
    Task ResolveLabelsOnReportAdded(Report addedReport, ProjectHealthRisk projectHealthRisk);

    Task<IEnumerable<(int ReportId, Guid Label)>> CalculateNewLabelsInLabelGroup(Guid label, double distanceThreshold, int? reportIdToIgnore);

    Task<List<Report>> FindReportsSatisfyingRangeAndTimeRequirements(Report report, ProjectHealthRisk projectHealthRisk);

    /// <Summary>
    ///  <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with
    /// care.
    /// </Summary>
    Task UpdateLabelsInDatabaseDirect(IEnumerable<(int ReportId, Guid Label)> pointsWithLabels);
}

public class ReportLabelingService : IReportLabelingService
{
    private readonly INyssContext _nyssContext;

    public ReportLabelingService(INyssContext nyssContext)
    {
        _nyssContext = nyssContext;
    }

    /// <Summary>
    ///  <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with
    /// care.
    /// </Summary>
    public async Task ResolveLabelsOnReportAdded(Report addedReport, ProjectHealthRisk projectHealthRisk)
    {
        if (addedReport.IsTraining)
        {
            return;
        }

        var reportsInRange = await FindReportsSatisfyingRangeAndTimeRequirements(addedReport, projectHealthRisk);

        var reportLabelsExistingInRange = reportsInRange.Select(r => r.ReportGroupLabel)
            .Distinct().ToList();

        var labelForNewConnectedArea = reportLabelsExistingInRange.Any()
            ? reportLabelsExistingInRange.FirstOrDefault()
            : Guid.NewGuid();

        if (reportLabelsExistingInRange.Count() >= 2)
        {
            var labelsPlaceholderTemplate = CreateSequenceOfParameterPlaceholders(1, reportLabelsExistingInRange.Count());

            var commandTemplate = $"UPDATE nyss.Reports SET ReportGroupLabel={{0}} WHERE ReportGroupLabel IN ({labelsPlaceholderTemplate})";
            await UpdateValueWhereParametersInRangeDirect(commandTemplate, labelForNewConnectedArea, reportLabelsExistingInRange);
        }

        addedReport.ReportGroupLabel = labelForNewConnectedArea;
    }

    public Task<List<Report>> FindReportsSatisfyingRangeAndTimeRequirements(Report report, ProjectHealthRisk projectHealthRisk)
    {
        var searchRadiusInMeters = projectHealthRisk.AlertRule.KilometersThreshold * 1000 * 2;

        var reportsQuery = _nyssContext.Reports
            .Where(r => r.ProjectHealthRisk == projectHealthRisk)
            .Where(r => StatusConstants.ReportStatusesConsideredForAlertProcessing.Contains(r.Status))
            .Where(r => !r.IsTraining)
            .Where(r => !r.ReportAlerts.Any(ra => ra.Alert.Status == AlertStatus.Closed))
            .Where(r => r.Id != report.Id)
            .Where(r => r.ReportGroupLabel != default)
            .Where(r => r.Location.Distance(report.Location) < searchRadiusInMeters);

        if (projectHealthRisk.AlertRule.DaysThreshold.HasValue)
        {
            var earliestReceivedTime = report.ReceivedAt.AddDays(-projectHealthRisk.AlertRule.DaysThreshold.Value);
            var latestReceivedTime = report.ReceivedAt.AddDays(projectHealthRisk.AlertRule.DaysThreshold.Value);
            reportsQuery = reportsQuery.Where(r => r.ReceivedAt > earliestReceivedTime && r.ReceivedAt < latestReceivedTime);
        }

        return reportsQuery.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<(int ReportId, Guid Label)>> CalculateNewLabelsInLabelGroup(Guid label, double distanceThreshold, int? reportIdToIgnore)
    {
        var reportsWithSelectedLabel = await _nyssContext.Reports
            .Include(r => r.ReportAlerts)
            .Where(r => !r.ReportAlerts.Any(ra => ra.Alert.Status == AlertStatus.Closed))
            .Where(r => StatusConstants.ReportStatusesConsideredForAlertProcessing.Contains(r.Status))
            .Where(r => !r.IsTraining)
            .Where(r => !reportIdToIgnore.HasValue || r.Id != reportIdToIgnore.Value)
            .Where(r => r.ReportGroupLabel == label)
            .Select(r => new ReportPoint
            {
                ReportId = r.Id,
                Label = null,
                Latitude = r.Location.Y,
                Longitude = r.Location.X
            })
            .ToListAsync();

        var updatedReportPoints = ReassignLabelsByAddingPointsOneByOne(reportsWithSelectedLabel, distanceThreshold);

        return updatedReportPoints.Select(p => (p.ReportId, Label: p.Label.Value));
    }

    /// <Summary>
    ///  <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with
    /// care.
    /// </Summary>
    public async Task UpdateLabelsInDatabaseDirect(IEnumerable<(int ReportId, Guid Label)> pointsWithLabels)
    {
        var labelGroups = pointsWithLabels.GroupBy(p => p.Label);
        foreach (var labelGroup in labelGroups)
        {
            var reportIdsInGroup = labelGroup.Select(g => g.ReportId);

            var idsPlaceholderTemplate = CreateSequenceOfParameterPlaceholders(1, reportIdsInGroup.Count());
            var commandTemplate = $"UPDATE nyss.Reports SET ReportGroupLabel={{0}} WHERE Id IN ({idsPlaceholderTemplate})";
            await UpdateValueWhereParametersInRangeDirect(commandTemplate, labelGroup.Key, reportIdsInGroup);
        }
    }

    private Task UpdateValueWhereParametersInRangeDirect<TUpdateValue, TParameter>(string commandTemplate, TUpdateValue updateValue, IEnumerable<TParameter> rangeOfParameters)
    {
        var parameters = rangeOfParameters.Cast<object>().ToList();
        parameters.Insert(0, updateValue);
        var labelUpdateCommand = FormattableStringFactory.Create(commandTemplate, parameters.ToArray());
        return _nyssContext.ExecuteSqlInterpolatedAsync(labelUpdateCommand);
    }

    private static string CreateSequenceOfParameterPlaceholders(int startIndex, int number)
    {
        var labelsPlaceholders = Enumerable.Range(startIndex, number).Select(x => $"{{{x}}}");
        var labelsPlaceholderTemplate = string.Join(", ", labelsPlaceholders);
        return labelsPlaceholderTemplate;
    }

    private List<ReportPoint> ReassignLabelsByAddingPointsOneByOne(List<ReportPoint> reportsWithNoLabel, double distanceThreshold)
    {
        var pointsWithAssignedLabel = new List<ReportPoint>();
        foreach (var reportPoint in reportsWithNoLabel)
        {
            var pointsInRange = pointsWithAssignedLabel
                .Where(p => reportPoint.GetPointsAreInRange(p, distanceThreshold))
                .ToList();

            var labelsInRange = pointsInRange.Select(r => r.Label).Distinct().ToList();

            var labelForNewConnectedArea = labelsInRange.Any()
                ? labelsInRange.First()
                : Guid.NewGuid();

            if (labelsInRange.Count >= 2)
            {
                pointsWithAssignedLabel.Where(p => labelsInRange.Contains(p.Label))
                    .ToList()
                    .ForEach(p => p.Label = labelForNewConnectedArea);
            }

            reportPoint.Label = labelForNewConnectedArea;
            pointsWithAssignedLabel.Add(reportPoint);
        }

        return pointsWithAssignedLabel;
    }
}
