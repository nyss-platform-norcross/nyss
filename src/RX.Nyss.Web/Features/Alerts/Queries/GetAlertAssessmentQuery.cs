using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Alerts.Queries
{
    public class GetAlertAssessmentQuery : IRequest<AlertAssessmentResponseDto>
    {
        public GetAlertAssessmentQuery(int alertId)
        {
            AlertId = alertId;
        }

        public int AlertId { get; }

        public int UtcOffset { get; set; }

        public class Handler : IRequestHandler<GetAlertAssessmentQuery, AlertAssessmentResponseDto>
        {
            private readonly IAuthorizationService _authorizationService;

            private readonly INyssContext _nyssContext;

            public Handler(IAuthorizationService authorizationService, INyssContext nyssContext)
            {
                _authorizationService = authorizationService;
                _nyssContext = nyssContext;
            }

            public async Task<AlertAssessmentResponseDto> Handle(GetAlertAssessmentQuery request, CancellationToken cancellationToken)
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                var isManagerOrAbove = _authorizationService.IsCurrentUserInAnyRole(
                    Role.Administrator, Role.Manager, Role.TechnicalAdvisor);

                var isAdmin = _authorizationService.IsCurrentUserInAnyRole(Role.Administrator);

                var currentUserOrganizationIds = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser!.Id)
                    .Select(uns => uns.OrganizationId)
                    .ToListAsync(cancellationToken);

                var alert = await _nyssContext.Alerts
                    .IgnoreQueryFilters()
                    .Where(a => a.Id == request.AlertId)
                    .Select(a => new
                    {
                        a.Status,
                        a.CreatedAt,
                        a.EscalatedAt,
                        a.Comments,
                        a.EscalatedOutcome,
                        a.RecipientsNotifiedAt,
                        a.ProjectHealthRisk.Project.NationalSociety.EnableEidsrIntegration,
                        ProjectId = a.ProjectHealthRisk.Project.Id,
                        ProjectHealthRiskId = a.ProjectHealthRisk.Id,
                        HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                            .Where(lc => a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage != null && lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id )
                            .Select(lc => lc.Name)
                            .FirstOrDefault() ?? string.Empty,   
                        HealthRiskType = a.ProjectHealthRisk.HealthRisk.HealthRiskType,
                        HealthRiskCountThreshold = a.ProjectHealthRisk.AlertRule.CountThreshold,
                        HealthRiskDaysThreshold = a.ProjectHealthRisk.AlertRule.DaysThreshold,
                        HealthRiskKilometersThreshold = a.ProjectHealthRisk.AlertRule.KilometersThreshold,
                        CaseDefinition = a.ProjectHealthRisk.CaseDefinition,

                        Reports = a.AlertReports
                            .Where(ar => ar.Report != null && ar.Report.DataCollector != null)
                            .Select(ar => new
                            {
                                ar.Report.Id,
                                ar.Report.ReceivedAt,
                                ar.Report.PhoneNumber,
                                ar.Report.ReportedCase,
                                ar.Report.Status,
                                ar.Report.AcceptedAt,
                                ar.Report.RejectedAt,
                                ar.Report.ResetAt,

                                DataCollector = ar.Report.DataCollector != null && ar.Report.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                                    ? ar.Report.DataCollector.Name
                                    : ar.Report.DataCollector != null
                                        ? ar.Report.DataCollector.DisplayName
                                        : null,

                                HasSupervisor = ar.Report.DataCollector != null && ar.Report.DataCollector.Supervisor != null,

                                SupervisorId = ar.Report.DataCollector != null && ar.Report.DataCollector.Supervisor != null
                                    ? (int?)ar.Report.DataCollector.Supervisor.Id
                                    : null,
                                HeadSupervisorId = ar.Report.DataCollector != null && ar.Report.DataCollector.HeadSupervisor != null
                                    ? (int?)ar.Report.DataCollector.HeadSupervisor.Id
                                    : null,

                                SupervisorName = ar.Report.DataCollector != null && ar.Report.DataCollector.Supervisor != null
                                    ? ar.Report.DataCollector.Supervisor.Name
                                    : ar.Report.DataCollector != null && ar.Report.DataCollector.HeadSupervisor != null
                                        ? ar.Report.DataCollector.HeadSupervisor.Name
                                        : null,

                                SupervisorPhoneNumber = ar.Report.DataCollector != null && ar.Report.DataCollector.Supervisor != null
                                    ? ar.Report.DataCollector.Supervisor.PhoneNumber
                                    : ar.Report.DataCollector != null && ar.Report.DataCollector.HeadSupervisor != null
                                        ? ar.Report.DataCollector.HeadSupervisor.PhoneNumber
                                        : null,

                                OrganizationId = ar.Report.DataCollector != null && ar.Report.DataCollector.Supervisor != null
                                    ? ar.Report.DataCollector.Supervisor.UserNationalSocieties!
                                        .Select(uns => (int?)uns.OrganizationId)
                                        .FirstOrDefault()
                                    : ar.Report.DataCollector != null && ar.Report.DataCollector.HeadSupervisor != null
                                        ? ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties!
                                            .Select(uns => (int?)uns.OrganizationId)
                                            .FirstOrDefault()
                                        : null,

                                OrganizationName = ar.Report.DataCollector != null && ar.Report.DataCollector.Supervisor != null
                                    ? ar.Report.DataCollector.Supervisor.UserNationalSocieties!
                                        .Select(uns => uns.Organization != null ? uns.Organization.Name : null)
                                        .FirstOrDefault()
                                    : ar.Report.DataCollector != null && ar.Report.DataCollector.HeadSupervisor != null
                                        ? ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties!
                                            .Select(uns => uns.Organization != null ? uns.Organization.Name : null)
                                            .FirstOrDefault()
                                        : null,

                                IsAnonymized =
                                    (currentUser!.Role == Role.Supervisor
                                        && ar.Report.DataCollector != null
                                        && (ar.Report.DataCollector.Supervisor == null
                                            || ar.Report.DataCollector.Supervisor.Id != currentUser.Id))
                                    || (currentUser!.Role == Role.HeadSupervisor
                                        && ar.Report.DataCollector != null
                                        && !((ar.Report.DataCollector.HeadSupervisor != null
                                                && ar.Report.DataCollector.HeadSupervisor.Id == currentUser.Id)
                                            || (ar.Report.DataCollector.Supervisor != null
                                                && ar.Report.DataCollector.Supervisor.HeadSupervisor != null
                                                && ar.Report.DataCollector.Supervisor.HeadSupervisor.Id == currentUser.Id))),

                                Zone = ar.Report.RawReport != null && ar.Report.RawReport.Zone != null
                                    ? ar.Report.RawReport.Zone.Name
                                    : string.Empty,
                                Village = ar.Report.RawReport != null && ar.Report.RawReport.Village != null
                                    ? ar.Report.RawReport.Village.Name
                                    : string.Empty,
                                District = ar.Report.RawReport != null
                                           && ar.Report.RawReport.Village != null
                                           && ar.Report.RawReport.Village.District != null
                                    ? ar.Report.RawReport.Village.District.Name
                                    : string.Empty,
                                Region = ar.Report.RawReport != null
                                         && ar.Report.RawReport.Village != null
                                         && ar.Report.RawReport.Village.District != null
                                         && ar.Report.RawReport.Village.District.Region != null
                                    ? ar.Report.RawReport.Village.District.Region.Name
                                    : string.Empty,
                            }),
                    })
                    .AsNoTracking()
                    .AsSplitQuery()
                    .SingleAsync(cancellationToken);

                var involvedOrganizationIds = alert.Reports
                    .Select(r => r.OrganizationId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                var involvedSupervisorIds = alert.Reports
                    .Select(r => r.SupervisorId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                var involvedHeadSupervisorIds = alert.Reports
                    .Select(r => r.HeadSupervisorId)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .ToList();

                var acceptedReports = alert.Reports.Count(r => r.Status == ReportStatus.Accepted);
                var pendingReports = alert.Reports.Count(r => r.Status == ReportStatus.Pending);

                var escalatedTo = new List<AlertAssessmentNotifiedUser>();

                if (alert.RecipientsNotifiedAt.HasValue && isManagerOrAbove)
                {
                    var recipients = await _nyssContext.AlertNotificationRecipients
                        .Include(r => r.GatewayModem)
                        .Where(r =>
                            r.ProjectId == alert.ProjectId
                            && involvedOrganizationIds.Contains(r.OrganizationId)
                            && (!r.SupervisorAlertRecipients.Any()
                                || r.SupervisorAlertRecipients.Any(sar => involvedSupervisorIds.Contains(sar.SupervisorId)))
                            && (!r.HeadSupervisorUserAlertRecipients.Any()
                                || r.HeadSupervisorUserAlertRecipients.Any(sar => involvedHeadSupervisorIds.Contains(sar.HeadSupervisorId)))
                            && (!r.ProjectHealthRiskAlertRecipients.Any()
                                || r.ProjectHealthRiskAlertRecipients.Any(phr => phr.ProjectHealthRiskId == alert.ProjectHealthRiskId)))
                        .Select(r => new AlertAssessmentNotifiedUser   // project in DB, not in-memory
                        {
                            Role = r.Role,
                            Email = r.Email,
                            Organization = r.Organization,
                            PhoneNumber = r.PhoneNumber,
                        })
                        .ToListAsync(cancellationToken);

                    escalatedTo.AddRange(recipients);
                }

                var reportDtos = alert.Reports
                    .Select(ar =>
                    {
                        var canSeeFullData = isAdmin || currentUserOrganizationIds.Contains(ar.OrganizationId ?? -1);

                        if (!canSeeFullData)
                        {
                            return new AlertAssessmentResponseDto.ReportDto
                            {
                                Id = ar.Id,
                                ReceivedAt = ar.ReceivedAt.AddHours(request.UtcOffset),
                                Status = ar.Status.ToString(),
                                Organization = ar.OrganizationName ?? string.Empty,
                                IsAnonymized = ar.IsAnonymized,
                            };
                        }

                        return new AlertAssessmentResponseDto.ReportDto
                        {
                            Id = ar.Id,
                            DataCollector = ar.IsAnonymized
                                ? ar.SupervisorName ?? string.Empty
                                : ar.DataCollector ?? string.Empty,
                            ReceivedAt = ar.ReceivedAt.AddHours(request.UtcOffset),
                            PhoneNumber = ar.IsAnonymized ? "***" : ar.PhoneNumber ?? string.Empty,
                            Status = ar.Status.ToString(),
                            Zone = ar.Zone ?? string.Empty,
                            Village = ar.Village ?? string.Empty,
                            District = ar.District,
                            Region = ar.Region,
                            Sex = alert.HealthRiskType == HealthRiskType.Human
                                ? ar.ReportedCase?.GetSex() ?? string.Empty
                                : string.Empty,
                            Age = alert.HealthRiskType == HealthRiskType.Human
                                ? ar.ReportedCase?.GetAge() ?? string.Empty
                                : string.Empty,
                            IsAnonymized = ar.IsAnonymized,
                            AcceptedAt = ar.AcceptedAt?.AddHours(request.UtcOffset),
                            RejectedAt = ar.RejectedAt?.AddHours(request.UtcOffset),
                            ResetAt = ar.ResetAt?.AddHours(request.UtcOffset),
                            SupervisorName = ar.SupervisorName ?? string.Empty,
                            SupervisorPhoneNumber = ar.SupervisorPhoneNumber ?? string.Empty,
                        };
                    })
                    .ToList();

                return new AlertAssessmentResponseDto
                {
                    HealthRisk = alert.HealthRisk,
                    Comments = alert.Comments ?? string.Empty,      
                    CreatedAt = alert.CreatedAt.AddHours(request.UtcOffset),
                    EscalatedAt = alert.EscalatedAt?.AddHours(request.UtcOffset),
                    CaseDefinition = alert.CaseDefinition ?? string.Empty, 
                    HealthRiskCountThreshold = alert.HealthRiskCountThreshold,
                    HealthRiskDaysThreshold = alert.HealthRiskDaysThreshold,
                    HealthRiskKilometersThreshold = alert.HealthRiskKilometersThreshold,
                    AssessmentStatus = alert.Status.GetAssessmentStatus(acceptedReports, pendingReports, alert.HealthRiskCountThreshold),
                    EscalatedOutcome = alert.EscalatedOutcome,
                    RecipientsNotified = alert.RecipientsNotifiedAt.HasValue,
                    EscalatedTo = escalatedTo,
                    IsNationalSocietyEidsrEnabled = alert.EnableEidsrIntegration,
                    Reports = reportDtos,
                };
            }
        }
    }
}