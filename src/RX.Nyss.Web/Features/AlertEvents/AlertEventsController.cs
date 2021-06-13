using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.AlertEvents.Dto;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.AlertEvents
{
    [Route("api/alertEvents")]
    public class AlertEventsController : BaseController
    {

        private readonly IAlertEventsService _alertEventsService;

        public AlertEventsController(IAlertEventsService alertEventsService)
        {
            _alertEventsService = alertEventsService;
        }

        /// <summary>
        /// Adds a new alert event to an alert
        /// </summary>
        [HttpPost("{alertId:int}/eventLog/add")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<Result> CreateAlertEventLogItem(int alertId, CreateAlertEventRequestDto createDto) =>
            await _alertEventService.CreateAlertEventLogItem(alertId, createDto);
        /// <summary>
        /// Retrieves the log of events for an alert
        /// </summary>
        /// <param name="alertId">An identifier of the alert</param>
        /// <param name="utcOffset">The offset from utc in hours</param>
        [HttpGet("{alertId:int}/eventLog")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.AlertAccess)]
        public async Task<Result<AlertEventsLogResponseDto>> GetAlertEventLogItems(int alertId, int utcOffset) =>
            await _alertEventsService.GetLogItems(alertId, utcOffset);


        /// <summary>
        /// Gets form data
        /// </summary>
        [HttpGet("eventLog/formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.Supervisor, Role.HeadSupervisor, Role.TechnicalAdvisor)]
        public async Task<Result<AlertEventCreateFormDto>> GetFormData() =>
            await _alertEventsService.GetFormData();

    }
}
