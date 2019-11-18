using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.DataCollector.Dto;
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
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId) =>
            _projectService.GetProjects(nationalSocietyId);

        /// <summary>
        /// Lists all health risks with details in language specific for the specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">A content language identifier</param>
        /// <returns>A list of health risks with details in language specific for the specified national society</returns>
        [HttpGet, Route("api/nationalSociety/{nationalSocietyId:int}/healthRisk/list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<IEnumerable<ProjectHealthRiskResponseDto>>> GetHealthRisks(int nationalSocietyId) => 
            await _projectService.GetHealthRisks(nationalSocietyId);

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
        [HttpGet("nationalSociety/{nationalSocietyId:int}/project/listOpenedProjects")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> ListOpenedProjects(int nationalSocietyId) =>
            await _projectService.ListOpenedProjects(nationalSocietyId);
    }
}
