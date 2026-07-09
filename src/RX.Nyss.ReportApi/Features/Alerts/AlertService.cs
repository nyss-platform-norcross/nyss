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

namespace RX.Nyss.ReportApi.Features.Alerts;

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

        var dataCollectorCannotTriggerAlerts =
            report.DataCollector.DataCollectorType != DataCollectorType.Human &&
            report.DataCollector.DataCollectorType != DataCollectorType.CollectionPoint;
        var reportTypeIsAggregate = report.ReportType == ReportType.Aggregate;
        var healthRiskType = await GetHealthRiskType(report.ProjectHealthRisk.Id);
        var healthRiskTypeIsActivity = healthRiskType == HealthRiskType.Activity;

        if (report.IsTraining || dataCollectorCannotTriggerAlerts || reportTypeIsAggregate || healthRiskTypeIsActivity || report.Location == null)
        {
            return new AlertData
            {
                Alert = null,
                IsExistingAlert = false
            };
        }

        var projectHealthRisk = await _nyssContext.ProjectHealthRisks
            .Where(phr => phr.Id == report.ProjectHealthRisk.Id)
            .Include(phr => phr.AlertRule)
            .SingleAsync();

        if (healthRiskType == HealthRiskType.Activity)
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
        _loggerAdapter.Info($"[RecalculateAlert] START reportId={reportId}");
        try
        {
            // Step 1: Load report (avoid HealthRisk include — causes SqlNullValueException when AlertRuleId is null)
            Report report;
            try
            {
                var reportEntityId = await _nyssContext.RawReports
                    .Where(rr => rr.Id == reportId)
                    .Select(rr => rr.ReportId)
                    .SingleOrDefaultAsync();

                if (reportEntityId == null)
                {
                    _loggerAdapter.Warn($"[RecalculateAlert] Step 1 — report with id {reportId} does not exist.");
                    return false;
                }

                report = await _nyssContext.Reports
                    .Include(r => r.RawReport)
                    .Include(r => r.DataCollector)
                    .Include(r => r.ProjectHealthRisk)
                        .ThenInclude(phr => phr.AlertRule)
                    .SingleOrDefaultAsync(r => r.Id == reportEntityId.Value);

                _loggerAdapter.Info($"[RecalculateAlert] Step 1 OK — report found={report != null}, apiKey={report?.RawReport?.ApiKey}");
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"[RecalculateAlert] Step 1 FAILED — EF query crashed for reportId={reportId}.");
                throw;
            }

            if (report == null)
            {
                _loggerAdapter.Warn($"[RecalculateAlert] Step 1 — report with id {reportId} does not exist.");
                return false;
            }

            // Step 2: Log key fields to spot bad data before ReportAdded
            _loggerAdapter.Info($"[RecalculateAlert] Step 2 — DataCollector={report.DataCollector?.Id.ToString() ?? "NULL"}, " +
                                $"ProjectHealthRisk={report.ProjectHealthRisk?.Id.ToString() ?? "NULL"}");

            // Step 3: ReportAdded
            AlertData alertData;
            try
            {
                alertData = await ReportAdded(report);
                _loggerAdapter.Info($"[RecalculateAlert] Step 3 OK — alertData.Alert={alertData?.Alert?.Id.ToString() ?? "NULL"}, IsExistingAlert={alertData?.IsExistingAlert}");
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"[RecalculateAlert] Step 3 FAILED — ReportAdded crashed for reportId={reportId}");
                throw;
            }

            if (alertData.Alert == null)
            {
                _loggerAdapter.Info($"[RecalculateAlert] Step 3 — no alert triggered for reportId={reportId}, returning true");
                return true;
            }

            // Step 4: GatewaySetting lookup
            GatewaySetting gatewaySetting;
            try
            {
                gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Where(gs => gs.ApiKey == report.RawReport.ApiKey)
                    .SingleOrDefaultAsync();
                _loggerAdapter.Info($"[RecalculateAlert] Step 4 OK — gatewaySetting found={gatewaySetting != null}");
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"[RecalculateAlert] Step 4 FAILED — GatewaySettings query crashed for apiKey={report.RawReport.ApiKey}");
                throw;
            }

            if (gatewaySetting == null)
            {
                _loggerAdapter.ErrorFormat("Could not send notification to Supervisor. GatewaySetting with API key: {0} not found.", report.RawReport.ApiKey);
                return true;
            }

            // Step 5: Send notifications
            try
            {
                if (alertData.IsExistingAlert)
                {
                    _loggerAdapter.Info($"[RecalculateAlert] Step 5 — sending notifications for EXISTING alert id={alertData.Alert.Id}, supervisors={alertData.SupervisorsAddedToExistingAlert?.Count ?? 0}");
                    await _alertNotificationService.SendNotificationsForSupervisorsAddedToExistingAlert(alertData.Alert, alertData.SupervisorsAddedToExistingAlert, gatewaySetting);
                }
                else
                {
                    _loggerAdapter.Info($"[RecalculateAlert] Step 5 — sending notifications for NEW alert id={alertData.Alert.Id}");
                    await _alertNotificationService.SendNotificationsForNewAlert(alertData.Alert, gatewaySetting);
                }
                _loggerAdapter.Info($"[RecalculateAlert] Step 5 OK — notifications sent for reportId={reportId}");
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"[RecalculateAlert] Step 5 FAILED — notification sending crashed for alertId={alertData.Alert.Id}, reportId={reportId}");
                throw;
            }

            _loggerAdapter.Info($"[RecalculateAlert] END OK reportId={reportId}");
            return true;
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e, $"[RecalculateAlert] UNHANDLED exception for reportId={reportId}");
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
            .OnlyRealReports()
            .FilterByReportStatus(StatusConstants.ReportStatusesConsideredForAlertProcessing)
            .FilterByGroupLabel(reportGroupLabel)
            .SelectMany(r => r.ReportAlerts)
            .Where(ar => !alertIdToIgnore.HasValue || ar.AlertId != alertIdToIgnore.Value)
            .Select(ra => ra.Alert)
            .FirstOrDefaultAsync(a => a.Status == AlertStatus.Open);

        if (existingActiveAlertForLabel != null)
        {
            var reportsInLabelWithNoActiveAlert = await _nyssContext.Reports
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
            Status = AlertStatus.Open
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

    private async Task<HealthRiskType> GetHealthRiskType(int projectHealthRiskId)
    {
        var healthRiskType = await _nyssContext.ProjectHealthRisks
            .Where(phr => phr.Id == projectHealthRiskId)
            .Select(phr => (HealthRiskType?)phr.HealthRisk.HealthRiskType)
            .SingleOrDefaultAsync();

        return healthRiskType ?? HealthRiskType.Human;
    }
}
