using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Coordinators.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Coordinators
{
    [Route("api/coordinator")]
    public class CoordinatorController : BaseController
    {
        private readonly ICoordinatorService _coordinatorService;

        public CoordinatorController(ICoordinatorService coordinatorService)
        {
            _coordinatorService = coordinatorService;
        }

        /// <summary>
        /// Register a Coordinator.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the Coordinator should be registered in</param>
        /// <param name="createCoordinatorRequestDto">The Coordinator to be created</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Create(int nationalSocietyId, [FromBody] CreateCoordinatorRequestDto createCoordinatorRequestDto) =>
            await _coordinatorService.Create(nationalSocietyId, createCoordinatorRequestDto);

        /// <summary>
        /// Get a Coordinator.
        /// </summary>
        /// <param name="coordinatorId">The ID of the requested Coordinator</param>
        /// <returns></returns>
        [HttpGet("{coordinatorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator), NeedsPolicy(Policy.CoordinatorAccess)]
        public async Task<Result> Get(int coordinatorId) =>
            await _coordinatorService.Get(coordinatorId);

        /// <summary>
        /// Update a Coordinator.
        /// </summary>
        /// <param name="coordinatorId">The id of the Coordinator to be edited</param>
        /// <param name="editCoordinatorRequestDto">The data used to update the specified Coordinator</param>
        /// <returns></returns>
        [HttpPost("{coordinatorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator), NeedsPolicy(Policy.CoordinatorAccess)]
        public async Task<Result> Edit(int coordinatorId, [FromBody] EditCoordinatorRequestDto editCoordinatorRequestDto) =>
            await _coordinatorService.Edit(coordinatorId, editCoordinatorRequestDto);

        /// <summary>
        /// Delete a Coordinator.
        /// </summary>
        /// <param name="coordinatorId">The ID of the Coordinator to be deleted</param>
        /// <returns></returns>
        [HttpPost("{coordinatorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator), NeedsPolicy(Policy.CoordinatorAccess)]
        public async Task<Result> Delete(int coordinatorId) =>
            await _coordinatorService.Delete(coordinatorId);
    }
}
