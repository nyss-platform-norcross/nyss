using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IAlertService
    {
        Task<Alert> ReportAdded(Report report);
        Task ReportDismissed(int reportId);
        void SendNotificationsForNewAlert(Alert alert);
    }

    public class AlertService: IAlertService
    {
        private readonly INyssContext _nyssContext;
        private readonly IReportLabelingService _reportLabelingService;
        private readonly ILoggerAdapter _loggerAdapter;

        public AlertService(INyssContext nyssContext, IReportLabelingService reportLabelingService, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _reportLabelingService = reportLabelingService;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Alert> ReportAdded(Report report)
        {
            if (report.ReportType != ReportType.Single)
            {
                return null;
            }

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Where(phr => phr == report.ProjectHealthRisk)
                .Include(phr => phr.AlertRule)
                .SingleAsync();

            await _reportLabelingService.ResolveLabelsOnReportAdded(report, projectHealthRisk);
            await _nyssContext.SaveChangesAsync();

            var triggeredAlert = await HandleAlerts(report);
            await _nyssContext.SaveChangesAsync();
            return triggeredAlert;
        }

        private async Task<Alert> HandleAlerts(Report report)
        {
            var reportGroupLabel = report.ReportGroupLabel;
            var projectHealthRisk = report.ProjectHealthRisk;

            var existingAlert = await IncludeAllReportsWithLabelInExistingAlert(reportGroupLabel);

            if (existingAlert == null)
            {
                var reportsWithLabel = await _nyssContext.Reports
                    .Where(r => r.ReportGroupLabel == reportGroupLabel)
                    .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
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

        private async Task<Alert> IncludeAllReportsWithLabelInExistingAlert(Guid reportGroupLabel)
        {
            var existingActiveAlertForLabel = await _nyssContext.Reports
                .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
                .Where(r => r.ReportGroupLabel == reportGroupLabel)
                .SelectMany(r => r.ReportAlerts)
                .Select(ra => ra.Alert)
                .OrderByDescending(a => a.Status == AlertStatus.Escalated)
                .FirstOrDefaultAsync(a => a.Status == AlertStatus.Pending || a.Status == AlertStatus.Escalated);

            if (existingActiveAlertForLabel != null)
            {
                var reportsInLabelWithNoActiveAlert = await _nyssContext.Reports
                    .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
                    .Where(r => r.ReportGroupLabel == reportGroupLabel)
                    .Where(r => !r.ReportAlerts.Any(ra => ra.Alert.Status == AlertStatus.Pending || ra.Alert.Status == AlertStatus.Escalated))
                    .ToListAsync();

                await AddReportsToAlert(existingActiveAlertForLabel, reportsInLabelWithNoActiveAlert);
            }

            return existingActiveAlertForLabel;
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
            reports.Where(r => r.Status == ReportStatus.New).ToList()
                .ForEach(r => r.Status = ReportStatus.Pending);
            return _nyssContext.AlertReports.AddRangeAsync(alertReports);
        }

        public async Task ReportDismissed(int reportId)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var inspectedAlert = await _nyssContext.AlertReports
                .Where(ar => ar.ReportId == reportId)
                .Where(ar => ar.Alert.Status == AlertStatus.Pending)
                .Select(ar => ar.Alert)
                .SingleOrDefaultAsync();

            if (inspectedAlert == null)
            {
                _loggerAdapter.Warn($"The alert for report with id {reportId} does not exist or has status different than pending.");
                return;
            }

            var report = await _nyssContext.Reports
                .Include(r => r.ProjectHealthRisk)
                .ThenInclude(phr => phr.AlertRule)
                .Where(r => r.Id == reportId)
                .SingleAsync();

            if (report == null)
            {
                _loggerAdapter.Warn($"The report with id {reportId} does not exist.");
                return;
            }

            var alertRule = report.ProjectHealthRisk.AlertRule;
            if (alertRule.CountThreshold == 1)
            {
                report.Status = ReportStatus.Rejected;
                inspectedAlert.Status = AlertStatus.Rejected;
                await _nyssContext.SaveChangesAsync();
                return;
            }

            report.Status = ReportStatus.Rejected;
            await _nyssContext.SaveChangesAsync();

            var pointsWithLabels = await _reportLabelingService.CalculateNewLabelsInLabelGroup(report.ReportGroupLabel, alertRule.KilometersThreshold.Value * 1000 * 2);
            await _reportLabelingService.UpdateLabelsInDatabaseDirect(pointsWithLabels);

            var alertReportsWitUpdatedLabels = await _nyssContext.AlertReports
                .Where(ar => ar.AlertId == inspectedAlert.Id)
                .Where(ar => ar.ReportId != reportId)
                .Select(ar => ar.Report)
                .ToListAsync();

            var updatedReportsGroupedByLabel = alertReportsWitUpdatedLabels.GroupBy(r => r.ReportGroupLabel);

            var noGroupWithinAlertSatisfiesCountThreshold = !updatedReportsGroupedByLabel.Any(g => g.Count() >= alertRule.CountThreshold);
            if (noGroupWithinAlertSatisfiesCountThreshold)
            {
                inspectedAlert.Status = AlertStatus.Rejected;
                await _nyssContext.SaveChangesAsync();
                foreach (var groupNotSatisfyingThreshold in updatedReportsGroupedByLabel)
                {
                    var reportGroupLabel = groupNotSatisfyingThreshold.Key;
                    await IncludeAllReportsWithLabelInExistingAlert(reportGroupLabel);
                }
            }

            await _nyssContext.SaveChangesAsync();

            transactionScope.Complete();
        }

        public void SendNotificationsForNewAlert(Alert alert)
        { }
    }
}
