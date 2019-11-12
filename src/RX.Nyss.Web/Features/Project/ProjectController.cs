using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

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
        /// <returns>A project</returns>
        [HttpGet("api/project/{projectId:int}/get"), NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager)]
        //[NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectResponseDto>> GetProject(int projectId) =>
            _projectService.GetProject(projectId);

        /// <summary>
        /// Lists projects assigned to a specified national society.
        /// </summary>
        /// <returns>A list of projects assigned to the national society</returns>
        [HttpGet("api/nationalSociety/{nationalSocietyId:int}/project/list"), NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager)]
        //[NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId) =>
            _projectService.GetProjects(nationalSocietyId);

        /// <summary>
        /// Adds a new project for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns>An identifier of the created project</returns>
        [HttpPost("api/nationalSociety/{nationalSocietyId:int}/project/add"), NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager)]
        //[NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> AddProject(int nationalSocietyId, [FromBody]ProjectRequestDto projectRequestDto) =>
            _projectService.AddProject(nationalSocietyId, projectRequestDto);

        /// <summary>
        /// Updates a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be updated</param>
        /// <param name="projectRequestDto">A project</param>
        /// <returns></returns>
        [HttpPost("api/project/{projectId:int}/edit"), NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager)]
        //[NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> UpdateProject(int projectId, [FromBody]ProjectRequestDto projectRequestDto) =>
            _projectService.UpdateProject(projectId, projectRequestDto);

        /// <summary>
        /// Removes a specified project.
        /// </summary>
        /// <param name="projectId">An identifier of project to be removed</param>
        /// <returns></returns>
        [HttpPost("api/project/{projectId:int}/remove"), NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager)]
        //[NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result> UpdateProject(int projectId) =>
            _projectService.DeleteProject(projectId);
    }
}
