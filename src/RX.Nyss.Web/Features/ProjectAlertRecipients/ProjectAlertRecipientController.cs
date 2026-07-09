using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients
{
    [Route("api/projectAlertRecipient")]
    public class ProjectAlertRecipientController : BaseController
    {
        private readonly IProjectAlertRecipientService _alertRecipientService;

        public ProjectAlertRecipientController(IProjectAlertRecipientService AlertRecipientService)
        {
            _alertRecipientService = AlertRecipientService;
        }

        /// <summary>
        /// Lists alert recipients assigned to a specified project.
        /// </summary>
        /// <returns>A list of alert recipients assigned to the project</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<List<ProjectAlertRecipientResponseDto>>> List(int nationalSocietyId, int projectId) =>
            _alertRecipientService.List(nationalSocietyId, projectId);

        /// <summary>
        /// Gets an alert recipient.
        /// </summary>
        /// <returns>An alert recipient</returns>
        [HttpGet("{alertRecipientId:int}/get")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.AlertRecipientAccess)]
        public Task<Result<ProjectAlertRecipientResponseDto>> Get(int alertRecipientId) =>
            _alertRecipientService.Get(alertRecipientId);

        /// <summary>
        /// Creates a new alert recipient for a specified project.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="createDto">AlertRecipient details</param>
        /// <returns>An identifier of the created alert recipient</returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<int>> Create(int nationalSocietyId, int projectId, [FromBody] ProjectAlertRecipientRequestDto createDto) =>
            _alertRecipientService.Create(nationalSocietyId, projectId, createDto);


        /// <summary>
        /// Gets form data needed for creating a new alert recipient for a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        [HttpGet("formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectAlertRecipientFormDataDto>> FormData(int projectId) =>
            _alertRecipientService.GetFormData(projectId);

        /// <summary>
        /// Edits an alert recipient.
        /// </summary>
        /// <param name="alertRecipientId">An identifier of an alert recipient</param>
        /// <param name="editDto">AlertRecipient details</param>
        /// <returns>An identifier of the created alert recipient</returns>
        [HttpPost("{alertRecipientId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.AlertRecipientAccess)]
        public Task<Result> Edit(int alertRecipientId, [FromBody] ProjectAlertRecipientRequestDto editDto) =>
            _alertRecipientService.Edit(alertRecipientId, editDto);

        /// <summary>
        /// Deletes a specified alert recipient.
        /// </summary>
        /// <param name="alertRecipientId">An identifier of alert recipient to be deleted</param>
        /// <returns></returns>
        [HttpPost("{alertRecipientId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.AlertRecipientAccess)]
        public Task<Result> Delete(int alertRecipientId) =>
            _alertRecipientService.Delete(alertRecipientId);
    }
}
