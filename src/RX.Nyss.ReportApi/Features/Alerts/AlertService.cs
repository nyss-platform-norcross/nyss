using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IAlertService
    {
        Task<Alert> CalculateAlerts(Report report);
        void SendNotificationsForNewAlert(Alert alert);
    }

    public class AlertService: IAlertService
    {
        private readonly INyssContext _nyssContext;

        public AlertService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<Alert> CalculateAlerts(Report report)
        {
            if (report.ReportType != ReportType.Single)
            {
                return null;
            }

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Where(phr => phr == report.ProjectHealthRisk)
                .Include(phr => phr.AlertRule)
                .SingleOrDefaultAsync();

            if (projectHealthRisk?.AlertRule == null)
            {
                return null;
            }

            var reportsInRange = await FindReportsSatysfyingRangeAndTimeRequirements(report, projectHealthRisk);
            await ResolveReportLabels(report, reportsInRange);
            await _nyssContext.SaveChangesAsync();

            var triggeredAlert = await HandleAlerts(report);
            await _nyssContext.SaveChangesAsync();
            return triggeredAlert;
        }

        private async Task<Alert> HandleAlerts(Report report)
        {
            var reportGroupLabel = report.ReportGroupLabel;
            var projectHealthRisk = report.ProjectHealthRisk;

            //var existingActiveAlertForLabel = await _nyssContext.Alerts
            //    .Where(a => a.Status == AlertStatus.Pending || a.Status == AlertStatus.Escalated)
            //    .FirstOrDefaultAsync(a => a.AlertReports.Any(ar => ar.Report.ReportGroupLabel == reportGroupLabel));

            var existingActiveAlertForLabel = await _nyssContext.Reports
                .Where(r => r.ReportGroupLabel == reportGroupLabel && r != report)
                .SelectMany(r => r.ReportAlerts)
                .Select(ra => ra.Alert)
                .OrderByDescending(a => a.Status == AlertStatus.Escalated)
                .FirstOrDefaultAsync(r => r.Status == AlertStatus.Pending || r.Status == AlertStatus.Escalated);


            if (existingActiveAlertForLabel != null)
            {
                var reportsInLabelNotInAnyAlert = await _nyssContext.Reports
                    .Where(r => r.ReportGroupLabel == reportGroupLabel)
                    .Where(r => !r.ReportAlerts.Any())
                    .ToListAsync();

                await AddReportsToAlert(existingActiveAlertForLabel, reportsInLabelNotInAnyAlert);
            }
            else
            {
                var reportsWithLabel = await _nyssContext.Reports
                    .Where(r => r.ReportGroupLabel == reportGroupLabel)
                    .ToListAsync();

                if (reportsWithLabel.Count >= projectHealthRisk.AlertRule.CountThreshold)
                {
                    var alert = await CreateNewAlert(projectHealthRisk);
                    await AddReportsToAlert(alert, reportsWithLabel);
                    return alert;
                }
            }

            return null;
        }

        private async Task<Alert> CreateNewAlert(ProjectHealthRisk projectHealthRisk)
        {
            var newAlert = new Alert
            {
                ProjectHealthRisk = projectHealthRisk,
                CreatedAt = DateTime.UtcNow,
                Status = AlertStatus.Pending
            };
            await _nyssContext.Alerts.AddAsync(newAlert);
            return newAlert;
        }

        private Task AddReportsToAlert(Alert alert, IEnumerable<Report> reports)
        {
            var alertReports = reports.Select(r => new AlertReport { Report = r, Alert = alert });
            return _nyssContext.AlertReports.AddRangeAsync(alertReports);
        }

        private async Task ResolveReportLabels(Report consideredReport, IEnumerable<Report> reportsInRange)
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

        private Task<List<Report>> FindReportsSatysfyingRangeAndTimeRequirements(Report report, ProjectHealthRisk projectHealthRisk)
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

            return reportsQuery.ToListAsync();
        }

        public void SendNotificationsForNewAlert(Alert alert)
        { }
    }
}
