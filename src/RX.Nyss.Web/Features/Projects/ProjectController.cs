using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Projects.Commands;
using RX.Nyss.Web.Features.Projects.Dto;
using RX.Nyss.Web.Features.Projects.Queries;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Projects
{
    [Route("api/project")]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Gets a project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>A project</returns>
        [HttpGet("{projectId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectResponseDto>> Get(int projectId) =>
            _projectService.Get(projectId);

        /// <summary>
        /// Lists projects assigned to a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="utcOffset">The UTC offset in hours</param>
        /// <returns>A list of projects assigned to the national society</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<ProjectListItemResponseDto>>> List(int nationalSocietyId, int utcOffset) =>
            _projectService.List(nationalSocietyId, utcOffset);

        /// <summary>
        /// Adds a new project for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns>An identifier of the created project</returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> Create(int nationalSocietyId, [FromBody] CreateProjectRequestDto projectRequestDto) =>
            _projectService.Create(nationalSocietyId, projectRequestDto);

        /// <summary>
        /// Updates a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be updated</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> Edit(int projectId, [FromBody] EditProjectRequestDto projectRequestDto) =>
            _projectService.Edit(projectId, projectRequestDto);

        /// <summary>
        /// Updates a specified project's health risks.
        /// </summary>
        /// <param name="projectId">An identifier of project to be updated</param>
        /// <param name="healthRisksRequestDto">A list of healthrisks</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/editProjectHealthRisks")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> EditProjectHealthRisks(int projectId, [FromBody] EditHealthRisksRequestDto healthRisksRequestDto) =>
            _projectService.EditProjectHealthRisks(projectId, healthRisksRequestDto);

        /// <summary>
        /// Closes a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be closed</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/close")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> Close(int projectId) =>
            _projectService.Close(projectId);

        /// <summary>
        /// Gets basic information about a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>Basic information about a project</returns>
        [HttpGet("{projectId:int}/basicData")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<ProjectBasicDataResponseDto>> GetBasicData(int projectId) =>
            await Sender.Send(new GetProjectBasicDataQuery(projectId));

        /// <summary>
        /// Get the data required to build a "create new project" form
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society</param>
        /// <returns>An object containing a list of health risks and a list of available timezones</returns>
        [HttpGet, Route("getFormData")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<ProjectFormDataResponseDto>> GetFormData(int nationalSocietyId) =>
            await _projectService.GetFormData(nationalSocietyId);

        /// <summary>
        /// Gets error message for specified project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>List of error messages</returns>
        [HttpGet("{projectId:int}/errorMessages")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IReadOnlyList<ProjectErrorMessageDto>> GetErrorMessages(int projectId) =>
            await Sender.Send(new GetProjectErrorMessagesQuery(projectId));

        /// <summary>
        /// Updates error message
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="body">Key-value pair with messages</param>
        /// <returns></returns>
        [HttpPut("{projectId:int}/errorMessages")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<IReadOnlyList<ProjectErrorMessageDto>> UpdateErrorMessages([FromRoute] int projectId, [FromBody] Dictionary<string, string> body) =>
            await Sender.Send(new UpdateProjectErrorMessagesCommand(projectId, body));
    }
}
