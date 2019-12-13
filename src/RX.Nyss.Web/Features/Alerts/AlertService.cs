using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;
using Message = Microsoft.Azure.ServiceBus.Message;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertService
    {
        Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber);
        Task<Result<AlertAssessmentResponseDto>> GetAlert(int alertId);
        Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId);
        Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId);
        Task<Result> EscalateAlert(int alertId);
        Task<Result> DismissAlert(int alertId);
        Task<Result> CloseAlert(int alertId, string comments);
    }

    public class AlertService : IAlertService
    {
        private readonly INyssContext _nyssContext;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;
        private readonly IConfig _config;
        private readonly QueueClient _queueClient;

        public AlertService(
            INyssContext nyssContext,
            IEmailPublisherService emailPublisherService,
            IEmailTextGeneratorService emailTextGeneratorService,
            IConfig config)
        {
            _nyssContext = nyssContext;
            _emailPublisherService = emailPublisherService;
            _emailTextGeneratorService = emailTextGeneratorService;
            _config = config;
            // _queueClient = new QueueClient(_config.ConnectionStrings.ServiceBus, _config.ServiceBusQueues.ReportDismissalQueue);
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
                    NotificationEmails = a.ProjectHealthRisk.Project.AlertRecipients.Select(ar => ar.EmailAddress).ToList()
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
                NotificationPhoneNumbers = new string[0], // TODO
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

        public async Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId)
        {
            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (alertReport.Alert.Status != AlertStatus.Pending)
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReportWrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Pending)
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReportWrongReportStatus);
            }

            alertReport.Report.Status = ReportStatus.Accepted;
            await _nyssContext.SaveChangesAsync();

            var response = new AcceptReportResponseDto
            {
                AssessmentStatus = await GetAlertAssessmentStatus(alertId)
            };

            return Success(response);
        }

        public async Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId)
        {
            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (alertReport.Alert.Status != AlertStatus.Pending)
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.AcceptReportWrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Pending)
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.AcceptReportWrongReportStatus);
            }

            alertReport.Report.Status = ReportStatus.Rejected;

            // await SendToQueue(new DismissReportMessage { ReportId = reportId });

            await _nyssContext.SaveChangesAsync();

            var response = new DismissReportResponseDto
            {
                AssessmentStatus = await GetAlertAssessmentStatus(alertId)
            };

            return Success(response);
        }

        public async Task<Result> EscalateAlert(int alertId)
        {
            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LastReportVillage = alert.AlertReports.OrderByDescending(r => r.Report.Id).First().Report.Village.Name,
                    HealthRisk = alert.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Project = alert.ProjectHealthRisk.Project.Name,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    NotificationEmails = alert.ProjectHealthRisk.Project.AlertRecipients.Select(ar => ar.EmailAddress).ToList(),
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    AcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted)
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
            await SendNotificationSmses(alertData.LanguageCode, new List<string>());

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
                    NotificationEmails = alert.ProjectHealthRisk.Project.AlertRecipients.Select(ar => ar.EmailAddress).ToList(),
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
                    NotificationEmails = alert.ProjectHealthRisk.Project.AlertRecipients.Select(ar => ar.EmailAddress).ToList(),
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

        private async Task<AlertAssessmentStatus> GetAlertAssessmentStatus(int alertId)
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

                AlertStatus.Pending when acceptedReports >= countThreshold =>
                    AlertAssessmentStatus.ToEscalate,

                AlertStatus.Pending when acceptedReports + pendingReports >= countThreshold =>
                    AlertAssessmentStatus.Open,

                _ =>
                    AlertAssessmentStatus.ToDismiss
            };

        private static string GetSex(ReportCase reportedCase)
        {
            if (reportedCase.CountFemalesAtLeastFive > 0 || reportedCase.CountFemalesBelowFive > 0)
            {
                return "Female";
            }

            if (reportedCase.CountMalesBelowFive > 0 || reportedCase.CountMalesAtLeastFive > 0)
            {
                return "Male";
            }

            throw new ResultException(ResultKey.Alert.InconsistentReportData);
        }

        private static string GetAge(ReportCase reportedCase)
        {
            if (reportedCase.CountFemalesAtLeastFive > 0 || reportedCase.CountMalesAtLeastFive > 0)
            {
                return "AtLeastFive";
            }

            if (reportedCase.CountFemalesBelowFive > 0 || reportedCase.CountMalesBelowFive > 0)
            {
                return "BelowFive";
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

        private Task SendNotificationSmses(string languageCode, List<string> notificationPhoneNumbers)
        {
            // TODO
            return Task.CompletedTask;
        }

        private async Task SendToQueue<T>(T data)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
            {
                Label = "RX.Nyss.Web",
            };

            await _queueClient.SendAsync(message);
        }
    }
}
