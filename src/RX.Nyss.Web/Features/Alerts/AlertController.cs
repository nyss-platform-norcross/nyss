using System.Collections.Generic;
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

        public AlertController(IAlertService alertService)
        {
            _alertService = alertService;
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
    }
}
