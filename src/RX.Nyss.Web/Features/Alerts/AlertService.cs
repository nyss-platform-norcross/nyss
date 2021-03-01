using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Extensions;
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
using RX.Nyss.Web.Features.Users;
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
        Task<Result<AlertAssessmentResponseDto>> Get(int alertId, int utcOffset);
        Task<Result> Escalate(int alertId, bool sendNotification);
        Task<Result> Dismiss(int alertId);
        Task<Result> Close(int alertId, string comments, EscalatedAlertOutcomes escalatedOutcome);
        Task<AlertAssessmentStatus> GetAssessmentStatus(int alertId);
        Task<Result<AlertLogResponseDto>> GetLogs(int alertId, int utcOffset);
        Task<Result<AlertRecipientsResponseDto>> GetAlertRecipientsByAlertId(int alertId);
        Task<Result<AlertListFilterResponseDto>> GetFiltersData(int projectId);
        Task<byte[]> Export(int projectId, AlertListFilterRequestDto filterRequestDto);
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
        private readonly IProjectService _projectService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IUserService _userService;

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
            IStringsResourcesService stringsResourcesService,
            IUserService userService)
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
            _stringsResourcesService = stringsResourcesService;
            _userService = userService;
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

        public async Task<Result<AlertAssessmentResponseDto>> Get(int alertId, int utcOffset)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var userOrganizations = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id)
                .Select(uns => uns.Organization)
                .ToListAsync();

            var alert = await _nyssContext.Alerts
                .IgnoreQueryFilters()
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    EscalatedAt = a.EscalatedAt,
                    Comments = a.Comments,
                    EscalatedOutcome = a.EscalatedOutcome,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    HealthRiskCountThreshold = a.ProjectHealthRisk.AlertRule.CountThreshold,
                    CaseDefinition = a.ProjectHealthRisk.CaseDefinition,
                    Reports = a.AlertReports.Select(ar => new
                    {
                        Id = ar.Report.Id,
                        DataCollector = ar.Report.DataCollector.DisplayName,
                        OrganizationId = ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().OrganizationId,
                        OrganizationName = ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().Organization.Name,
                        IsAnonymized = (currentUser.Role == Role.Supervisor && ar.Report.DataCollector.Supervisor.Id != currentUser.Id)
                            || (currentUser.Role == Role.HeadSupervisor && ar.Report.DataCollector.Supervisor.HeadSupervisor.Id != currentUser.Id),
                        SupervisorName = ar.Report.DataCollector.Supervisor.Name,
                        ReceivedAt = ar.Report.ReceivedAt,
                        PhoneNumber = ar.Report.PhoneNumber,
                        Village = ar.Report.RawReport.Village.Name,
                        District = ar.Report.RawReport.Village.District.Name,
                        Region = ar.Report.RawReport.Village.District.Region.Name,
                        ReportedCase = ar.Report.ReportedCase,
                        Status = ar.Report.Status,
                        AcceptedAt = ar.Report.AcceptedAt,
                        RejectedAt = ar.Report.RejectedAt,
                        ResetAt = ar.Report.ResetAt
                    })
                })
                .AsNoTracking()
                .SingleAsync();

            var acceptedReports = alert.Reports.Count(r => r.Status == ReportStatus.Accepted);
            var pendingReports = alert.Reports.Count(r => r.Status == ReportStatus.Pending);
            var currentUserCanSeeEveryoneData = _authorizationService.IsCurrentUserInAnyRole(Role.Administrator);

            var dto = new AlertAssessmentResponseDto
            {
                HealthRisk = alert.HealthRisk,
                Comments = alert.Comments,
                CreatedAt = alert.CreatedAt.AddHours(utcOffset),
                EscalatedAt = alert.EscalatedAt?.AddHours(utcOffset),
                CaseDefinition = alert.CaseDefinition,
                AssessmentStatus = GetAssessmentStatus(alert.Status, acceptedReports, pendingReports, alert.HealthRiskCountThreshold),
                EscalatedOutcome = alert.EscalatedOutcome,
                Reports = alert.Reports.Select(ar => currentUserCanSeeEveryoneData || userOrganizations.Any(uo => ar.OrganizationId == uo.Id)
                    ? new AlertAssessmentResponseDto.ReportDto
                    {
                        Id = ar.Id,
                        DataCollector = ar.IsAnonymized
                            ? ar.SupervisorName
                            : ar.DataCollector,
                        ReceivedAt = ar.ReceivedAt.AddHours(utcOffset),
                        PhoneNumber = ar.IsAnonymized
                            ? "***"
                            : ar.PhoneNumber,
                        Status = ar.Status.ToString(),
                        Village = ar.Village,
                        District = ar.District,
                        Region = ar.Region,
                        Sex = GetSex(ar.ReportedCase),
                        Age = GetAge(ar.ReportedCase),
                        IsAnonymized = ar.IsAnonymized,
                        AcceptedAt = ar.AcceptedAt?.AddHours(utcOffset),
                        RejectedAt = ar.RejectedAt?.AddHours(utcOffset),
                        ResetAt = ar.ResetAt?.AddHours(utcOffset)
                    }
                    : new AlertAssessmentResponseDto.ReportDto
                    {
                        Id = ar.Id,
                        ReceivedAt = ar.ReceivedAt.AddHours(utcOffset),
                        Status = ar.Status.ToString(),
                        Organization = ar.OrganizationName,
                        IsAnonymized = ar.IsAnonymized
                    }).ToList()
            };

            return Success(dto);
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

        public async Task<Result> Close(int alertId, string comments, EscalatedAlertOutcomes escalatedOutcome)
        {
            if (!await HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.CloseAlert.NoPermission);
            }

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

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

            if (alertData.Alert.Status != AlertStatus.Escalated)
            {
                return Error(ResultKey.Alert.CloseAlert.WrongStatus);
            }

            alertData.Alert.Status = AlertStatus.Closed;
            alertData.Alert.ClosedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.ClosedBy = await _authorizationService.GetCurrentUser();
            alertData.Alert.EscalatedOutcome = escalatedOutcome;
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

        public async Task<Result<AlertLogResponseDto>> GetLogs(int alertId, int utcOffset)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentUserOrganization = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id && uns.NationalSociety == _nyssContext.Alerts
                    .Where(a => a.Id == alertId)
                    .Select(a => a.ProjectHealthRisk.Project.NationalSociety)
                    .Single())
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            var alert = await _nyssContext.Alerts
                .IgnoreQueryFilters()
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    a.CreatedAt,
                    a.EscalatedAt,
                    a.DismissedAt,
                    a.ClosedAt,
                    a.EscalatedOutcome,
                    a.Comments,
                    EscalatedBy = a.EscalatedBy.DeletedAt.HasValue ||
                        (currentUser.Role != Role.Administrator &&
                            currentUserOrganization != a.EscalatedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                            ? a.EscalatedBy.UserNationalSocieties.Single(eduns => eduns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                            : a.EscalatedBy.Name,
                    DismissedBy = a.DismissedBy.DeletedAt.HasValue ||
                        (currentUser.Role != Role.Administrator &&
                            currentUserOrganization != a.DismissedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                            ? a.DismissedBy.UserNationalSocieties.Single(dbuns => dbuns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                            : a.DismissedBy.Name,
                    ClosedBy = a.ClosedBy.DeletedAt.HasValue ||
                        (currentUser.Role != Role.Administrator &&
                            currentUserOrganization != a.ClosedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                            ? a.ClosedBy.UserNationalSocieties.Single(cbuns => cbuns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                            : a.ClosedBy.Name,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    Reports = a.AlertReports
                        .Where(ar => ar.Report.Status == ReportStatus.Accepted || ar.Report.Status == ReportStatus.Rejected || ar.Report.ResetAt.HasValue)
                        .Select(ar => new
                        {
                            ar.ReportId,
                            ar.Report.AcceptedAt,
                            ar.Report.RejectedAt,
                            ar.Report.ResetAt,
                            AcceptedBy = ar.Report.AcceptedBy.DeletedAt.HasValue ||
                                (currentUser.Role != Role.Administrator &&
                                    currentUserOrganization != ar.Report.AcceptedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                        .Organization)
                                    ? ar.Report.AcceptedBy.UserNationalSocieties.Single(abuns => abuns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                                    : ar.Report.AcceptedBy.Name,
                            RejectedBy = ar.Report.RejectedBy.DeletedAt.HasValue ||
                                (currentUser.Role != Role.Administrator &&
                                    currentUserOrganization != ar.Report.RejectedBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety)
                                        .Organization)
                                    ? ar.Report.RejectedBy.UserNationalSocieties.Single(rejecteduns => rejecteduns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                                    : ar.Report.RejectedBy.Name,
                            ResetBy = ar.Report.ResetBy.DeletedAt.HasValue ||
                                (currentUser.Role != Role.Administrator &&
                                    currentUserOrganization != ar.Report.ResetBy.UserNationalSocieties.Single(uns => uns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization)
                                    ? ar.Report.ResetBy.UserNationalSocieties.Single(resetuns => resetuns.NationalSociety == a.ProjectHealthRisk.Project.NationalSociety).Organization.Name
                                    : ar.Report.ResetBy.Name
                        })
                        .ToList()
                })
                .SingleAsync();

            var list = new List<AlertLogResponseDto.Item> { new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.TriggeredAlert, alert.CreatedAt.AddHours(utcOffset), null) };

            if (alert.EscalatedAt.HasValue)
            {
                list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.EscalatedAlert, alert.EscalatedAt.Value.AddHours(utcOffset), alert.EscalatedBy));
            }

            if (alert.DismissedAt.HasValue)
            {
                list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.DismissedAlert, alert.DismissedAt.Value.AddHours(utcOffset), alert.DismissedBy));
            }

            if (alert.ClosedAt.HasValue)
            {
                list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.ClosedAlert, alert.ClosedAt.Value.AddHours(utcOffset), alert.ClosedBy, new
                {
                    alert.EscalatedOutcome,
                    alert.Comments
                }));
            }

            foreach (var report in alert.Reports)
            {
                if (report.AcceptedAt.HasValue)
                {
                    list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.AcceptedReport, report.AcceptedAt.Value.AddHours(utcOffset), report.AcceptedBy, new { report.ReportId }));
                }

                if (report.RejectedAt.HasValue)
                {
                    list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.RejectedReport, report.RejectedAt.Value.AddHours(utcOffset), report.RejectedBy, new { report.ReportId }));
                }

                if (report.ResetAt.HasValue)
                {
                    list.Add(new AlertLogResponseDto.Item(AlertLogResponseDto.LogType.ResetReport, report.ResetAt.Value.AddHours(utcOffset), report.ResetBy, new { report.ReportId }));
                }
            }

            return Success(new AlertLogResponseDto
            {
                HealthRisk = alert.HealthRisk,
                CreatedAt = alert.CreatedAt.AddHours(utcOffset),
                Items = list.OrderBy(x => x.Date).ToList()
            });
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

            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUserName);
            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguageCode)).Value;

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
                        .Single()
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
                    Status = GetStringResource(stringResources, $"alerts.alertStatus.{a.Status.ToString().ToLower()}"),
                    EscalatedOutcome = a.EscalatedOutcome,
                    Comments = a.Comments,
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReport.IsAnonymized
                        ? ""
                        : a.LastReport.VillageName,
                    LastReportDistrict = a.LastReport.DistrictName,
                    LastReportRegion = a.LastReport.RegionName,
                    HealthRisk = a.HealthRisk
                }).ToList();

            var documentTitle = GetStringResource(stringResources, "alerts.export.title");
            var columnLabels = GetColumnLabels(stringResources);
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

        private List<string> GetColumnLabels(IDictionary<string, StringResourceValue> stringResources) =>
            new List<string>
            {
                GetStringResource(stringResources, "alerts.export.id"),
                GetStringResource(stringResources, "alerts.export.timeTriggered"),
                GetStringResource(stringResources, "alerts.export.timeOfLastReport"),
                GetStringResource(stringResources, "alerts.export.healthRisk"),
                GetStringResource(stringResources, "alerts.export.reports"),
                GetStringResource(stringResources, "alerts.export.status"),
                GetStringResource(stringResources, "alerts.export.lastReportRegion"),
                GetStringResource(stringResources, "alerts.export.lastReportDistrict"),
                GetStringResource(stringResources, "alerts.export.lastReportVillage"),
                GetStringResource(stringResources, "alerts.export.lastReportZone"),
                GetStringResource(stringResources, "alerts.export.timeEscalated"),
                GetStringResource(stringResources, "alerts.export.timeClosed"),
                GetStringResource(stringResources, "alerts.export.timeDismissed"),
                GetStringResource(stringResources, "alerts.export.escalatedOutcome"),
                GetStringResource(stringResources, "alerts.export.closedComments")
            };

        private static string GetStringResource(IDictionary<string, StringResourceValue> stringResources, string key) =>
            stringResources.Keys.Contains(key)
                ? stringResources[key].Value
                : key;

        private async Task<List<AlertNotificationRecipient>> GetAlertRecipients(int alertId)
        {
            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(a => new
                {
                    ProjectId = a.ProjectHealthRisk.Project.Id,
                    ProjectHealthRiskId = a.ProjectHealthRisk.Id,
                    InvolvedSupervisors = a.AlertReports.Select(ar => ar.Report.DataCollector.Supervisor.Id).Distinct().ToList(),
                    InvolvedOrganizations = a.AlertReports
                        .Select(ar => ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().OrganizationId).Distinct().ToList()
                })
                .SingleOrDefaultAsync();

            var recipients = await _nyssContext.AlertNotificationRecipients
                .Include(ar => ar.GatewayModem)
                .Where(ar =>
                    ar.ProjectId == alert.ProjectId &&
                    alert.InvolvedOrganizations.Contains(ar.OrganizationId) &&
                    (ar.SupervisorAlertRecipients.Count == 0 || ar.SupervisorAlertRecipients.Any(sar => alert.InvolvedSupervisors.Contains(sar.SupervisorId))) &&
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

        private static AlertAssessmentStatus GetAssessmentStatus(AlertStatus alertStatus, int acceptedReports, int pendingReports, int countThreshold) =>
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
