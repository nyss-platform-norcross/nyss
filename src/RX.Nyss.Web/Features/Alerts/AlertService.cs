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
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertService
    {
        Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber, AlertListFilterRequestDto filterRequestDto);

        Task<Result> Escalate(int alertId, bool sendNotification);

        Task<Result> Dismiss(int alertId);

        Task<Result> Close(int alertId);

        Task<AlertAssessmentStatus> GetAssessmentStatus(int alertId);

        Task<Result<AlertRecipientsResponseDto>> GetAlertRecipientsByAlertId(int alertId);

        Task<Result<AlertListFilterResponseDto>> GetFiltersData(int projectId);

        Task<byte[]> Export(int projectId, AlertListFilterRequestDto filterRequestDto);
    }

    public class AlertService : IAlertService
    {
        private readonly INyssContext _nyssContext;

        private readonly IEmailPublisherService _emailPublisherService;

        private readonly IEmailTextGeneratorService _emailTextGeneratorService;

        private readonly ISmsPublisherService _smsPublisherService;

        private readonly ISmsTextGeneratorService _smsTextGeneratorService;

        private readonly INyssWebConfig _config;

        private readonly ILoggerAdapter _loggerAdapter;

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly IAuthorizationService _authorizationService;

        private readonly IProjectService _projectService;

        private readonly IExcelExportService _excelExportService;

        private readonly IStringsService _stringsService;

        public AlertService(
            INyssContext nyssContext,
            IEmailPublisherService emailPublisherService,
            IEmailTextGeneratorService emailTextGeneratorService,
            INyssWebConfig config,
            ISmsTextGeneratorService smsTextGeneratorService,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService,
            ISmsPublisherService smsPublisherService,
            IProjectService projectService,
            IExcelExportService excelExportService,
            IStringsService stringsService)
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
            _projectService = projectService;
            _excelExportService = excelExportService;
            _stringsService = stringsService;
        }

        public async Task<Result<AlertListFilterResponseDto>> GetFiltersData(int projectId)
        {
            var healthRiskTypes = new List<HealthRiskType>
            {
                HealthRiskType.Human,
                HealthRiskType.NonHuman,
                HealthRiskType.UnusualEvent
            };
            var healthRisks = await _projectService.GetHealthRiskNames(projectId, healthRiskTypes);

            return Success(new AlertListFilterResponseDto
            {
                HealthRisks = healthRisks
            });
        }

        public async Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber, AlertListFilterRequestDto filterRequestDto)
        {
            var alertsQuery = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByHealthRisk(filterRequestDto.HealthRiskId)
                .FilterByArea(MapToArea(filterRequestDto.Area))
                .FilterByStatus(filterRequestDto.Status)
                .Sort(filterRequestDto.OrderBy, filterRequestDto.SortAscending);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalCount = await alertsQuery.CountAsync();
            var currentRole = (await _authorizationService.GetCurrentUser()).Role;
            var currentUserName = _authorizationService.GetCurrentUserName();
            var currentUserId = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == currentUserName)
                .Select(u => u.Id)
                .SingleAsync();
            var currentUserOrganizationId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUserId)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.Id,
                    CreatedAt = a.CreatedAt.AddHours(filterRequestDto.UtcOffset),
                    a.Status,
                    a.EscalatedOutcome,
                    a.Comments,
                    ReportCount = a.AlertReports.Count,
                    LastReport = a.AlertReports.OrderByDescending(ar => ar.Report.Id)
                        .Select(ar => new
                        {
                            VillageName = ar.Report.RawReport.Village.Name,
                            DistrictName = ar.Report.RawReport.Village.District.Name,
                            RegionName = ar.Report.RawReport.Village.District.Region.Name,
                            IsAnonymized = currentRole != Role.Administrator && !ar.Report.RawReport.NationalSociety.NationalSocietyUsers.Any(
                                nsu => nsu.UserId == ar.Report.RawReport.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganizationId)
                        }).First(),
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
                    CreatedAt = a.CreatedAt,
                    Status = a.Status.ToString(),
                    EscalatedOutcome = a.EscalatedOutcome,
                    Comments = a.Comments,
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReport.IsAnonymized
                        ? ""
                        : a.LastReport.VillageName,
                    LastReportDistrict = a.LastReport.DistrictName,
                    LastReportRegion = a.LastReport.RegionName,
                    HealthRisk = a.HealthRisk
                })
                .AsPaginatedList(pageNumber, totalCount, rowsPerPage);

            return Success(dtos);
        }

        public async Task<Result> Escalate(int alertId, bool sendNotification)
        {
            if (!await HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.EscalateAlert.NoPermission);
            }

            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LastReportVillage = alert.AlertReports.OrderByDescending(r => r.Report.Id)
                        .Select(ar => $"{ar.Report.RawReport.Village.Name}, {ar.Report.RawReport.Village.District.Name}, {ar.Report.RawReport.Village.District.Region.Name}")
                        .First(),
                    HealthRisk = alert.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Project = alert.ProjectHealthRisk.Project.Name,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
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
            alertData.Alert.EscalatedBy = await _authorizationService.GetCurrentUser();
            await _nyssContext.SaveChangesAsync();

            if (sendNotification)
            {
                var notificationRecipients = await GetAlertRecipients(alertId);
                try
                {
                    var notificationEmails = notificationRecipients
                        .Where(nr => !string.IsNullOrEmpty(nr.Email))
                        .Select(nr => nr.Email).Distinct().ToList();
                    var notificationPhoneNumbers = notificationRecipients
                        .Where(nr => !string.IsNullOrEmpty(nr.PhoneNumber))
                        .Select(nr => new SendSmsRecipient
                        {
                            PhoneNumber = nr.PhoneNumber,
                            Modem = nr.GatewayModem?.ModemId
                        })
                        .GroupBy(nr => nr.PhoneNumber)
                        .Select(g => g.First())
                        .ToList();

                    await SendNotificationEmails(alertData.LanguageCode, notificationEmails, alertData.Project, alertData.HealthRisk, alertData.LastReportVillage);
                    await SendNotificationSmses(alertData.NationalSocietyId, alertData.LanguageCode, notificationPhoneNumbers, alertData.Project,
                        alertData.HealthRisk, alertData.LastReportVillage);
                }
                catch (ResultException exception)
                {
                    return Success(exception.Result.Message.Key);
                }
            }

            return Success(ResultKey.Alert.EscalateAlert.Success);
        }

        public async Task<Result> Dismiss(int alertId)
        {
            if (!await HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.DismissAlert.NoPermission);
            }

            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    NotificationEmails = alert.ProjectHealthRisk.Project.AlertNotificationRecipients.Select(ar => ar.Email).ToList(),
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    MaximumAcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted || r.Report.Status == ReportStatus.Pending)
                })
                .SingleAsync();

            if (alertData.Alert.Status != AlertStatus.Pending)
            {
                return Error(ResultKey.Alert.DismissAlert.WrongStatus);
            }

            if (alertData.MaximumAcceptedReportCount >= alertData.CountThreshold)
            {
                return Error(ResultKey.Alert.DismissAlert.PossibleEscalation);
            }

            alertData.Alert.Status = AlertStatus.Dismissed;
            alertData.Alert.DismissedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.DismissedBy = await _authorizationService.GetCurrentUser();
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result> Close(int alertId)
        {
            if (!await HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.CloseAlert.NoPermission);
            }

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .SingleAsync();

            if (alert.Status != AlertStatus.Escalated)
            {
                return Error(ResultKey.Alert.CloseAlert.WrongStatus);
            }

            alert.Status = AlertStatus.Closed;
            alert.ClosedAt = _dateTimeProvider.UtcNow;
            alert.ClosedBy = await _authorizationService.GetCurrentUser();

            FormattableString updateReportsCommand = $@"UPDATE Nyss.Reports SET Status = {ReportStatus.Closed.ToString()} WHERE Status = {ReportStatus.Pending.ToString()}
                                AND Id IN (SELECT ReportId FROM Nyss.AlertReports WHERE AlertId = {alert.Id}) ";
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

            return alertData.Status.GetAssessmentStatus(alertData.AcceptedReports, alertData.PendingReports, alertData.CountThreshold);
        }

        public async Task<byte[]> Export(int projectId, AlertListFilterRequestDto filterRequestDto)
        {
            var currentRole = (await _authorizationService.GetCurrentUser()).Role;
            var currentUserName = _authorizationService.GetCurrentUserName();
            var currentUserId = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == currentUserName)
                .Select(u => u.Id)
                .SingleAsync();
            var currentUserOrganizationId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUserId)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            var strings = await _stringsService.GetForCurrentUser();

            var alertsQuery = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByHealthRisk(filterRequestDto.HealthRiskId)
                .FilterByArea(MapToArea(filterRequestDto.Area))
                .FilterByStatus(filterRequestDto.Status)
                .Sort(filterRequestDto.OrderBy, filterRequestDto.SortAscending);

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.Id,
                    a.CreatedAt,
                    a.EscalatedAt,
                    a.DismissedAt,
                    a.ClosedAt,
                    a.Status,
                    a.EscalatedOutcome,
                    a.Comments,
                    ReportCount = a.AlertReports.Count,
                    LastReport = a.AlertReports.OrderByDescending(ar => ar.Report.Id)
                        .Select(ar => new
                        {
                            ZoneName = ar.Report.RawReport.Zone.Name,
                            VillageName = ar.Report.RawReport.Village.Name,
                            DistrictName = ar.Report.RawReport.Village.District.Name,
                            RegionName = ar.Report.RawReport.Village.District.Region.Name,
                            Timestamp = ar.Report.ReceivedAt,
                            IsAnonymized = currentRole != Role.Administrator && !ar.Report.RawReport.NationalSociety.NationalSocietyUsers.Any(
                                nsu => nsu.UserId == ar.Report.RawReport.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganizationId)
                        }).First(),
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    InvestigationEventSubtype = _nyssContext.AlertEventLogs
                        .Where(log => log.AlertId == a.Id && log.AlertEventType.Id == 1)
                        .Select(log => log.AlertEventSubtype)
                        .First(),
                    OutcomeEventSubtype = _nyssContext.AlertEventLogs
                        .Where(log => log.AlertId == a.Id && log.AlertEventType.Id == 4)
                        .Select(log => log.AlertEventSubtype)
                        .First()
                })
                .ToListAsync();

            var dtos = alerts
                .Select(a => new AlertListExportResponseDto
                {
                    Id = a.Id,
                    LastReportTimestamp = a.LastReport.Timestamp.AddHours(filterRequestDto.UtcOffset),
                    TriggeredAt = a.CreatedAt.AddHours(filterRequestDto.UtcOffset),
                    EscalatedAt = a.EscalatedAt?.AddHours(filterRequestDto.UtcOffset),
                    DismissedAt = a.DismissedAt?.AddHours(filterRequestDto.UtcOffset),
                    ClosedAt = a.ClosedAt?.AddHours(filterRequestDto.UtcOffset),
                    Status = strings[$"alerts.alertStatus.{a.Status.ToString().ToLower()}"],
                    EscalatedOutcome = a.EscalatedOutcome,
                    Comments = a.Comments,
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReport.IsAnonymized
                        ? ""
                        : a.LastReport.VillageName,
                    LastReportDistrict = a.LastReport.DistrictName,
                    LastReportRegion = a.LastReport.RegionName,
                    HealthRisk = a.HealthRisk,
                    Investigation = a.InvestigationEventSubtype != null
                        ? strings[$"alerts.eventTypes.subtypes.{a.InvestigationEventSubtype.Name.ToString().ToCamelCase()}"]
                        : null,
                    Outcome = a.OutcomeEventSubtype != null
                        ? strings[$"alerts.eventTypes.subtypes.{a.OutcomeEventSubtype.Name.ToString().ToCamelCase()}"]
                        : null
                }).ToList();

            var documentTitle = strings["alerts.export.title"];
            var columnLabels = GetColumnLabels(strings);
            var excelDoc = _excelExportService.ToExcel(dtos, columnLabels, documentTitle);

            return excelDoc.GetAsByteArray();
        }

        public async Task<Result<AlertRecipientsResponseDto>> GetAlertRecipientsByAlertId(int alertId)
        {
            var currentUserOrganization = await _nyssContext.Alerts.Where(a => a.Id == alertId)
                .SelectMany(a => a.ProjectHealthRisk.Project.ProjectOrganizations
                    .Where(po => po.Organization.NationalSocietyUsers.Any(nsu => nsu.User.EmailAddress == _authorizationService.GetCurrentUserName())))
                .SingleOrDefaultAsync();

            var recipients = (await GetAlertRecipients(alertId)).Select(r => new
            {
                IsAnonymized = !_authorizationService.IsCurrentUserInRole(Role.Administrator) && r.OrganizationId != currentUserOrganization.OrganizationId,
                r.PhoneNumber,
                r.Email
            }).ToList();

            return Success(new AlertRecipientsResponseDto
            {
                Emails = recipients.Where(r => !string.IsNullOrEmpty(r.Email)).Select(r => r.IsAnonymized
                    ? "***"
                    : r.Email).ToList(),
                PhoneNumbers = recipients.Where(r => !string.IsNullOrEmpty(r.PhoneNumber)).Select(r => r.IsAnonymized
                    ? "***"
                    : r.PhoneNumber).ToList(),
            });
        }

        private static IReadOnlyList<string> GetColumnLabels(StringsResourcesVault strings) =>
            new []
            {
                strings["alerts.export.id"],
                strings["alerts.export.dateTriggered"],
                strings["alerts.export.timeTriggered"],
                strings["alerts.export.dateOfLastReport"],
                strings["alerts.export.timeOfLastReport"],
                strings["alerts.export.healthRisk"],
                strings["alerts.export.reports"],
                strings["alerts.export.status"],
                strings["alerts.export.lastReportRegion"],
                strings["alerts.export.lastReportDistrict"],
                strings["alerts.export.lastReportVillage"],
                strings["alerts.export.lastReportZone"],
                strings["alerts.export.dateEscalated"],
                strings["alerts.export.timeEscalated"],
                strings["alerts.export.dateClosed"],
                strings["alerts.export.timeClosed"],
                strings["alerts.export.dateDismissed"],
                strings["alerts.export.timeDismissed"],
                strings["alerts.export.investigation"],
                strings["alerts.export.outcome"],
                strings["alerts.export.escalatedOutcome"],
                strings["alerts.export.closedComments"],
            };

        private async Task<List<AlertNotificationRecipient>> GetAlertRecipients(int alertId)
        {
            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    ProjectId = a.ProjectHealthRisk.Project.Id,
                    ProjectHealthRiskId = a.ProjectHealthRisk.Id,
                    InvolvedSupervisorIds = a.AlertReports
                        .Where(ar => ar.Report.DataCollector.Supervisor != null)
                        .Select(ar => ar.Report.DataCollector.Supervisor.Id)
                        .ToList(),
                    InvolvedHeadSupervisorIds = a.AlertReports
                        .Where(ar => ar.Report.DataCollector.HeadSupervisor != null)
                        .Select(ar => ar.Report.DataCollector.HeadSupervisor.Id)
                        .ToList(),
                    InvolvedOrganizations = a.AlertReports
                        .Select(ar => new
                        {
                            OrganizationId = ar.Report.DataCollector.Supervisor != null
                            ? ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().OrganizationId
                            : ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties.Single().OrganizationId
                        })
                        .Select(x => x.OrganizationId).ToList()
                })
                .SingleOrDefaultAsync();

            var recipients = await _nyssContext.AlertNotificationRecipients
                .Include(ar => ar.GatewayModem)
                .Where(ar =>
                    ar.ProjectId == alert.ProjectId &&
                    alert.InvolvedOrganizations.Contains(ar.OrganizationId) &&
                    (ar.SupervisorAlertRecipients.Count == 0 || ar.SupervisorAlertRecipients.Any(sar => alert.InvolvedSupervisorIds.Contains(sar.SupervisorId))) &&
                    (ar.HeadSupervisorUserAlertRecipients.Count == 0 || ar.HeadSupervisorUserAlertRecipients.Any(sar => alert.InvolvedHeadSupervisorIds.Contains(sar.HeadSupervisorId))) &&
                    (ar.ProjectHealthRiskAlertRecipients.Count == 0 || ar.ProjectHealthRiskAlertRecipients.Any(phr => phr.ProjectHealthRiskId == alert.ProjectHealthRiskId)))
                .ToListAsync();

            return recipients;
        }

        private async Task<bool> HasCurrentUserAlertEditAccess(int alertId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var currentUserOrgs = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id)
                .Select(uns => uns.Organization.Id)
                .ToListAsync();

            if (currentUser.Role == Role.Supervisor)
            {
                return await _nyssContext.Alerts
                    .IgnoreQueryFilters()
                    .Where(a => a.Id == alertId && a.AlertReports.Any(ar => ar.Report.DataCollector.Supervisor.Id == currentUser.Id))
                    .AnyAsync();
            }

            if (currentUser.Role == Role.HeadSupervisor)
            {
                return await _nyssContext.Alerts
                    .IgnoreQueryFilters()
                    .Where(a => a.Id == alertId && a.AlertReports.Any(ar => ar.Report.DataCollector.Supervisor.HeadSupervisor.Id == currentUser.Id))
                    .AnyAsync();
            }

            var organizationHasReportsInAlert = await _nyssContext.Alerts
                .IgnoreQueryFilters()
                .Where(a => a.Id == alertId
                    && (currentUser.Role == Role.Administrator
                        || a.AlertReports.Any(ar => currentUserOrgs.Contains(ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().OrganizationId.Value))
                    )).AnyAsync();

            return organizationHasReportsInAlert;
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

        private async Task SendNotificationSmses(int nationalSocietyId, string languageCode, List<SendSmsRecipient> notificationRecipients, string project, string healthRisk,
            string lastReportVillage)
        {
            try
            {
                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
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
                    await _smsPublisherService.SendSms(gatewaySetting.IotHubDeviceName, notificationRecipients, text, gatewaySetting.Modems.Any());
                }
                else
                {
                    await Task.WhenAll(notificationRecipients
                        .Select(sms => _emailPublisherService.SendEmail((gatewaySetting.EmailAddress, gatewaySetting.EmailAddress), sms.PhoneNumber, text, true)));
                }
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, $"Failed to send escalation notification SMSes for project {project} with health risk {healthRisk}");
                throw new ResultException(ResultKey.Alert.EscalateAlert.SmsNotificationFailed);
            }
        }

        private static Area MapToArea(AreaDto area) =>
            area == null
                ? null
                : new Area
                {
                    AreaType = area.Type,
                    AreaId = area.Id
                };
    }
}
