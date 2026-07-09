using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients
{
    [Route("api/projectAlertNotHandledRecipient")]
    public class ProjectAlertNotHandledRecipientController : BaseController
    {
        private readonly IProjectAlertNotHandledRecipientService _projectAlertNotHandledRecipientService;

        public ProjectAlertNotHandledRecipientController(IProjectAlertNotHandledRecipientService projectAlertNotHandledRecipientService)
        {
            _projectAlertNotHandledRecipientService = projectAlertNotHandledRecipientService;
        }

        /// <summary>
        /// Creates an alert not handled notification recipient for a project
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <param name="dto">AlertNotHandledRecipient details</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result> Create(int projectId, [FromBody] ProjectAlertNotHandledRecipientRequestDto dto) =>
            await _projectAlertNotHandledRecipientService.Create(projectId, dto);


        /// <summary>
        /// Edits an alert not handled notification recipient for a project
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <param name="dto">AlertNotHandledRecipients details</param>
        /// <returns></returns>
        [HttpPost("edit")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result> Edit(int projectId, [FromBody] ProjectAlertNotHandledRecipientsRequestDto dto) =>
            await _projectAlertNotHandledRecipientService.Edit(projectId, dto);

        /// Deletes an alert not handled notification recipient for a project
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <param name="dto">AlertNotHandledRecipient details</param>
        /// <returns></returns>
        [HttpPost("delete")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result> Edit(int projectId, [FromBody] ProjectAlertNotHandledRecipientRequestDto dto) =>
            await _projectAlertNotHandledRecipientService.Delete(projectId, dto);

        /// <summary>
        /// Lists all alert not handled recipients configured for a project
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <returns>A list of all alert not handled recipients</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<List<ProjectAlertNotHandledRecipientsResponseDto>>> List(int projectId) =>
            await _projectAlertNotHandledRecipientService.List(projectId);

        /// <summary>
        /// Lists all users available to be set as alert not handled recipient for a project
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <returns>List of all available users</returns>
        [HttpGet("formData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<List<ProjectAlertNotHandledRecipientResponseDto>>> GetFormData(int projectId) =>
            await _projectAlertNotHandledRecipientService.GetFormData(projectId);
    }
}
