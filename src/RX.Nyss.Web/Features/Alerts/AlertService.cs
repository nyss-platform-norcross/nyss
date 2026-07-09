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

            return alertData.Status.GetAssessmentStatus(
                alertData.AcceptedReports,
                alertData.PendingReports,
                alertData.CountThreshold);
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
                        .Where(ar => ar.Report.DataCollector != null
                            && ar.Report.DataCollector.Supervisor != null)
                        .Select(ar => ar.Report.DataCollector!.Supervisor!.Id)
                        .ToList(),
                    InvolvedHeadSupervisorIds = a.AlertReports
                        .Where(ar => ar.Report.DataCollector != null
                            && ar.Report.DataCollector.HeadSupervisor != null)
                        .Select(ar => ar.Report.DataCollector!.HeadSupervisor!.Id)
                        .ToList(),
                    OrganizationsFromSupervisors = a.AlertReports
                        .Where(ar => ar.Report.DataCollector != null
                            && ar.Report.DataCollector.Supervisor != null
                            && ar.Report.DataCollector.Supervisor.UserNationalSocieties != null)
                        .SelectMany(ar => ar.Report.DataCollector!.Supervisor!.UserNationalSocieties!
                            .Where(uns => uns.OrganizationId != null)
                            .Select(uns => uns.OrganizationId!.Value))
                        .ToList(),
                    OrganizationsFromHeadSupervisors = a.AlertReports
                        .Where(ar => ar.Report.DataCollector != null
                            && ar.Report.DataCollector.HeadSupervisor != null
                            && ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties != null)
                        .SelectMany(ar => ar.Report.DataCollector!.HeadSupervisor!.UserNationalSocieties!
                            .Where(uns => uns.OrganizationId != null)
                            .Select(uns => uns.OrganizationId!.Value))
                        .ToList()
                })
                .SingleOrDefaultAsync();

            if (alert == null)
            {
                return new List<AlertNotificationRecipient>();
            }

            // Merge the two organization lists client-side (both are already in memory)
            var involvedOrganizations = alert.OrganizationsFromSupervisors
                .Union(alert.OrganizationsFromHeadSupervisors)
                .ToList();

            var recipients = await _nyssContext.AlertNotificationRecipients
                .Include(ar => ar.GatewayModem)
                .Where(ar =>
                    ar.ProjectId == alert.ProjectId &&
                    involvedOrganizations.Contains(ar.OrganizationId) &&
                    (ar.SupervisorAlertRecipients.Count == 0 ||
                        ar.SupervisorAlertRecipients.Any(sar => alert.InvolvedSupervisorIds.Contains(sar.SupervisorId))) &&
                    (ar.HeadSupervisorUserAlertRecipients.Count == 0 ||
                        ar.HeadSupervisorUserAlertRecipients.Any(sar => alert.InvolvedHeadSupervisorIds.Contains(sar.HeadSupervisorId))) &&
                    (ar.ProjectHealthRiskAlertRecipients.Count == 0 ||
                        ar.ProjectHealthRiskAlertRecipients.Any(phr => phr.ProjectHealthRiskId == alert.ProjectHealthRiskId)))
                .AsSplitQuery()
                .ToListAsync();

            return recipients;
        }

        public async Task<bool> HasCurrentUserAlertEditAccess(int alertId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            if (currentUser == null)
            {
                return false;
            }

            if (currentUser.Role == Role.Administrator)
            {
                return true;
            }

            if (currentUser.Role == Role.Supervisor)
            {
                return await _nyssContext.Alerts
                    .IgnoreQueryFilters()
                    .Where(a => a.Id == alertId
                        && a.AlertReports.Any(ar =>
                            ar.Report.DataCollector != null
                            && ar.Report.DataCollector.Supervisor != null
                            && ar.Report.DataCollector.Supervisor.Id == currentUser.Id))
                    .AnyAsync();
            }

            if (currentUser.Role == Role.HeadSupervisor)
            {
                return await _nyssContext.Alerts
                    .IgnoreQueryFilters()
                    .Where(a => a.Id == alertId
                        && a.AlertReports.Any(ar =>
                            ar.Report.DataCollector != null
                            && ((ar.Report.DataCollector.HeadSupervisor != null
                                    && ar.Report.DataCollector.HeadSupervisor.Id == currentUser.Id)
                                || (ar.Report.DataCollector.Supervisor != null
                                    && ar.Report.DataCollector.Supervisor.HeadSupervisor != null
                                    && ar.Report.DataCollector.Supervisor.HeadSupervisor.Id == currentUser.Id))))
                    .AnyAsync();
            }

            var currentUserOrgs = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id && uns.OrganizationId != null)
                .Select(uns => uns.OrganizationId!.Value)
                .ToListAsync();

            if (!currentUserOrgs.Any())
            {
                return false;
            }

            return await _nyssContext.Alerts
                .IgnoreQueryFilters()
                .Where(a => a.Id == alertId
                    && (a.AlertReports.Any(ar =>
                            ar.Report.DataCollector != null
                            && ar.Report.DataCollector.Supervisor != null
                            && ar.Report.DataCollector.Supervisor.UserNationalSocieties != null
                            && ar.Report.DataCollector.Supervisor.UserNationalSocieties
                                .Any(uns => uns.OrganizationId != null
                                    && currentUserOrgs.Contains(uns.OrganizationId.Value)))
                        || a.AlertReports.Any(ar =>
                            ar.Report.DataCollector != null
                            && ar.Report.DataCollector.HeadSupervisor != null
                            && ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties != null
                            && ar.Report.DataCollector.HeadSupervisor.UserNationalSocieties
                                .Any(uns => uns.OrganizationId != null
                                    && currentUserOrgs.Contains(uns.OrganizationId.Value)))))
                .AnyAsync();
        }
    }
}