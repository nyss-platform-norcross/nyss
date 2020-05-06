using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Organizations.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Organizations
{
    [Route("api/organization")]
    public class OrganizationController : BaseController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        /// <summary>
        /// Gets a organization.
        /// </summary>
        /// <returns>A organization</returns>
        [HttpGet("{organizationId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.OrganizationAccess)]
        public Task<Result<OrganizationResponseDto>> Get(int organizationId) =>
            _organizationService.Get(organizationId);

        /// <summary>
        /// Lists organizations assigned to a specified national society.
        /// </summary>
        /// <returns>A list of organizations assigned to the national society</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<OrganizationListResponseDto>>> List(int nationalSocietyId) =>
            _organizationService.List(nationalSocietyId);

        /// <summary>
        /// Creates a new organization for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="gatewaySettingRequestDto">A organization settings</param>
        /// <returns>An identifier of the created organization setting</returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> Create(int nationalSocietyId, [FromBody] OrganizationRequestDto gatewaySettingRequestDto) =>
            _organizationService.Create(nationalSocietyId, gatewaySettingRequestDto);

        /// <summary>
        /// Edits a specified organization.
        /// </summary>
        /// <param name="organizationId">An identifier of organization to be updated</param>
        /// <param name="gatewaySettingRequestDto">A organization settings</param>
        /// <returns></returns>
        [HttpPost("{organizationId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.OrganizationAccess)]
        public Task<Result> Edit(int organizationId, [FromBody] OrganizationRequestDto gatewaySettingRequestDto) =>
            _organizationService.Edit(organizationId, gatewaySettingRequestDto);

        /// <summary>
        /// Deletes a specified organization.
        /// </summary>
        /// <param name="organizationId">An identifier of organization to be deleted</param>
        /// <returns></returns>
        [HttpPost("{organizationId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.OrganizationAccess)]
        public Task<Result> Delete(int organizationId) =>
            _organizationService.Delete(organizationId);
    }
}
