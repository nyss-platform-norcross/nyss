using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Projects.Dto;
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
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectResponseDto>> Get(int projectId) =>
            _projectService.Get(projectId);

        /// <summary>
        /// Lists projects assigned to a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <returns>A list of projects assigned to the national society</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<ProjectListItemResponseDto>>> List(int nationalSocietyId) =>
            _projectService.List(nationalSocietyId);

        /// <summary>
        /// Adds a new project for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns>An identifier of the created project</returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> Create(int nationalSocietyId, [FromBody] ProjectRequestDto projectRequestDto) =>
            _projectService.Create(nationalSocietyId, projectRequestDto);

        /// <summary>
        /// Updates a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be updated</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> Edit(int projectId, [FromBody] ProjectRequestDto projectRequestDto) =>
            _projectService.Edit(projectId, projectRequestDto);

        /// <summary>
        /// Closes a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be closed</param>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/close")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess), NeedsPolicy(Policy.HeadManagerAccess)]
        public Task<Result> Close(int projectId) =>
            _projectService.Close(projectId);

        /// <summary>
        /// Gets basic information about a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>Basic information about a project</returns>
        [HttpGet("{projectId:int}/basicData")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<ProjectBasicDataResponseDto>> GetBasicData(int projectId) =>
            await _projectService.GetBasicData(projectId);

        /// <summary>
        /// Get the data required to build a "create new project" form
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society</param>
        /// <returns>An object containing a list of health risks and a list of available timezones</returns>
        [HttpGet, Route("getFormData")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<ProjectFormDataResponseDto>> GetFormData(int nationalSocietyId) =>
            await _projectService.GetFormData(nationalSocietyId);
    }
}
