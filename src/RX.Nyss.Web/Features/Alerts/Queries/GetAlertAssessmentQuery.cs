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
                var escalatedTo = new List<AlertAssessmentNotifiedUser>();

                var userOrganizations = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser.Id)
                    .Select(uns => uns.Organization)
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
                        ProjectId = a.ProjectHealthRisk.Project.Id,
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
                            SupervisorPhoneNumber = ar.Report.DataCollector.Supervisor.PhoneNumber,
                            ReceivedAt = ar.Report.ReceivedAt,
                            PhoneNumber = ar.Report.PhoneNumber,
                            Village = ar.Report.RawReport.Village.Name,
                            District = ar.Report.RawReport.Village.District.Name,
                            Region = ar.Report.RawReport.Village.District.Region.Name,
                            ReportedCase = ar.Report.ReportedCase,
                            Status = ar.Report.Status,
                            AcceptedAt = ar.Report.AcceptedAt,
                            RejectedAt = ar.Report.RejectedAt,
                            ResetAt = ar.Report.ResetAt,
                        }),
                        InvolvedOrganizations = a.AlertReports
                            .Select(ar => new
                            {
                                OrganizationId = ar.Report.DataCollector.Supervisor != null
                                    ? ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().OrganizationId
                                    : ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties.Single().OrganizationId
                            })
                            .Select(x => x.OrganizationId).ToList(),
                        ProjectHealthRiskId = a.ProjectHealthRisk.Id,
                        InvolvedSupervisorIds = a.AlertReports
                            .Where(ar => ar.Report.DataCollector.Supervisor != null)
                            .Select(ar => ar.Report.DataCollector.Supervisor.Id)
                            .ToList(),
                        InvolvedHeadSupervisorIds = a.AlertReports
                            .Where(ar => ar.Report.DataCollector.HeadSupervisor != null)
                            .Select(ar => ar.Report.DataCollector.HeadSupervisor.Id)
                            .ToList(),
                    })
                    .AsNoTracking()
                    .SingleAsync(cancellationToken);

                var acceptedReports = alert.Reports.Count(r => r.Status == ReportStatus.Accepted);
                var pendingReports = alert.Reports.Count(r => r.Status == ReportStatus.Pending);
                var currentUserCanSeeEveryoneData = _authorizationService.IsCurrentUserInAnyRole(Role.Administrator);

                if (alert.RecipientsNotifiedAt.HasValue)
                {
                    var recipients = await _nyssContext.AlertNotificationRecipients
                        .Include(ar => ar.GatewayModem)
                        .Where(ar =>
                            ar.ProjectId == alert.ProjectId &&
                            alert.InvolvedOrganizations.Contains(ar.OrganizationId) &&
                            (ar.SupervisorAlertRecipients.Count == 0 || ar.SupervisorAlertRecipients.Any(sar => alert.InvolvedSupervisorIds.Contains(sar.SupervisorId))) &&
                            (ar.HeadSupervisorUserAlertRecipients.Count == 0 || ar.HeadSupervisorUserAlertRecipients.Any(sar => alert.InvolvedHeadSupervisorIds.Contains(sar.HeadSupervisorId))) &&
                            (ar.ProjectHealthRiskAlertRecipients.Count == 0 || ar.ProjectHealthRiskAlertRecipients.Any(phr => phr.ProjectHealthRiskId == alert.ProjectHealthRiskId)))
                        .ToListAsync(cancellationToken);

                    escalatedTo.AddRange(recipients.Select(r => new AlertAssessmentNotifiedUser
                    {
                        Role = r.Role,
                        Email = r.Email,
                        Organization = r.Organization,
                        PhoneNumber = r.PhoneNumber,
                    }));
                }

                return new AlertAssessmentResponseDto
                {
                    HealthRisk = alert.HealthRisk,
                    Comments = alert.Comments,
                    CreatedAt = alert.CreatedAt.AddHours(request.UtcOffset),
                    EscalatedAt = alert.EscalatedAt?.AddHours(request.UtcOffset),
                    CaseDefinition = alert.CaseDefinition,
                    AssessmentStatus = alert.Status.GetAssessmentStatus(acceptedReports, pendingReports, alert.HealthRiskCountThreshold),
                    EscalatedOutcome = alert.EscalatedOutcome,
                    RecipientsNotified = alert.RecipientsNotifiedAt.HasValue,
                    EscalatedTo = escalatedTo,
                    Reports = alert.Reports.Select(ar => currentUserCanSeeEveryoneData || userOrganizations.Any(uo => ar.OrganizationId == uo.Id)
                        ? new AlertAssessmentResponseDto.ReportDto
                        {
                            Id = ar.Id,
                            DataCollector = ar.IsAnonymized
                                ? ar.SupervisorName
                                : ar.DataCollector,
                            ReceivedAt = ar.ReceivedAt.AddHours(request.UtcOffset),
                            PhoneNumber = ar.IsAnonymized
                                ? "***"
                                : ar.PhoneNumber,
                            Status = ar.Status.ToString(),
                            Village = ar.Village,
                            District = ar.District,
                            Region = ar.Region,
                            Sex = ar.ReportedCase.GetSex(),
                            Age = ar.ReportedCase.GetAge(),
                            IsAnonymized = ar.IsAnonymized,
                            AcceptedAt = ar.AcceptedAt?.AddHours(request.UtcOffset),
                            RejectedAt = ar.RejectedAt?.AddHours(request.UtcOffset),
                            ResetAt = ar.ResetAt?.AddHours(request.UtcOffset),
                            SupervisorName = ar.SupervisorName,
                            SupervisorPhoneNumber = ar.SupervisorPhoneNumber
                        }
                        : new AlertAssessmentResponseDto.ReportDto
                        {
                            Id = ar.Id,
                            ReceivedAt = ar.ReceivedAt.AddHours(request.UtcOffset),
                            Status = ar.Status.ToString(),
                            Organization = ar.OrganizationName,
                            IsAnonymized = ar.IsAnonymized
                        }).ToList()
                };
            }
        }
    }
}
