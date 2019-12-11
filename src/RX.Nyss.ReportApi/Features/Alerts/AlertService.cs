using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IAlertService
    {
        Task<Alert> ReportAdded(Report report);
        Task ReportRejected(int reportId);
        void SendNotificationsForNewAlert(Alert alert);
    }

    public class AlertService: IAlertService
    {
        private readonly INyssContext _nyssContext;
        private readonly IReportLabelingService _reportLabelingService;

        public AlertService(INyssContext nyssContext, IReportLabelingService reportLabelingService)
        {
            _nyssContext = nyssContext;
            _reportLabelingService = reportLabelingService;
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
                .SingleOrDefaultAsync();

            if (projectHealthRisk?.AlertRule == null)
            {
                return null;
            }

            var reportsInRange = await FindReportsSatisfyingRangeAndTimeRequirements(report, projectHealthRisk);
            await _reportLabelingService.ResolveReportLabels(report, reportsInRange);
            await _nyssContext.SaveChangesAsync();

            var triggeredAlert = await HandleAlerts(report);
            await _nyssContext.SaveChangesAsync();
            return triggeredAlert;
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

            return reportsQuery.ToListAsync();
        }

        

        private async Task<Alert> HandleAlerts(Report report)
        {
            var reportGroupLabel = report.ReportGroupLabel;
            var projectHealthRisk = report.ProjectHealthRisk;

            var existingAlert = await IncludeAllReportsInExistingAlert(reportGroupLabel);

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

        private async Task<Alert> IncludeAllReportsInExistingAlert(Guid reportGroupLabel)
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
                var reportsInLabelNotInAnyActiveAlert = await _nyssContext.Reports
                    .Where(r => r.ReportGroupLabel == reportGroupLabel)
                    .Where(r => r.Status != ReportStatus.Rejected && r.Status != ReportStatus.Removed)
                    .Where(r => !r.ReportAlerts.Any(ra => ra.Alert.Status == AlertStatus.Pending || ra.Alert.Status == AlertStatus.Escalated))
                    .ToListAsync();

                await AddReportsToAlert(existingActiveAlertForLabel, reportsInLabelNotInAnyActiveAlert);
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
            reports.ToList().ForEach(r => r.Status = ReportStatus.Pending);
            return _nyssContext.AlertReports.AddRangeAsync(alertReports);
        }

        public async Task ReportRejected(int reportId)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var inspectedAlert = await _nyssContext.AlertReports
                .Where(ar => ar.ReportId == reportId)
                .Where(ar => ar.Alert.Status == AlertStatus.Pending || ar.Alert.Status == AlertStatus.Escalated)
                .Select(ar => ar.Alert)
                .SingleOrDefaultAsync();

            if (inspectedAlert == null)
            {
                return;
            }

            var report = await _nyssContext.Reports
                .Include(r => r.ProjectHealthRisk)
                .ThenInclude(phr => phr.AlertRule)
                .Where(r => r.Id == reportId)
                .SingleOrDefaultAsync();

            if (report == null)
            {
                //todo: throw exception
                return;
            }

            var alertRule = report.ProjectHealthRisk.AlertRule;
            if (alertRule.CountThreshold == 1)
            {
                return;
            }

            report.Status = ReportStatus.Rejected;
            await _nyssContext.SaveChangesAsync();

            await _reportLabelingService.RedoLabelingForReportGroupMembers(report, alertRule.KilometersThreshold.Value * 1000 * 2);

            var reports = await _nyssContext.AlertReports
                .Where(ar => ar.AlertId == inspectedAlert.Id)
                .Where(ar => ar.ReportId != reportId)
                .Select(ar => ar.Report)
                .ToListAsync();

            var reportsGroupedByLabel = reports.GroupBy(r => r.ReportGroupLabel);

            var reportHasGroupsThatSatisfyCountThreshold = reportsGroupedByLabel.Any(g => g.Count() > alertRule.CountThreshold);
            if (!reportHasGroupsThatSatisfyCountThreshold)
            {
                inspectedAlert.Status = AlertStatus.Rejected;
                await _nyssContext.SaveChangesAsync();
                foreach (var groupNotSatisfyingThreshold in reportsGroupedByLabel)
                {
                    var reportGroupLabel = groupNotSatisfyingThreshold.Key;

                    await IncludeAllReportsInExistingAlert(reportGroupLabel);
                }
            }

            await _nyssContext.SaveChangesAsync();

            transactionScope.Complete();
        }

        public void SendNotificationsForNewAlert(Alert alert)
        { }
    }
}
