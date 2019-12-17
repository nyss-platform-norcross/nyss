using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Authentication.Policies;
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
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber) =>
            _alertService.List(projectId, pageNumber);

        /// <summary>
        /// Gets information about the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/get")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AlertAssessmentResponseDto>> GetAlert(int alertId) =>
            _alertService.GetAlert(alertId);

        /// <summary>
        /// Accepts the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpGet("{alertId:int}/acceptReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId) =>
            _alertReportService.AcceptReport(alertId, reportId);

        /// <summary>
        /// Dismisses the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpGet("{alertId:int}/dismissReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId) =>
            _alertReportService.DismissReport(alertId, reportId);

        /// <summary>
        /// Escalates the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/escalate")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result> EscalateAlert(int alertId) =>
            _alertService.EscalateAlert(alertId);

        /// <summary>
        /// Dismisses the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/dismiss")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result> DismissAlert(int alertId) =>
            _alertService.DismissAlert(alertId);

        /// <summary>
        /// Closes the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="dto">Details related to closing process</param>
        [HttpPost("{alertId:int}/close")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.DataConsumer, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result> CloseAlert(int alertId, [FromBody]CloseAlertRequestDto dto) =>
            _alertService.CloseAlert(alertId, dto.Comments);
    }
}
