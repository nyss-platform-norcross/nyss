using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Common;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.ReportApi.Services;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IAlertService
    {
        Task<AlertData> ReportAdded(Report report);
        Task SendNotificationsForNewAlert(Alert alert, GatewaySetting gatewaySetting);
        Task SendNotificationsForSupervisorsAddedToExistingAlert(Alert alert, List<SupervisorUser> supervisors, GatewaySetting gatewaySetting);
        Task EmailHeadManagerIfAlertIsPending(int alertId);
    }

    public class AlertService : IAlertService
    {
        private readonly INyssContext _nyssContext;
        private readonly IReportLabelingService _reportLabelingService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IQueuePublisherService _queuePublisherService;
        private readonly INyssReportApiConfig _config;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AlertService(INyssContext nyssContext, IReportLabelingService reportLabelingService, ILoggerAdapter loggerAdapter, IQueuePublisherService queuePublisherService,
            INyssReportApiConfig config, IStringsResourcesService stringsResourcesService, IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _reportLabelingService = reportLabelingService;
            _loggerAdapter = loggerAdapter;
            _queuePublisherService = queuePublisherService;
            _config = config;
            _stringsResourcesService = stringsResourcesService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<AlertData> ReportAdded(Report report)
        {
            var dataCollectorIsNotHuman = report.DataCollector.DataCollectorType != DataCollectorType.Human;
            var reportTypeIsAggregateOrDcp = report.ReportType == ReportType.Aggregate || report.ReportType == ReportType.DataCollectionPoint;
            var healthRiskTypeIsActivity = report.ProjectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Activity;

            if (report.IsTraining || dataCollectorIsNotHuman || reportTypeIsAggregateOrDcp || healthRiskTypeIsActivity)
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

        public async Task SendNotificationsForNewAlert(Alert alert, GatewaySetting gatewaySetting)
        {
            var phoneNumbersOfSupervisorsInAlert = await _nyssContext.AlertReports
                .Where(ar => ar.Alert.Id == alert.Id)
                .Select(ar => ar.Report.DataCollector.Supervisor)
                .Distinct()
                .Select(s => new SendSmsRecipient
                {
                    PhoneNumber = s.PhoneNumber,
                    Modem = s.Modem.ModemId
                })
                .ToListAsync();

            var message = await CreateNotificationMessageForNewAlert(alert);

            await _queuePublisherService.SendSms(phoneNumbersOfSupervisorsInAlert, gatewaySetting, message);
            await _queuePublisherService.QueueAlertCheck(alert.Id);
        }

        public async Task SendNotificationsForSupervisorsAddedToExistingAlert(Alert alert, List<SupervisorUser> supervisors, GatewaySetting gatewaySetting)
        {
            var phoneNumbers = supervisors.Select(s => new SendSmsRecipient
            {
                PhoneNumber = s.PhoneNumber,
                Modem = s.Modem.ModemId
            }).ToList();
            var message = await CreateNotificationMessageForExistingAlert(alert);

            await _queuePublisherService.SendSms(phoneNumbers, gatewaySetting, message);
        }

        public async Task EmailHeadManagerIfAlertIsPending(int alertId)
        {
            var alert = await _nyssContext.Alerts.Where(a => a.Id == alertId)
                .Select(a =>
                    new
                    {
                        a.Id,
                        a.Status,
                        a.CreatedAt,
                        ProjectId = a.ProjectHealthRisk.Project.Id,
                        HeadManagers = a.ProjectHealthRisk.Project.NationalSociety.NationalSocietyUsers
                            .Where(nsu => nsu.Organization.HeadManager == nsu.User)
                            .Where(nshm => a.AlertReports.Any(ar => nshm.Organization == ar.Report.DataCollector.Supervisor.UserNationalSocieties
                                                                                            .Where(suns => suns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                                                                            .Select(suns => suns.Organization)
                                                                                            .First()))
                            .Select(nsu => new { nsu.User.Name, nsu.User.EmailAddress, Organization = nsu.Organization.Name }),
                        Supervisors = a.AlertReports
                            .Select(ar => new
                            {
                                Name = ar.Report.DataCollector.Supervisor.Name,
                                Organization = ar.Report.DataCollector.Supervisor.UserNationalSocieties
                                    .Where(suns => suns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                    .Select(suns => suns.Organization.Name)
                                    .First()
                            }),
                        HealthRiskName = a.ProjectHealthRisk.HealthRisk.LanguageContents.First().Name,
                        VillageOfLastReport = a.AlertReports.OrderByDescending(ar => ar.Report.ReceivedAt)
                            .Select(ar => ar.Report.RawReport.Village.Name)
                            .FirstOrDefault(),
                        LanguageCode = a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode.ToLower()
                    })
                .FirstOrDefaultAsync();

            if (alert == null)
            {
                _loggerAdapter.WarnFormat("Alert {0} not found", alertId);
                return;
            }

            if (alert.Status == AlertStatus.Pending)
            {
                _loggerAdapter.WarnFormat("Alert {0} has not been assessed since it was triggered {1}, sending email to head manager", alertId, alert.CreatedAt.ToString("O"));

                var timeSinceTriggered = (_dateTimeProvider.UtcNow - alert.CreatedAt).TotalHours;
                var emailSubject = await GetEmailMessageContent(EmailContentKey.AlertHasNotBeenHandled.Subject, alert.LanguageCode);
                var emailBody = await GetEmailMessageContent(EmailContentKey.AlertHasNotBeenHandled.Body, alert.LanguageCode);

                var baseUrl = new Uri(_config.BaseUrl);
                var relativeUrl = $"projects/{alert.ProjectId}/alerts/{alert.Id}/assess";
                var linkToAlert = new Uri(baseUrl, relativeUrl);

                foreach (var headManager in alert.HeadManagers)
                {
                    var supervisors = alert.Supervisors
                        .Select(supervisor => supervisor.Organization == headManager.Organization
                            ? supervisor.Name
                            : supervisor.Organization)
                        .Distinct();

                    var headManagerEmailBody = emailBody
                        .Replace("{{healthRiskName}}", alert.HealthRiskName)
                        .Replace("{{lastReportVillage}}", alert.VillageOfLastReport)
                        .Replace("{{supervisors}}", string.Join(", ", supervisors))
                        .Replace("{{timeSinceAlertWasTriggeredInHours}}", timeSinceTriggered.ToString("0.##"))
                        .Replace("{{linkToAlert}}", linkToAlert.ToString());

                    await _queuePublisherService.SendEmail((headManager.Name, headManager.EmailAddress), emailSubject, headManagerEmailBody);
                }
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
                .Where(r => r.ReportGroupLabel == reportGroupLabel)
                .Where(r => !r.ReportAlerts.Any(ra => StatusConstants.AlertStatusesNotAllowingReportsToTriggerNewAlert.Contains(ra.Alert.Status)))
                .Where(r => StatusConstants.ReportStatusesConsideredForAlertProcessing.Contains(r.Status))
                .Where(r => !r.IsTraining)
                .Where(r => !r.MarkedAsError)
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

        private async Task<(Alert, List<SupervisorUser>)> IncludeAllReportsWithLabelInExistingAlert(Guid reportGroupLabel, int? alertIdToIgnore = null)
        {
            var addedSupervisors = new List<SupervisorUser>();
            var existingActiveAlertForLabel = await _nyssContext.Reports
                .Where(r => StatusConstants.ReportStatusesConsideredForAlertProcessing.Contains(r.Status))
                .Where(r => !r.MarkedAsError)
                .Where(r => !r.IsTraining)
                .Where(r => r.ReportGroupLabel == reportGroupLabel)
                .SelectMany(r => r.ReportAlerts)
                .Where(ar => !alertIdToIgnore.HasValue || ar.AlertId != alertIdToIgnore.Value)
                .Select(ra => ra.Alert)
                .FirstOrDefaultAsync(a => a.Status == AlertStatus.Pending);

            if (existingActiveAlertForLabel != null)
            {
                var reportsInLabelWithNoActiveAlert = await _nyssContext.Reports
                    .Where(r => StatusConstants.ReportStatusesConsideredForAlertProcessing.Contains(r.Status))
                    .Where(r => !r.MarkedAsError)
                    .Where(r => !r.IsTraining)
                    .Where(r => r.ReportGroupLabel == reportGroupLabel)
                    .Where(r => !r.ReportAlerts.Any(ra => ra.Alert.Status == AlertStatus.Pending || ra.Alert.Status == AlertStatus.Escalated || ra.Alert.Status == AlertStatus.Closed)
                        || r.ReportAlerts.Any(ra => ra.AlertId == alertIdToIgnore))
                    .Include(r => r.DataCollector)
                    .ThenInclude(dc => dc.Supervisor)
                    .ToListAsync();

                var supervisorsConnectedToExistingAlert = await GetSupervisorsConnectedToExistingAlert(existingActiveAlertForLabel);
                addedSupervisors = reportsInLabelWithNoActiveAlert
                    .Select(r => r.DataCollector.Supervisor)
                    .Distinct()
                    .Where(s => !supervisorsConnectedToExistingAlert.Any(sup => sup == s))
                    .ToList();

                await AddReportsToAlert(existingActiveAlertForLabel, reportsInLabelWithNoActiveAlert);
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

        private async Task<string> CreateNotificationMessageForNewAlert(Alert alert)
        {
            var alertData = await _nyssContext.Alerts.Where(a => a.Id == alert.Id)
                .Select(a => new
                {
                    ProjectId = a.ProjectHealthRisk.Project.Id,
                    HealthRiskName = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage)
                        .Select(lc => lc.Name)
                        .FirstOrDefault(),
                    a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    VillageOfLastReport = a.AlertReports.OrderByDescending(ar => ar.Report.ReceivedAt)
                        .Select(ar => ar.Report.RawReport.Village.Name)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            var message = await GetSmsMessageContent(SmsContentKey.Alerts.AlertTriggered, alertData.LanguageCode.ToLower());

            var baseUrl = new Uri(_config.BaseUrl);
            var relativeUrl = $"projects/{alertData.ProjectId}/alerts/{alert.Id}/assess";
            var linkToAlert = new Uri(baseUrl, relativeUrl);

            message = message.Replace("{{healthRisk/event}}", alertData.HealthRiskName);
            message = message.Replace("{{village}}", alertData.VillageOfLastReport);
            message = message.Replace("{{linkToAlert}}", linkToAlert.ToString());

            return message;
        }

        private async Task<string> CreateNotificationMessageForExistingAlert(Alert alert)
        {
            var alertData = await _nyssContext.Alerts.Where(a => a.Id == alert.Id)
                .Select(a => new
                {
                    ProjectId = a.ProjectHealthRisk.Project.Id,
                    HealthRiskName = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage)
                        .Select(lc => lc.Name)
                        .FirstOrDefault(),
                    a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    VillageOfLastReport = a.AlertReports.OrderByDescending(ar => ar.Report.ReceivedAt)
                        .Select(ar => ar.Report.RawReport.Village.Name)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            var message = await GetSmsMessageContent(SmsContentKey.Alerts.SupervisorAddedToExistingAlert, alertData.LanguageCode.ToLower());

            var baseUrl = new Uri(_config.BaseUrl);
            var relativeUrl = $"projects/{alertData.ProjectId}/alerts/{alert.Id}/assess";
            var linkToAlert = new Uri(baseUrl, relativeUrl);

            message = message.Replace("{{healthRisk/event}}", alertData.HealthRiskName);
            message = message.Replace("{{linkToAlert}}", linkToAlert.ToString());

            return message;
        }

        private async Task<string> GetSmsMessageContent(string key, string languageCode)
        {
            var smsContents = await _stringsResourcesService.GetSmsContentResources(!string.IsNullOrEmpty(languageCode)
                ? languageCode
                : "en");
            smsContents.Value.TryGetValue(key, out var message);

            if (message == null)
            {
                throw new ArgumentException($"No sms content resource found for key '{key}' (languageCode: {languageCode})");
            }

            if (message?.Length > 160)
            {
                _loggerAdapter.Warn($"SMS content with key '{key}' ({languageCode}) is longer than 160 characters.");
            }

            return message;
        }

        private async Task<string> GetEmailMessageContent(string key, string languageCode)
        {
            var contents = await _stringsResourcesService.GetEmailContentResources(!string.IsNullOrEmpty(languageCode)
                ? languageCode
                : "en");

            if (!contents.Value.TryGetValue(key, out var message))
            {
                throw new ArgumentException($"No email content resource found for key '{key}' (languageCode: {languageCode})");
            }

            return message;
        }

        private async Task<IEnumerable<SupervisorUser>> GetSupervisorsConnectedToExistingAlert(Alert alert) =>
            await _nyssContext.AlertReports
                .Where(ar => ar.Alert == alert)
                .Select(ar => ar.Report.DataCollector.Supervisor)
                .Distinct()
                .ToListAsync();
    }
}
