using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Commands;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Alerts.Queries;
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
        /// Gets filter data
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        [HttpGet("getFiltersData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<AlertListFilterResponseDto>> GetFiltersData(int projectId) =>
            await Sender.Send(new GetFiltersDataQuery(projectId));

        /// <summary>
        /// Lists alerts for a specific project
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="filterRequestDto">Filters</param>
        [HttpPost("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber, [FromBody] AlertListFilterRequestDto filterRequestDto) =>
            await Sender.Send(new GetListQuery(projectId, pageNumber, filterRequestDto));

        /// <summary>
        /// Gets information about the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="utcOffset">The offset from utc in hours</param>
        [HttpGet("{alertId:int}/get")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor, Role.Coordinator)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<AlertAssessmentResponseDto> Get(int alertId, int utcOffset) =>
            await Sender.Send(new GetAlertAssessmentQuery(alertId) { UtcOffset = utcOffset });

        /// <summary>
        /// Gets information about the alert's notification recipients
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpGet("{alertId:int}/alertRecipients")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<Result<AlertRecipientsResponseDto>> GetAlertRecipients(int alertId) =>
            await Sender.Send(new GetAlertRecipientsByAlertIdQuery(alertId));

        /// <summary>
        /// Accepts the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpPost("{alertId:int}/acceptReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<AcceptReportResponseDto>> AcceptReport(int alertId, int reportId) =>
            _alertReportService.AcceptReport(alertId, reportId);

        /// <summary>
        /// Dismisses the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpPost("{alertId:int}/dismissReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<DismissReportResponseDto>> DismissReport(int alertId, int reportId) =>
            _alertReportService.DismissReport(alertId, reportId);

        /// <summary>
        /// Resets the report
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="reportId">An identifier of the report</param>
        [HttpPost("{alertId:int}/resetReport")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public Task<Result<ResetReportResponseDto>> ResetReport(int alertId, int reportId) =>
            _alertReportService.ResetReport(alertId, reportId);

        /// <summary>
        /// Escalates the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="dto">A parameter enabling/disabling notifications</param>
        [HttpPost("{alertId:int}/escalate")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<Result> Escalate(int alertId, [FromBody] EscalateAlertRequestDto dto) =>
            await Sender.Send(new EscalateCommand(alertId,  dto.SendNotification));

        /// <summary>
        /// Dismisses the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpPost("{alertId:int}/dismiss")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<Result> Dismiss(int alertId) =>
            await Sender.Send(new DismissCommand(alertId));

        /// <summary>
        /// Closes the alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        [HttpPost("{alertId:int}/close")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<Result> Close(int alertId) =>
            await Sender.Send(new CloseCommand(alertId));

        /// <summary>
        /// Exports the alert list to excel
        /// </summary>
        /// <param name="projectId">An identifier of the project</param>
        /// <param name="alertListFilterRequestDto">Filters</param>
        /// <returns>An excel export of the alert list</returns>
        [HttpPost("export")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor, Role.HeadSupervisor, Role.Coordinator)]
        [NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IActionResult> Export(int projectId, [FromBody] AlertListFilterRequestDto alertListFilterRequestDto)
        {
            var excelSheetBytes =  await Sender.Send(new ExportQuery(projectId, alertListFilterRequestDto));;
            return File(excelSheetBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
