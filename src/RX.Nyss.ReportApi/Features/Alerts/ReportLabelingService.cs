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
        Task ResolveReportLabels(Report consideredReport, IEnumerable<Report> reportsInRange);
        Task<List<Guid>> RedoLabelingForReportGroupMembers(Report report, double distanceThreshold);
    }


    public class ReportLabelingService: IReportLabelingService
    {
        private readonly INyssContext _nyssContext;

        public ReportLabelingService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }


        public async Task ResolveReportLabels(Report consideredReport, IEnumerable<Report> reportsInRange)
        {
            var reportLabelsExistingInRange = reportsInRange.Select(r => r.ReportGroupLabel).Distinct();

            var labelForNewConnectedArea = reportLabelsExistingInRange.Any()
                ? reportLabelsExistingInRange.FirstOrDefault()
                : Guid.NewGuid();

            if (reportLabelsExistingInRange.Count() > 1)
            {
                var labelsPlaceholders = Enumerable.Range(1, reportLabelsExistingInRange.Count()).Select(x => $"{{{x}}}");
                var labelsPlaceholderTemplate = string.Join(", ", labelsPlaceholders);

                var commandTemplate = $"UPDATE nyss.Reports SET ReportGroupLabel={{0}} WHERE ReportGroupLabel IN ({labelsPlaceholderTemplate})";
                var parameters = reportLabelsExistingInRange.Cast<object>().ToList();
                parameters.Insert(0, labelForNewConnectedArea);
                var labelUpdateCommand = FormattableStringFactory.Create(commandTemplate, parameters.ToArray());

                await _nyssContext.ExecuteSqlInterpolatedAsync(labelUpdateCommand);
            }

            consideredReport.ReportGroupLabel = labelForNewConnectedArea;
        }

        public async Task<List<Guid>> RedoLabelingForReportGroupMembers(Report report, double distanceThreshold)
        {
            var reportsInLabelGroup = await _nyssContext.Reports
                .Include(r => r.ReportAlerts)
                .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
                .Where(r => r.ReportGroupLabel == report.ReportGroupLabel)
                .AsNoTracking()
                .ToListAsync();

            var reportPoints = reportsInLabelGroup
                .Select(r => new ReportPoint
                {
                    ReportId = r.Id,
                    Label = null,
                    Latitude = r.Location.Y,
                    Longitude = r.Location.X
                });

            var labeledReportPoints = new List<ReportPoint>();

            foreach (var reportPoint in reportPoints)
            {
                var pointsInRange = labeledReportPoints
                    .Where(p => GetPointsAreInRange(reportPoint, p, distanceThreshold))
                    .ToList();

                var labelsInRange = pointsInRange.Select(r => r.Label).Distinct();

                var labelForNewConnectedArea = labelsInRange.Any()
                    ? labelsInRange.FirstOrDefault()
                    : Guid.NewGuid();

                if (labelsInRange.Count() > 1)
                {
                    labeledReportPoints.Where(p => labelsInRange.Contains(p.Label))
                        .ToList()
                        .ForEach(p => p.Label = labelForNewConnectedArea);
                }

                reportPoint.Label = labelForNewConnectedArea;
                labeledReportPoints.Add(reportPoint);
            }

            var labelGroups = labeledReportPoints.GroupBy(p => p.Label);

            foreach (var labelGroup in labelGroups)
            {
                var reportIdsInGroup = labelGroup.Select(g => g.ReportId);

                var reportIdsPlaceholders = Enumerable.Range(1, reportIdsInGroup.Count()).Select(x => $"{{{x}}}");
                var reportIdsPlaceholdersTemplate = string.Join(", ", reportIdsPlaceholders);

                var commandTemplate = $"UPDATE nyss.Reports SET ReportGroupLabel={{0}} WHERE Id IN ({reportIdsPlaceholdersTemplate})";
                var parameters = reportIdsInGroup.Cast<object>().ToList();
                parameters.Insert(0, labelGroup.Key);
                var labelUpdateCommand = FormattableStringFactory.Create(commandTemplate, parameters.ToArray());

                await _nyssContext.ExecuteSqlInterpolatedAsync(labelUpdateCommand);
            }

            return labelGroups.Select(g => g.Key.Value).ToList();

            bool GetPointsAreInRange(ReportPoint firstPoint, ReportPoint secondPoint, double distanceThreshold)
            {
                var firstCoordinate = new GeoCoordinate(firstPoint.Latitude, firstPoint.Longitude);
                var secondCoordinate = new GeoCoordinate(secondPoint.Latitude, secondPoint.Longitude);
                var distance = firstCoordinate.GetDistanceTo(secondCoordinate);
                return distance <= distanceThreshold;
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
