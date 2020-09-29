using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertReportService
    {
        Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId);
        Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId);
        Task<Result<ResetReportResponseDto>> ResetReport(int alertId, int reportId);
    }

    public class AlertReportService : IAlertReportService
    {
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IAlertService _alertService;
        private readonly IQueueService _queueService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;

        public AlertReportService(
            INyssWebConfig config,
            INyssContext nyssContext,
            IAlertService alertService,
            IQueueService queueService,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService)
        {
            _config = config;
            _nyssContext = nyssContext;
            _alertService = alertService;
            _queueService = queueService;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
        }

        public async Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId)
        {
            if (!await HasCurrentUserReportAssessAccess(reportId))
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReport.NoPermission);
            }

            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (!GetAlertHasStatusThatAllowsReportCrossChecks(alertReport))
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReport.WrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Pending)
            {
                return Error<AcceptReportResponseDto>(ResultKey.Alert.AcceptReport.WrongReportStatus);
            }

            alertReport.Report.Status = ReportStatus.Accepted;
            alertReport.Report.AcceptedAt = _dateTimeProvider.UtcNow;
            alertReport.Report.AcceptedBy = _authorizationService.GetCurrentUser();
            await _nyssContext.SaveChangesAsync();

            var response = new AcceptReportResponseDto { AssessmentStatus = await _alertService.GetAssessmentStatus(alertId) };

            return Success(response);
        }

        public async Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId)
        {
            if (!await HasCurrentUserReportAssessAccess(reportId))
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.DismissReport.NoPermission);
            }

            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (!GetAlertHasStatusThatAllowsReportCrossChecks(alertReport))
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.DismissReport.WrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Pending)
            {
                return Error<DismissReportResponseDto>(ResultKey.Alert.DismissReport.WrongReportStatus);
            }

            alertReport.Report.Status = ReportStatus.Rejected;
            alertReport.Report.RejectedAt = _dateTimeProvider.UtcNow;
            alertReport.Report.RejectedBy = _authorizationService.GetCurrentUser();

            await DismissAlertReport(reportId);

            await _nyssContext.SaveChangesAsync();

            var response = new DismissReportResponseDto { AssessmentStatus = await _alertService.GetAssessmentStatus(alertId) };

            return Success(response);
        }

        public async Task<Result<ResetReportResponseDto>> ResetReport(int alertId, int reportId)
        {
            if (!await HasCurrentUserReportAssessAccess(reportId))
            {
                return Error<ResetReportResponseDto>(ResultKey.Alert.ResetReport.NoPermission);
            }

            var alertReport = await _nyssContext.AlertReports
                .Include(ar => ar.Alert)
                .Include(ar => ar.Report)
                .Where(ar => ar.AlertId == alertId && ar.ReportId == reportId)
                .SingleAsync();

            if (!GetAlertHasStatusThatAllowsReportCrossChecks(alertReport))
            {
                return Error<ResetReportResponseDto>(ResultKey.Alert.ResetReport.WrongAlertStatus);
            }

            if (alertReport.Report.Status != ReportStatus.Accepted && alertReport.Report.Status != ReportStatus.Rejected)
            {
                return Error<ResetReportResponseDto>(ResultKey.Alert.ResetReport.WrongReportStatus);
            }

            var reportUpdatedTime = alertReport.Report.Status == ReportStatus.Accepted ? alertReport.Report.AcceptedAt : alertReport.Report.RejectedAt;

            if (alertReport.Alert.Status == AlertStatus.Escalated && reportUpdatedTime < alertReport.Alert.EscalatedAt)
            {
                return Error<ResetReportResponseDto>(ResultKey.Alert.ResetReport.ReportWasCrossCheckedBeforeAlertEscalation);
            }

            alertReport.Report.ResetAt = _dateTimeProvider.UtcNow;
            alertReport.Report.ResetBy = _authorizationService.GetCurrentUser();
            
            await _nyssContext.SaveChangesAsync();

            await ResetAlertReport(reportId);

            var response = new ResetReportResponseDto { AssessmentStatus = await _alertService.GetAssessmentStatus(alertId) };

            return Success(response);
        }

        private async Task<bool> HasCurrentUserReportAssessAccess(int reportId)
        {
            var currentUser = await _authorizationService.GetCurrentUserAsync();

            var userOrganizations = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id)
                .Select(uns => uns.Organization.Id)
                .ToListAsync();

            var canAssess = await _nyssContext.AlertReports
                .IgnoreQueryFilters()
                .Where(ar =>
                    ar.ReportId == reportId
                    && (currentUser.Role == Role.Administrator
                        || userOrganizations.Contains(ar.Report.DataCollector.Supervisor.UserNationalSocieties.Single().OrganizationId.Value)))
                .AnyAsync();
            return canAssess;
        }

        private static bool GetAlertHasStatusThatAllowsReportCrossChecks(AlertReport alertReport) =>
            StatusConstants.AlertStatusesAllowingCrossChecks.Contains(alertReport.Alert.Status);

        private Task DismissAlertReport(int reportId)
        {
            var message = new DismissReportMessage { ReportId = reportId };

            return _queueService.Send(_config.ServiceBusQueues.ReportDismissalQueue, message);
        }

        private Task ResetAlertReport(int reportId)
        {
            var message = new ResetReportMessage { ReportId = reportId };

            return _queueService.Send(_config.ServiceBusQueues.ReportResetQueue, message);
        }
    }
}
