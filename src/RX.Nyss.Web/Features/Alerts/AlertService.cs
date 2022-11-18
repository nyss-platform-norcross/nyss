using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertService
    {
        Task<AlertAssessmentStatus> GetAssessmentStatus(int alertId);

        Task<bool> HasCurrentUserAlertEditAccess(int alertId);

        Task<List<AlertNotificationRecipient>> GetAlertRecipients(int alertId);
    }

    public class AlertService : IAlertService
    {
        private readonly INyssContext _nyssContext;

        private readonly IAuthorizationService _authorizationService;

        public AlertService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
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


        public async Task<List<AlertNotificationRecipient>> GetAlertRecipients(int alertId)
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

        public async Task<bool> HasCurrentUserAlertEditAccess(int alertId)
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
                        || a.AlertReports.Any(ar => currentUserOrgs.Contains(ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties.Single().OrganizationId.Value))
                    )).AnyAsync();

            return organizationHasReportsInAlert;
        }
    }
}
