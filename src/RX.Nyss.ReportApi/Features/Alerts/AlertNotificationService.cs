using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using RX.Nyss.ReportApi.Features.Common.Contracts;
using RX.Nyss.ReportApi.Services;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public interface IAlertNotificationService
    {
        Task<IEnumerable<SupervisorSmsRecipient>> GetSupervisorsConnectedToExistingAlert(int alertId);
        Task<IEnumerable<SupervisorSmsRecipient>> GetHeadSupervisorsConnectedToExistingAlert(int alertId);
        Task SendNotificationsForSupervisorsAddedToExistingAlert(Alert alert, List<SupervisorSmsRecipient> supervisors, GatewaySetting gatewaySetting);
        Task SendNotificationsForNewAlert(Alert alert, GatewaySetting gatewaySetting);
        Task EmailAlertNotHandledRecipientsIfAlertIsPending(int alertId);
    }

    public class AlertNotificationService : IAlertNotificationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IQueuePublisherService _queuePublisherService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly INyssReportApiConfig _config;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AlertNotificationService(
            INyssContext nyssContext,
            IQueuePublisherService queuePublisherService,
            IStringsResourcesService stringsResourcesService,
            INyssReportApiConfig config,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _queuePublisherService = queuePublisherService;
            _stringsResourcesService = stringsResourcesService;
            _config = config;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task SendNotificationsForNewAlert(Alert alert, GatewaySetting gatewaySetting)
        {
            var phoneNumbersOfSupervisorsInAlert = _nyssContext.AlertReports
                .Where(ar => ar.Alert.Id == alert.Id)
                .Select(ar => ar.Report.DataCollector.Supervisor)
                .Distinct()
                .Select(s => new SendSmsRecipient
                {
                    PhoneNumber = s.PhoneNumber,
                    Modem = s.Modem != null
                        ? s.Modem.ModemId
                        : null
                });

            var phoneNumbersOfHeadSupervisorsInAlert = _nyssContext.AlertReports
                .Where(ar => ar.Alert.Id == alert.Id)
                .Select(ar => ar.Report.DataCollector.HeadSupervisor)
                .Distinct()
                .Select(s => new SendSmsRecipient
                {
                    PhoneNumber = s.PhoneNumber,
                    Modem = s.Modem != null
                        ? s.Modem.ModemId
                        : null
                });

            var recipients = await phoneNumbersOfSupervisorsInAlert
                .Concat(phoneNumbersOfHeadSupervisorsInAlert)
                .ToListAsync();

            var message = await CreateNotificationMessageForNewAlert(alert);

            await _queuePublisherService.SendSms(recipients, gatewaySetting, message);
            await _queuePublisherService.QueueAlertCheck(alert.Id);
        }

        public async Task SendNotificationsForSupervisorsAddedToExistingAlert(Alert alert, List<SupervisorSmsRecipient> supervisors, GatewaySetting gatewaySetting)
        {
            var phoneNumbers = supervisors.Select(s => new SendSmsRecipient
            {
                PhoneNumber = s.PhoneNumber,
                Modem = s.Modem
            }).ToList();
            var message = await CreateNotificationMessageForExistingAlert(alert);

            await _queuePublisherService.SendSms(phoneNumbers, gatewaySetting, message);
        }

        public async Task<IEnumerable<SupervisorSmsRecipient>> GetSupervisorsConnectedToExistingAlert(int alertId) =>
            await _nyssContext.AlertReports
                .Where(ar => ar.AlertId == alertId && ar.Report.DataCollector.Supervisor != null)
                .Select(ar => ar.Report.DataCollector.Supervisor)
                .Distinct()
                .Select(s => new SupervisorSmsRecipient
                {
                    UserId = s.Id,
                    Name = s.Name,
                    PhoneNumber = s.PhoneNumber,
                    Modem = s.Modem.ModemId
                })
                .ToListAsync();

        public async Task<IEnumerable<SupervisorSmsRecipient>> GetHeadSupervisorsConnectedToExistingAlert(int alertId) =>
            await _nyssContext.AlertReports
                .Where(ar => ar.AlertId == alertId && ar.Report.DataCollector.HeadSupervisor != null)
                .Select(ar => ar.Report.DataCollector.HeadSupervisor)
                .Distinct()
                .Select(s => new SupervisorSmsRecipient
                {
                    UserId = s.Id,
                    Name = s.Name,
                    PhoneNumber = s.PhoneNumber,
                    Modem = s.Modem.ModemId
                })
                .ToListAsync();

        public async Task EmailAlertNotHandledRecipientsIfAlertIsPending(int alertId)
        {
            var alert = await _nyssContext.Alerts.Where(a => a.Id == alertId)
                .Select(a =>
                    new
                    {
                        a.Id,
                        a.Status,
                        a.CreatedAt,
                        ProjectId = a.ProjectHealthRisk.Project.Id,
                        Supervisors = a.AlertReports.Select(ar => ar.Report.DataCollector.Supervisor.UserNationalSocieties
                            .Where(uns => uns.NationalSociety == ar.Alert.ProjectHealthRisk.Project.NationalSociety)
                            .Select(uns => new
                            {
                                User = uns.User,
                                Organization = uns.Organization
                            }).First()),
                        HealthRiskName = a.ProjectHealthRisk.HealthRisk.LanguageContents.First().Name,
                        VillageOfLastReport = a.AlertReports.OrderByDescending(ar => ar.Report.ReceivedAt)
                            .Select(ar => ar.Report.RawReport.Village.Name)
                            .FirstOrDefault(),
                        LanguageCode = a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode.ToLower(),
                        AlertNotHandledRecipients = a.ProjectHealthRisk.Project.AlertNotHandledNotificationRecipients
                            .Select(anr => new
                            {
                                User = anr.User,
                                Organization = anr.Organization
                            })
                    })
                .FirstOrDefaultAsync();

            if (alert == null)
            {
                _loggerAdapter.WarnFormat("Alert {0} not found", alertId);
                return;
            }

            if (alert.Status == AlertStatus.Pending)
            {
                _loggerAdapter.WarnFormat("Alert {0} has not been assessed since it was triggered {1}, sending email to alert not handled recipients", alertId, alert.CreatedAt.ToString("O"));

                var timeSinceTriggered = (_dateTimeProvider.UtcNow - alert.CreatedAt).TotalHours;
                var emailSubject = await GetEmailMessageContent(EmailContentKey.AlertHasNotBeenHandled.Subject, alert.LanguageCode);
                var emailBody = await GetEmailMessageContent(EmailContentKey.AlertHasNotBeenHandled.Body, alert.LanguageCode);

                var baseUrl = new Uri(_config.BaseUrl);
                var relativeUrl = $"projects/{alert.ProjectId}/alerts/{alert.Id}/assess";
                var linkToAlert = new Uri(baseUrl, relativeUrl);

                var alertNotHandledRecipients = alert.AlertNotHandledRecipients
                    .Where(anhr => alert.Supervisors.Any(sup => sup.Organization.Id == anhr.Organization.Id));

                foreach (var recipient in alertNotHandledRecipients)
                {
                    var supervisors = alert.Supervisors
                        .Select(sup => sup.Organization.Id == recipient.Organization.Id
                            ? sup.User.Name
                            : sup.Organization.Name)
                        .Distinct();

                    var alertNotHandledEmailBody = emailBody
                        .Replace("{{healthRiskName}}", alert.HealthRiskName)
                        .Replace("{{lastReportVillage}}", alert.VillageOfLastReport)
                        .Replace("{{supervisors}}", string.Join(", ", supervisors))
                        .Replace("{{timeSinceAlertWasTriggeredInHours}}", timeSinceTriggered.ToString("0.##"))
                        .Replace("{{linkToAlert}}", linkToAlert.ToString());

                    await _queuePublisherService.SendEmail((recipient.User.Name, recipient.User.EmailAddress), emailSubject, alertNotHandledEmailBody);
                }
            }
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
            message = message.Replace("{{alertId}}", alert.Id.ToString());

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
            message = message.Replace("{{alertId}}", alert.Id.ToString());

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
    }
}
