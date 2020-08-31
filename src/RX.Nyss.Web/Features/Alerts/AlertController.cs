using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Alerts
{
    [Route("api/alert")]
    public class AlertController : BaseController
    {
        private readonly IAlertService _alertService;
        private readonly IAlertReportService _alertReportService;

        public AlertController(
            IAlertService alertService,
            IAlertReportService alertReportService)
        {
            _alertService = alertService;
            _alertReportService = alertReportService;
        }

        /// <summary>
        /// Lists alerts for a specific project
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="pageNumber">Page number</param>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber) =>
            _alertService.List(projectId, pageNumber);

        /// <summary>
        /// Gets information about the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/get")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AlertAssessmentResponseDto>> Get(int alertId) =>
            _alertService.Get(alertId);


        /// <summary>
        /// Gets information about the alert's notification recipients
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/alertRecipients")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AlertRecipientsResponseDto>> GetAlertRecipients(int alertId) =>
            _alertService.GetAlertRecipientsByAlertId(alertId);

        /// <summary>
        /// Accepts the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpPost("{alertId:int}/acceptReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId) =>
            _alertReportService.AcceptReport(alertId, reportId);

        /// <summary>
        /// Dismisses the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpPost("{alertId:int}/dismissReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId) =>
            _alertReportService.DismissReport(alertId, reportId);

        /// <summary>
        /// Resets the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpPost("{alertId:int}/resetReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<ResetReportResponseDto>> ResetReport(int alertId, int reportId) =>
            _alertReportService.ResetReport(alertId, reportId);

        /// <summary>
        /// Escalates the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="dto">A parameter enabling/disabling notifications</param>
        [HttpPost("{alertId:int}/escalate")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result> Escalate(int alertId, [FromBody] EscalateAlertRequestDto dto) =>
            _alertService.Escalate(alertId, dto.SendNotification);

        /// <summary>
        /// Dismisses the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpPost("{alertId:int}/dismiss")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result> Dismiss(int alertId) =>
            _alertService.Dismiss(alertId);

        /// <summary>
        /// Closes the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="dto">Details related to closing process</param>
        [HttpPost("{alertId:int}/close")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result> Close(int alertId, [FromBody] CloseAlertRequestDto dto) =>
            _alertService.Close(alertId, dto.Comments, dto.CloseOption);

        /// <summary>
        /// Retrieves the alert actions' log
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/getLogs")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AlertLogResponseDto>> GetLogs(int alertId) =>
            _alertService.GetLogs(alertId);
    }
}
