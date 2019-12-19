using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertService
    {
        Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber);
        Task<Result<AlertAssessmentResponseDto>> GetAlert(int alertId);
        Task<Result> EscalateAlert(int alertId);
        Task<Result> DismissAlert(int alertId);
        Task<Result> CloseAlert(int alertId, string comments);
        Task<AlertAssessmentStatus> GetAlertAssessmentStatus(int alertId);
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
        private readonly ISmsTextGeneratorService _smsTextGeneratorService;
        private readonly IConfig _config;

        public AlertService(
            INyssContext nyssContext,
            IEmailPublisherService emailPublisherService,
            IEmailTextGeneratorService emailTextGeneratorService,
            IConfig config,
            ISmsTextGeneratorService smsTextGeneratorService)
        {
            _nyssContext = nyssContext;
            _emailPublisherService = emailPublisherService;
            _emailTextGeneratorService = emailTextGeneratorService;
            _smsTextGeneratorService = smsTextGeneratorService;
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
                    LastReportVillage = a.AlertReports.OrderByDescending(r => r.Report.Id).First().Report.Village.Name,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single()
                })
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

        public async Task<Result<AlertAssessmentResponseDto>> GetAlert(int alertId)
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
                        Village = ar.Report.Village.Name,
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
                AssessmentStatus = GetAlertAssessmentStatus(alert.Status, acceptedReports, pendingReports, alert.HealthRiskCountThreshold),
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

        public async Task<Result> EscalateAlert(int alertId)
        {
            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LastReportVillage = alert.AlertReports.OrderByDescending(r => r.Report.Id).First().Report.Village.Name,
                    LastReportGateway = alert.AlertReports.OrderByDescending(r => r.Report.Id).First().Report.RawReport.ApiKey,
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
                return Error(ResultKey.Alert.EscalateAlertWrongStatus);
            }

            if (alertData.AcceptedReportCount < alertData.CountThreshold)
            {
                return Error(ResultKey.Alert.EscalateAlertThresholdNotReached);
            }

            await SendNotificationEmails(alertData.LanguageCode, alertData.NotificationEmails, alertData.Project, alertData.HealthRisk, alertData.LastReportVillage);
            await SendNotificationSmses(alertData.NationalSocietyId, alertData.LastReportGateway, alertData.LanguageCode, alertData.NotificationPhoneNumbers, alertData.Project, alertData.HealthRisk, alertData.LastReportVillage);

            alertData.Alert.Status = AlertStatus.Escalated;
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result> DismissAlert(int alertId)
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

            if (alertData.Alert.Status != AlertStatus.Pending)
            {
                return Error(ResultKey.Alert.DismissAlertWrongStatus);
            }

            if (alertData.MaximumAcceptedReportCount >= alertData.CountThreshold)
            {
                return Error(ResultKey.Alert.DismissAlertPossibleEscalation);
            }

            alertData.Alert.Status = AlertStatus.Dismissed;
            await _nyssContext.SaveChangesAsync();

            return Success();
        }


        public async Task<Result> CloseAlert(int alertId, string comments)
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

            if (alertData.Alert.Status != AlertStatus.Escalated)
            {
                return Error(ResultKey.Alert.CloseAlertWrongStatus);
            }

            alertData.Alert.Status = AlertStatus.Closed;
            alertData.Alert.Comments = comments;

            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<AlertAssessmentStatus> GetAlertAssessmentStatus(int alertId)
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

            return GetAlertAssessmentStatus(alertData.Status, alertData.AcceptedReports, alertData.PendingReports, alertData.CountThreshold);
        }

        private static AlertAssessmentStatus GetAlertAssessmentStatus(AlertStatus alertStatus, int acceptedReports, int pendingReports, int countThreshold) =>
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
            var (subject, body) = await _emailTextGeneratorService.GenerateEscalatedAlertEmail(languageCode);

            body = body
                .Replace("{project}", project)
                .Replace("{healthRisk}", healthRisk)
                .Replace("{lastReportVillage}", lastReportVillage);

            foreach (var email in notificationEmails)
            {
                await _emailPublisherService.SendEmail((email, email), subject, body);
            }
        }

        private async Task SendNotificationSmses(int nationalSocietyId, string lastReportApiKey, string languageCode, List<string> notificationPhoneNumbers, string project, string healthRisk, string lastReportVillage)
        {
            var lastUsedGatewaySettings = await _nyssContext.GatewaySettings.Where(gs => gs.ApiKey == lastReportApiKey).FirstOrDefaultAsync();
            var gatewayEmail = lastUsedGatewaySettings?.EmailAddress ??
                (await _nyssContext.GatewaySettings.Where(gs => gs.NationalSocietyId == nationalSocietyId).FirstAsync()).EmailAddress;
            
            var text = await _smsTextGeneratorService.GenerateEscalatedAlertSms(languageCode);

            text = text
                .Replace("{project}", project)
                .Replace("{healthRisk}", healthRisk)
                .Replace("{lastReportVillage}", lastReportVillage);

            foreach (var sms in notificationPhoneNumbers)
            {
                await _emailPublisherService.SendEmail((gatewayEmail, gatewayEmail), sms, text, true);
            }
        }
    }
}
