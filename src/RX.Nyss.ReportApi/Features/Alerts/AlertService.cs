using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Common.Contracts;
using RX.Nyss.ReportApi.Features.Common.Extensions;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IAlertService
    {
        Task<AlertData> ReportAdded(Report report);
        Task<bool> RecalculateAlertForReport(int reportId);
    }

    public class AlertService : IAlertService
    {
        private readonly INyssContext _nyssContext;
        private readonly IReportLabelingService _reportLabelingService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IAlertNotificationService _alertNotificationService;

        public AlertService(
            INyssContext nyssContext,
            IReportLabelingService reportLabelingService,
            ILoggerAdapter loggerAdapter,
            IAlertNotificationService alertNotificationService)
        {
            _nyssContext = nyssContext;
            _reportLabelingService = reportLabelingService;
            _loggerAdapter = loggerAdapter;
            _alertNotificationService = alertNotificationService;
        }

        public async Task<AlertData> ReportAdded(Report report)
        {
            if (report.DataCollector == null)
            {
                return new AlertData
                {
                    Alert = null,
                    IsExistingAlert = false
                };
            }

            var dataCollectorIsNotHuman = report.DataCollector.DataCollectorType != DataCollectorType.Human;
            var reportTypeIsAggregateOrDcp = report.ReportType == ReportType.Aggregate || report.ReportType == ReportType.DataCollectionPoint;
            var healthRiskTypeIsActivity = report.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Activity;

            if (report.IsTraining || dataCollectorIsNotHuman || reportTypeIsAggregateOrDcp || healthRiskTypeIsActivity || report.Location == null)
            {
                return new AlertData
                {
                    Alert = null,
                    IsExistingAlert = false
                };
            }

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Where(phr => phr == report.ProjectHealthRisk)
                .Include(phr => phr.AlertRule)
                .Include(phr => phr.HealthRisk)
                .SingleAsync();

            if (projectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Activity)
            {
                return new AlertData
                {
                    Alert = null,
                    IsExistingAlert = false
                };
            }

            await _reportLabelingService.ResolveLabelsOnReportAdded(report, projectHealthRisk);
            await _nyssContext.SaveChangesAsync();

            var triggeredAlertData = await HandleAlerts(report);
            await _nyssContext.SaveChangesAsync();
            return triggeredAlertData;
        }

        public async Task<bool> RecalculateAlertForReport(int reportId)
        {
            try
            {
                var rawReport = await _nyssContext.RawReports
                    .Include(r => r.Report).ThenInclude(r => r.DataCollector)
                    .Include(r => r.Report).ThenInclude(r => r.ProjectHealthRisk.HealthRisk)
                    .Where(r => r.Id == reportId)
                    .SingleOrDefaultAsync();

                if (rawReport?.Report == null)
                {
                    _loggerAdapter.Warn($"The report with id {reportId} does not exist.");
                    return false;
                }

                var alertData = await ReportAdded(rawReport.Report);

                if (alertData.Alert == null)
                {
                    return true;
                }

                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Where(gs => gs.ApiKey == rawReport.ApiKey)
                    .SingleOrDefaultAsync();

                if (gatewaySetting == null)
                {
                    _loggerAdapter.ErrorFormat("Could not send notification to Supervisor. GatewaySetting with API key: {0} not found.", rawReport.ApiKey);
                    return true;
                }

                if (alertData.IsExistingAlert)
                {
                    await _alertNotificationService.SendNotificationsForSupervisorsAddedToExistingAlert(alertData.Alert, alertData.SupervisorsAddedToExistingAlert, gatewaySetting);
                }
                else
                {
                    await _alertNotificationService.SendNotificationsForNewAlert(alertData.Alert, gatewaySetting);
                }

                return true;
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"Could not recalculate alert for report with id: '{reportId}'.");
                return false;
            }
        }

        private async Task<AlertData> HandleAlerts(Report report)
        {
            var reportGroupLabel = report.ReportGroupLabel;
            var projectHealthRisk = report.ProjectHealthRisk;

            var (existingAlert, addedSupervisors) = await IncludeAllReportsWithLabelInExistingAlert(reportGroupLabel);

            if (existingAlert != null)
            {
                return new AlertData
                {
                    Alert = existingAlert,
                    SupervisorsAddedToExistingAlert = addedSupervisors,
                    IsExistingAlert = true
                };
            }

            var reportsWithLabel = await _nyssContext.Reports
                .OnlyCorrectReports()
                .OnlyRealReports()
                .OnlyReportsThatCanTriggerNewAlert()
                .FilterByGroupLabel(reportGroupLabel)
                .FilterByReportStatus(StatusConstants.ReportStatusesConsideredForAlertProcessing)
                .ToListAsync();

            if (projectHealthRisk.AlertRule.CountThreshold == 0 || reportsWithLabel.Count < projectHealthRisk.AlertRule.CountThreshold)
            {
                return new AlertData
                {
                    Alert = null,
                    IsExistingAlert = false
                };
            }

            var alert = await CreateNewAlert(projectHealthRisk);
            await AddReportsToAlert(alert, reportsWithLabel);
            return new AlertData
            {
                Alert = alert,
                IsExistingAlert = false
            };
        }

        private async Task<(Alert, List<SupervisorSmsRecipient>)> IncludeAllReportsWithLabelInExistingAlert(Guid reportGroupLabel, int? alertIdToIgnore = null)
        {
            var addedSupervisors = new List<SupervisorSmsRecipient>();
            var existingActiveAlertForLabel = await _nyssContext.Reports
                .OnlyCorrectReports()
                .OnlyRealReports()
                .FilterByReportStatus(StatusConstants.ReportStatusesConsideredForAlertProcessing)
                .FilterByGroupLabel(reportGroupLabel)
                .SelectMany(r => r.ReportAlerts)
                .Where(ar => !alertIdToIgnore.HasValue || ar.AlertId != alertIdToIgnore.Value)
                .Select(ra => ra.Alert)
                .FirstOrDefaultAsync(a => a.Status == AlertStatus.Pending);

            if (existingActiveAlertForLabel != null)
            {
                var reportsInLabelWithNoActiveAlert = await _nyssContext.Reports
                    .OnlyCorrectReports()
                    .OnlyRealReports()
                    .FilterByReportStatus(StatusConstants.ReportStatusesConsideredForAlertProcessing)
                    .FilterByGroupLabel(reportGroupLabel)
                    .NotInExistingAlert(alertIdToIgnore)
                    .Select(r => new
                    {
                        Report = r,
                        DataCollector = r.DataCollector,
                        Supervisor = r.DataCollector.Supervisor,
                        SupervisorModem = r.DataCollector.Supervisor.Modem,
                        HeadSupervisor = r.DataCollector.HeadSupervisor,
                        HeadSupervisorModem = r.DataCollector.HeadSupervisor.Modem
                    })
                    .ToListAsync();

                var supervisorsConnectedToExistingAlert = await _alertNotificationService.GetSupervisorsConnectedToExistingAlert(existingActiveAlertForLabel.Id);
                var headSupervisorsConnectedToExistingAlert = await _alertNotificationService.GetHeadSupervisorsConnectedToExistingAlert(existingActiveAlertForLabel.Id);
                var supervisorsAddedToAlert = reportsInLabelWithNoActiveAlert
                    .Where(r => r.Supervisor != null)
                    .Select(r => new SupervisorSmsRecipient
                    {
                        Name = r.Supervisor.Name,
                        PhoneNumber = r.Supervisor.PhoneNumber,
                        UserId = r.Supervisor.Id,
                        Modem = r.SupervisorModem?.ModemId
                    })
                    .Distinct()
                    .Where(sup => supervisorsConnectedToExistingAlert.All(s => s.UserId != sup.UserId));

                var headSupervisorsAddedToAlert = reportsInLabelWithNoActiveAlert
                    .Where(r => r.HeadSupervisor != null)
                    .Select(r => new SupervisorSmsRecipient
                    {
                        Name = r.HeadSupervisor.Name,
                        PhoneNumber = r.HeadSupervisor.PhoneNumber,
                        UserId = r.HeadSupervisor.Id,
                        Modem = r.HeadSupervisorModem?.ModemId
                    })
                    .Distinct()
                    .Where(headSup => headSupervisorsConnectedToExistingAlert.All(s => s.UserId != headSup.UserId));

                addedSupervisors = supervisorsAddedToAlert
                    .Concat(headSupervisorsAddedToAlert)
                    .ToList();

                var reportsInNoAlert = reportsInLabelWithNoActiveAlert
                    .Select(r => r.Report)
                    .ToList();

                await AddReportsToAlert(existingActiveAlertForLabel, reportsInNoAlert);
            }

            return (existingActiveAlertForLabel, addedSupervisors);
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

        private Task AddReportsToAlert(Alert alert, List<Report> reports)
        {
            reports.Where(r => r.Status == ReportStatus.New).ToList()
                .ForEach(r => r.Status = ReportStatus.Pending);

            var alertReports = reports.Select(r => new AlertReport
            {
                Report = r,
                Alert = alert
            });
            return _nyssContext.AlertReports.AddRangeAsync(alertReports);
        }
    }
}
