using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.ProjectOrganizations.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectOrganizations
{
    [Route("api/projectOrganization")]
    public class ProjectOrganizationController : BaseController
    {
        private readonly IProjectOrganizationService _projectOrganizationService;

        public ProjectOrganizationController(IProjectOrganizationService projectOrganizationService)
        {
            _projectOrganizationService = projectOrganizationService;
        }

        /// <summary>
        /// Lists project organizations assigned to a specified project.
        /// </summary>
        /// <returns>A list of project organizations assigned to the project</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<List<ProjectOrganizationListResponseDto>>> List(int projectId) =>
            _projectOrganizationService.List(projectId);

        /// <summary>
        /// Creates a new project organization for a specified national society.
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <param name="createDto">ProjectOrganization details</param>
        /// <returns>An identifier of the created project organization</returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<int>> Create(int projectId, [FromBody] ProjectOrganizationRequestDto createDto) =>
            _projectOrganizationService.Create(projectId, createDto);

        /// <summary>
        /// Gets data used for project organization creation
        /// </summary>
        /// <param name="projectId">An identifier of a project</param>
        /// <returns>An identifier of the created project organization</returns>
        [HttpGet("getCreateData")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.ProjectAccess)]
        public Task<Result<ProjectOrganizationCreateDataResponseDto>> GetCreateData(int projectId) =>
            _projectOrganizationService.GetCreateData(projectId);

        /// <summary>
        /// Deletes a specified project organization.
        /// </summary>
        /// <param name="projectOrganizationId">An identifier of project organization to be deleted</param>
        /// <returns></returns>
        [HttpPost("{projectOrganizationId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.ProjectOrganizationAccess)]
        public Task<Result> Delete(int projectOrganizationId) =>
            _projectOrganizationService.Delete(projectOrganizationId);
    }
}
