using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Project
{
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
        [HttpGet("api/project/{projectId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectResponseDto>> GetProject(int projectId) =>
            _projectService.GetProject(projectId);

        /// <summary>
        /// Lists projects assigned to a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <returns>A list of projects assigned to the national society</returns>
        [HttpGet("api/nationalSociety/{nationalSocietyId:int}/project/list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<ProjectListItemResponseDto>>> ListProjects(int nationalSocietyId) =>
            _projectService.ListProjects(nationalSocietyId, User.Identity.Name, User.GetRoles());

        /// <summary>
        /// Adds a new project for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns>An identifier of the created project</returns>
        [HttpPost("api/nationalSociety/{nationalSocietyId:int}/project/add")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> AddProject(int nationalSocietyId, [FromBody]ProjectRequestDto projectRequestDto) =>
            _projectService.AddProject(nationalSocietyId, projectRequestDto);

        /// <summary>
        /// Updates a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be updated</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns></returns>
        [HttpPost("api/project/{projectId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> UpdateProject(int projectId, [FromBody]ProjectRequestDto projectRequestDto) =>
            _projectService.UpdateProject(projectId, projectRequestDto);

        /// <summary>
        /// Removes a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be removed</param>
        /// <returns></returns>
        [HttpPost("api/project/{projectId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> UpdateProject(int projectId) =>
            _projectService.DeleteProject(projectId);

        [HttpGet("api/project/{projectId:int}/basicData"), NeedsPolicy(Policy.ProjectAccess)]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result<ProjectBasicDataResponseDto>> GetProjectBasicData(int projectId) =>
            await _projectService.GetProjectBasicData(projectId);

        /// <summary>
        /// Get a list of all opened projects in the national society
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society</param>
        /// <returns></returns>
        [HttpGet("api/nationalSociety/{nationalSocietyId:int}/project/listOpenedProjects")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> ListOpenedProjects(int nationalSocietyId) =>
            await _projectService.ListOpenedProjects(nationalSocietyId);

        /// <summary>
        /// Get the data required to build a "create new project" form
        /// </summary>
        /// <param name="nationalSocietyId">The id of the national society</param>
        /// <returns>An object containing a list of health risks and a list of available timezones</returns>
        [HttpGet, Route("api/nationalSociety/{nationalSocietyId:int}/getFormData")]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor)]
        public async Task<Result<ProjectFormDataResponseDto>> GetProjectFormData(int nationalSocietyId) =>
            await _projectService.GetFormData(nationalSocietyId);
    }
}
