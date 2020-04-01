using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Extensions;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertService
    {
        Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber);
        Task<Result<AlertAssessmentResponseDto>> Get(int alertId);
        Task<Result> Escalate(int alertId);
        Task<Result> Dismiss(int alertId);
        Task<Result> Close(int alertId, string comments);
        Task<AlertAssessmentStatus> GetAssessmentStatus(int alertId);
        Task<Result<AlertLogResponseDto>> GetLogs(int alertId);
    }

    public class AlertService : IAlertService
    {
        private const string SexFemale = "Female";
        private const string SexMale = "Male";
        private const string AgeAtLeastFive = "AtLeastFive";
        private const string AgeBelowFive = "BelowFive";

        private readonly INyssContext _nyssContext;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;
        private readonly ISmsPublisherService _smsPublisherService;
        private readonly ISmsTextGeneratorService _smsTextGeneratorService;
        private readonly INyssWebConfig _config;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;

        public AlertService(
            INyssContext nyssContext,
            IEmailPublisherService emailPublisherService,
            IEmailTextGeneratorService emailTextGeneratorService,
            INyssWebConfig config,
            ISmsTextGeneratorService smsTextGeneratorService,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService,
            ISmsPublisherService smsPublisherService)
        {
            _nyssContext = nyssContext;
            _emailPublisherService = emailPublisherService;
            _emailTextGeneratorService = emailTextGeneratorService;
            _smsTextGeneratorService = smsTextGeneratorService;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
            _smsPublisherService = smsPublisherService;
            _config = config;
        }

        public async Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber)
        {
            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);

            var alertsQuery = _nyssContext.Alerts
                .Where(a => a.ProjectHealthRisk.Project.Id == projectId);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalCount = await alertsQuery.CountAsync();

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.Id,
                    a.CreatedAt,
                    a.Status,
                    ReportCount = a.AlertReports.Count,
                    LastReportVillage = a.AlertReports.OrderByDescending(ar => ar.Report.Id).First().Report.RawReport.Village.Name,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single()
                })
                .OrderBy(a => a.Status == AlertStatus.Escalated ? 0 :
                    a.Status == AlertStatus.Pending ? 1 :
                    a.Status == AlertStatus.Rejected ? 2 :
                    a.Status == AlertStatus.Closed ? 3 : 4) // ...and Dismissed last
                .ThenByDescending(a => a.CreatedAt)
                .Page(pageNumber, rowsPerPage)
                .AsNoTracking()
                .ToListAsync();

            var dtos = alerts
                .Select(a => new AlertListItemResponseDto
                {
                    Id = a.Id,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(a.CreatedAt, projectTimeZone),
                    Status = a.Status.ToString(),
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReportVillage,
                    HealthRisk = a.HealthRisk
                })
                .AsPaginatedList(pageNumber, totalCount, rowsPerPage);

            return Success(dtos);
        }

        public async Task<Result<AlertAssessmentResponseDto>> Get(int alertId)
        {
            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    Comments = a.Comments,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    HealthRiskCountThreshold = a.ProjectHealthRisk.AlertRule.CountThreshold,
                    ProjectTimeZone = a.ProjectHealthRisk.Project.TimeZone,
                    CaseDefinition = a.ProjectHealthRisk.CaseDefinition,
                    Reports = a.AlertReports.Select(ar => new
                    {
                        Id = ar.Report.Id,
                        DataCollector = ar.Report.DataCollector.DisplayName,
                        ReceivedAt = ar.Report.ReceivedAt,
                        PhoneNumber = ar.Report.PhoneNumber,
                        Village = ar.Report.RawReport.Village.Name,
                        ReportedCase = ar.Report.ReportedCase,
                        Status = ar.Report.Status
                    }),
                    NotificationEmails = a.ProjectHealthRisk.Project.EmailAlertRecipients.Select(ar => ar.EmailAddress).ToList(),
                    NotificationPhoneNumbers = a.ProjectHealthRisk.Project.SmsAlertRecipients.Select(sar => sar.PhoneNumber).ToList()
                })
                .AsNoTracking()
                .SingleAsync();

            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(alert.ProjectTimeZone);

            var acceptedReports = alert.Reports.Count(r => r.Status == ReportStatus.Accepted);
            var pendingReports = alert.Reports.Count(r => r.Status == ReportStatus.Pending);

            var dto = new AlertAssessmentResponseDto
            {
                HealthRisk = alert.HealthRisk,
                Comments = alert.Comments,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(alert.CreatedAt, projectTimeZone),
                CaseDefinition = alert.CaseDefinition,
                NotificationEmails = alert.NotificationEmails,
                NotificationPhoneNumbers = alert.NotificationPhoneNumbers,
                AssessmentStatus = GetAssessmentStatus(alert.Status, acceptedReports, pendingReports, alert.HealthRiskCountThreshold),
                Reports = alert.Reports.Select(ar => new AlertAssessmentResponseDto.ReportDto
                {
                    Id = ar.Id,
                    DataCollector = ar.DataCollector,
                    ReceivedAt = TimeZoneInfo.ConvertTimeFromUtc(ar.ReceivedAt, projectTimeZone),
                    PhoneNumber = ar.PhoneNumber,
                    Status = ar.Status.ToString(),
                    Village = ar.Village,
                    Sex = GetSex(ar.ReportedCase),
                    Age = GetAge(ar.ReportedCase)
                }).ToList()
            };

            return Success(dto);
        }

        public async Task<Result> Escalate(int alertId)
        {
            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LastReportVillage = alert.AlertReports.OrderByDescending(r => r.Report.Id).First().Report.RawReport.Village.Name,
                    HealthRisk = alert.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Project = alert.ProjectHealthRisk.Project.Name,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    NotificationEmails = alert.ProjectHealthRisk.Project.EmailAlertRecipients.Select(ear => ear.EmailAddress).ToList(),
                    NotificationPhoneNumbers = alert.ProjectHealthRisk.Project.SmsAlertRecipients.Select(sar => sar.PhoneNumber).ToList(),
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    AcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted),
                    NationalSocietyId = alert.ProjectHealthRisk.Project.NationalSociety.Id
                })
                .SingleAsync();

            if (alertData.Alert.Status != AlertStatus.Pending)
            {
                return Error(ResultKey.Alert.EscalateAlert.WrongStatus);
            }

            if (alertData.AcceptedReportCount < alertData.CountThreshold)
            {
                return Error(ResultKey.Alert.EscalateAlert.ThresholdNotReached);
            }

            alertData.Alert.Status = AlertStatus.Escalated;
            alertData.Alert.EscalatedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.EscalatedBy = _authorizationService.GetCurrentUser();
            await _nyssContext.SaveChangesAsync();

            try
            {
                await SendNotificationEmails(alertData.LanguageCode, alertData.NotificationEmails, alertData.Project, alertData.HealthRisk, alertData.LastReportVillage);
                await SendNotificationSmses(alertData.NationalSocietyId, alertData.LanguageCode, alertData.NotificationPhoneNumbers, alertData.Project,
                    alertData.HealthRisk, alertData.LastReportVillage);
            }
            catch (ResultException exception)
            {
                return Success(exception.Result.Message.Key);
            }

            return Success(ResultKey.Alert.EscalateAlert.Success);
        }

        public async Task<Result> Dismiss(int alertId)
        {
            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    NotificationEmails = alert.ProjectHealthRisk.Project.EmailAlertRecipients.Select(ar => ar.EmailAddress).ToList(),
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    MaximumAcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted || r.Report.Status == ReportStatus.Pending)
                })
                .SingleAsync();

            if (alertData.Alert.Status != AlertStatus.Pending && alertData.Alert.Status != AlertStatus.Rejected)
            {
                return Error(ResultKey.Alert.DismissAlert.WrongStatus);
            }

            if (alertData.MaximumAcceptedReportCount >= alertData.CountThreshold && alertData.Alert.Status != AlertStatus.Rejected)
            {
                return Error(ResultKey.Alert.DismissAlert.PossibleEscalation);
            }

            alertData.Alert.Status = AlertStatus.Dismissed;
            alertData.Alert.DismissedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.DismissedBy = _authorizationService.GetCurrentUser();
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result> Close(int alertId, string comments)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    NotificationEmails = alert.ProjectHealthRisk.Project.EmailAlertRecipients.Select(ar => ar.EmailAddress).ToList(),
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    MaximumAcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted || r.Report.Status == ReportStatus.Pending)
                })
                .SingleAsync();

            if (alertData.Alert.Status != AlertStatus.Escalated)
            {
                return Error(ResultKey.Alert.CloseAlert.WrongStatus);
            }

            alertData.Alert.Status = AlertStatus.Closed;
            alertData.Alert.ClosedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.ClosedBy = _authorizationService.GetCurrentUser();
            alertData.Alert.Comments = comments;

            FormattableString updateReportsCommand = $@"UPDATE Nyss.Reports SET Status = {ReportStatus.Closed.ToString()} WHERE Status = {ReportStatus.Pending.ToString()}
                                AND Id IN (SELECT ReportId FROM Nyss.AlertReports WHERE AlertId = {alertData.Alert.Id}) ";
            await _nyssContext.ExecuteSqlInterpolatedAsync(updateReportsCommand);

            await _nyssContext.SaveChangesAsync();

            transactionScope.Complete();

            return Success();
        }

        public async Task<AlertAssessmentStatus> GetAssessmentStatus(int alertId)
        {
            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    a.Status,
                    a.ProjectHealthRisk.AlertRule.CountThreshold,
                    AcceptedReports = a.AlertReports.Count(ar => ar.Report.Status == ReportStatus.Accepted),
                    PendingReports = a.AlertReports.Count(ar => ar.Report.Status == ReportStatus.Pending)
                })
                .SingleAsync();

            return GetAssessmentStatus(alertData.Status, alertData.AcceptedReports, alertData.PendingReports, alertData.CountThreshold);
        }

        public async Task<Result<AlertLogResponseDto>> GetLogs(int alertId)
        {
            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    a.CreatedAt,
                    a.EscalatedAt,
                    a.DismissedAt,
                    a.ClosedAt,
                    EscalatedBy = a.EscalatedBy.Name,
                    DismissedBy = a.DismissedBy.Name,
                    ClosedBy = a.ClosedBy.Name,
                    ProjectTimeZone = a.ProjectHealthRisk.Project.TimeZone,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Reports = a.AlertReports
                        .Where(ar => ar.Report.Status == ReportStatus.Accepted || ar.Report.Status == ReportStatus.Rejected)
                        .Select(ar => new
                        {
                            ar.ReportId,
                            ar.Report.AcceptedAt,
                            ar.Report.RejectedAt,
                            AcceptedBy = ar.Report.AcceptedBy.Name,
                            RejectedBy = ar.Report.RejectedBy.Name
                        })
                        .ToList()
                })
                .SingleAsync();

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(alert.ProjectTimeZone);

            var list = new List<AlertLogResponseDto.Item> { new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.TriggeredAlert, alert.CreatedAt.ApplyTimeZone(timeZone), null) };

            if (alert.EscalatedAt.HasValue)
            {
                list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.EscalatedAlert, alert.EscalatedAt.Value.ApplyTimeZone(timeZone), alert.EscalatedBy));
            }

            if (alert.DismissedAt.HasValue)
            {
                list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.DismissedAlert, alert.DismissedAt.Value.ApplyTimeZone(timeZone), alert.DismissedBy));
            }

            if (alert.ClosedAt.HasValue)
            {
                list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.ClosedAlert, alert.ClosedAt.Value.ApplyTimeZone(timeZone), alert.ClosedBy));
            }

            foreach (var report in alert.Reports)
            {
                if (report.AcceptedAt.HasValue)
                {
                    list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.AcceptedReport, report.AcceptedAt.Value.ApplyTimeZone(timeZone), report.AcceptedBy, new { report.ReportId }));
                }

                if (report.RejectedAt.HasValue)
                {
                    list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.RejectedReport, report.RejectedAt.Value.ApplyTimeZone(timeZone), report.RejectedBy, new { report.ReportId }));
                }
            }

            return Success(new AlertLogResponseDto
            {
                HealthRisk = alert.HealthRisk,
                CreatedAt = alert.CreatedAt.ApplyTimeZone(timeZone),
                Items = list.OrderBy(x => x.Date).ToList()
            });
        }

        private static AlertAssessmentStatus GetAssessmentStatus(AlertStatus alertStatus, int acceptedReports, int pendingReports, int countThreshold) =>
            alertStatus switch
            {
                AlertStatus.Escalated =>
                AlertAssessmentStatus.Escalated,

                AlertStatus.Dismissed =>
                AlertAssessmentStatus.Dismissed,

                AlertStatus.Closed =>
                AlertAssessmentStatus.Closed,

                AlertStatus.Rejected =>
                AlertAssessmentStatus.Rejected,

                AlertStatus.Pending when acceptedReports >= countThreshold =>
                AlertAssessmentStatus.ToEscalate,

                AlertStatus.Pending when acceptedReports + pendingReports >= countThreshold =>
                AlertAssessmentStatus.Open,

                _ =>
                AlertAssessmentStatus.ToDismiss
            };

        private static string GetSex(ReportCase reportedCase)
        {
            if (reportedCase == null)
            {
                return null;
            }

            if (reportedCase.CountFemalesAtLeastFive > 0 || reportedCase.CountFemalesBelowFive > 0)
            {
                return SexFemale;
            }

            if (reportedCase.CountMalesBelowFive > 0 || reportedCase.CountMalesAtLeastFive > 0)
            {
                return SexMale;
            }

            throw new ResultException(ResultKey.Alert.InconsistentReportData);
        }

        private static string GetAge(ReportCase reportedCase)
        {
            if (reportedCase == null)
            {
                return null;
            }

            if (reportedCase.CountFemalesAtLeastFive > 0 || reportedCase.CountMalesAtLeastFive > 0)
            {
                return AgeAtLeastFive;
            }

            if (reportedCase.CountFemalesBelowFive > 0 || reportedCase.CountMalesBelowFive > 0)
            {
                return AgeBelowFive;
            }

            throw new ResultException(ResultKey.Alert.InconsistentReportData);
        }

        private async Task SendNotificationEmails(string languageCode, List<string> notificationEmails, string project, string healthRisk, string lastReportVillage)
        {
            try
            {
                var (subject, body) = await _emailTextGeneratorService.GenerateEscalatedAlertEmail(languageCode);

                body = body
                    .Replace("{{project}}", project)
                    .Replace("{{healthRisk}}", healthRisk)
                    .Replace("{{lastReportVillage}}", lastReportVillage);

                foreach (var email in notificationEmails)
                {
                    await _emailPublisherService.SendEmail((email, email), subject, body);
                }
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"Failed to send escalation notification emails for project {project} with health risk {healthRisk}");
                throw new ResultException(ResultKey.Alert.EscalateAlert.EmailNotificationFailed);
            }
        }

        private async Task SendNotificationSmses(int nationalSocietyId, string languageCode, List<string> notificationPhoneNumbers, string project, string healthRisk,
            string lastReportVillage)
        {
            try
            {
                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Where(gs => gs.NationalSocietyId == nationalSocietyId).FirstOrDefaultAsync();

                if (gatewaySetting == null)
                {
                    throw new ArgumentException("SmsGateway not found!");
                }

                var text = await _smsTextGeneratorService.GenerateEscalatedAlertSms(languageCode);
                text = text
                    .Replace("{{project}}", project)
                    .Replace("{{healthRisk}}", healthRisk)
                    .Replace("{{lastReportVillage}}", lastReportVillage);

                if (!string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
                {
                    await _smsPublisherService.SendSms(gatewaySetting.IotHubDeviceName, notificationPhoneNumbers, text);
                }
                else
                {
                    await Task.WhenAll(notificationPhoneNumbers
                        .Select(sms => _emailPublisherService.SendEmail((gatewaySetting.EmailAddress, gatewaySetting.EmailAddress), sms, text, true)));
                }
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"Failed to send escalation notification SMSes for project {project} with health risk {healthRisk}");
                throw new ResultException(ResultKey.Alert.EscalateAlert.SmsNotificationFailed);
            }
        }
    }
}
