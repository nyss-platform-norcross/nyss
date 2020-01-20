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
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectResponseDto>> GetProject(int projectId) =>
            _projectService.GetProject(projectId);

        /// <summary>
        /// Lists projects assigned to a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <returns>A list of projects assigned to the national society</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<ProjectListItemResponseDto>>> ListProjects(int nationalSocietyId) =>
            _projectService.ListProjects(nationalSocietyId);

        /// <summary>
        /// Adds a new project for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns>An identifier of the created project</returns>
        [HttpPost("add")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> AddProject(int nationalSocietyId, [FromBody]ProjectRequestDto projectRequestDto) =>
            _projectService.AddProject(nationalSocietyId, projectRequestDto);

        /// <summary>
        /// Updates a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be updated</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> UpdateProject(int projectId, [FromBody]ProjectRequestDto projectRequestDto) =>
            _projectService.UpdateProject(projectId, projectRequestDto);

        /// <summary>
        /// Removes a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be removed</param>
        /// <returns></returns>
        [HttpPost("{projectId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> UpdateProject(int projectId) =>
            _projectService.DeleteProject(projectId);

        /// <summary>
        /// Gets basic information about a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>Basic information about a project</returns>
        [HttpGet("{projectId:int}/basicData")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public async Task<Result<ProjectBasicDataResponseDto>> GetProjectBasicData(int projectId) =>
            await _projectService.GetProjectBasicData(projectId);

        /// <summary>
        /// Get a list of all opened projects in the national society
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society</param>
        /// <returns>A list of opened projects</returns>
        [HttpGet("listOpenedProjects")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> ListOpenedProjects(int nationalSocietyId) =>
            await _projectService.ListOpenedProjects(nationalSocietyId);

        /// <summary>
        /// Get the data required to build a "create new project" form
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society</param>
        /// <returns>An object containing a list of health risks and a list of available timezones</returns>
        [HttpGet, Route("getFormData")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<ProjectFormDataResponseDto>> GetProjectFormData(int nationalSocietyId) =>
            await _projectService.GetFormData(nationalSocietyId);
    }
}
