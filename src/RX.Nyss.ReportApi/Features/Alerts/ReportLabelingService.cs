using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IReportLabelingService
    {
        ///<Summary>
        /// <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with care.
        /// </Summary>
        Task ResolveLabelsOnReportAdded(Report addedReport, ProjectHealthRisk projectHealthRisk);
        Task<IEnumerable<(int ReportId, Guid Label)>> CalculateNewLabelsInLabelGroup(Guid label, double distanceThreshold);

        ///<Summary>
        /// <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with care.
        /// </Summary>
        Task UpdateLabelsInDatabaseDirect(IEnumerable<(int ReportId, Guid Label)> pointsWithLabels);
    }

    public class ReportLabelingService: IReportLabelingService
    {
        private readonly INyssContext _nyssContext;

        public ReportLabelingService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        ///<Summary>
        /// <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with care.
        /// </Summary>
        public async Task ResolveLabelsOnReportAdded(Report addedReport, ProjectHealthRisk projectHealthRisk)
        {
            var reportsInRange = await FindReportsSatisfyingRangeAndTimeRequirements(addedReport, projectHealthRisk);

            var reportLabelsExistingInRange = reportsInRange.Select(r => r.ReportGroupLabel).Distinct();

            var labelForNewConnectedArea = reportLabelsExistingInRange.Any()
                ? reportLabelsExistingInRange.FirstOrDefault()
                : Guid.NewGuid();

            if (reportLabelsExistingInRange.Count() > 1)
            {
                var labelsPlaceholderTemplate = CreateSequenceOfParameterPlaceholders(1, reportLabelsExistingInRange.Count());

                var commandTemplate = $"UPDATE nyss.Reports SET ReportGroupLabel={{0}} WHERE ReportGroupLabel IN ({labelsPlaceholderTemplate})";
                await UpdateValueWhereParametersInRangeDirect(commandTemplate, labelForNewConnectedArea, reportLabelsExistingInRange);
            }

            addedReport.ReportGroupLabel = labelForNewConnectedArea;
        }

        private Task<List<Report>> FindReportsSatisfyingRangeAndTimeRequirements(Report report, ProjectHealthRisk projectHealthRisk)
        {
            var searchRadiusInMeters = projectHealthRisk.AlertRule.KilometersThreshold * 1000 * 2;

            var reportsQuery = _nyssContext.Reports
                .Where(r => r.ProjectHealthRisk == projectHealthRisk)
                .Where(r => !r.IsTraining)
                .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
                .Where(r => r.Id != report.Id)
                .Where(r => r.Location.Distance(report.Location) < searchRadiusInMeters);

            if (projectHealthRisk.AlertRule.DaysThreshold.HasValue)
            {
                var utcNow = DateTime.UtcNow;
                var receivalThreshold = utcNow.AddDays(-projectHealthRisk.AlertRule.DaysThreshold.Value);
                reportsQuery = reportsQuery.Where(r => r.ReceivedAt > receivalThreshold);
            }

            return reportsQuery.AsNoTracking().ToListAsync();
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

        public async Task<IEnumerable<(int ReportId, Guid Label)>> CalculateNewLabelsInLabelGroup(Guid label, double distanceThreshold)
        {
            var reportsWithSelectedLabel = await _nyssContext.Reports
                .Include(r => r.ReportAlerts)
                .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
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


            List<ReportPoint> ReassignLabelsByAddingPointsOneByOne(List<ReportPoint> reportsWithNoLabel, double distanceThreshold)
            {
                var pointsWithAssignedLabel = new List<ReportPoint>();
                foreach (var reportPoint in reportsWithNoLabel)
                {
                    var pointsInRange = pointsWithAssignedLabel
                        .Where(p => GetPointsAreInRange(reportPoint, p, distanceThreshold))
                        .ToList();

                    var labelsInRange = pointsInRange.Select(r => r.Label).Distinct();

                    var labelForNewConnectedArea = labelsInRange.Any()
                        ? labelsInRange.FirstOrDefault()
                        : Guid.NewGuid();

                    if (labelsInRange.Count() > 1)
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

            bool GetPointsAreInRange(ReportPoint firstPoint, ReportPoint secondPoint, double distanceThreshold)
            {
                var firstCoordinate = new GeoCoordinate(firstPoint.Latitude, firstPoint.Longitude);
                var secondCoordinate = new GeoCoordinate(secondPoint.Latitude, secondPoint.Longitude);
                var distance = firstCoordinate.GetDistanceTo(secondCoordinate);
                return distance <= distanceThreshold;
            }
        }

        ///<Summary>
        /// <strong>Caution:</strong> does a direct update in the DB that is not tracked by the database context. Handle with care.
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


        private class ReportPoint
        {
            public Guid? Label { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int ReportId { get; set; }
        }
    }
}
