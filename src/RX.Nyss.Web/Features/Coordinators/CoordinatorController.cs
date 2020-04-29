using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Coordinators.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Coordinators
{
    [Route("api/Coordinator")]
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
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Create(int nationalSocietyId, [FromBody] CreateCoordinatorRequestDto createCoordinatorRequestDto) =>
            await _coordinatorService.Create(nationalSocietyId, createCoordinatorRequestDto);

        /// <summary>
        /// Get a Coordinator.
        /// </summary>
        /// <param name="CoordinatorId">The ID of the requested Coordinator</param>
        /// <returns></returns>
        [HttpGet("{CoordinatorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator)]
        public async Task<Result> Get(int CoordinatorId) =>
            await _coordinatorService.Get(CoordinatorId);

        /// <summary>
        /// Update a Coordinator.
        /// </summary>
        /// <param name="CoordinatorId">The id of the Coordinator to be edited</param>
        /// <param name="editCoordinatorRequestDto">The data used to update the specified Coordinator</param>
        /// <returns></returns>
        [HttpPost("{CoordinatorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Coordinator)]
        public async Task<Result> Edit(int CoordinatorId, [FromBody] EditCoordinatorRequestDto editCoordinatorRequestDto) =>
            await _coordinatorService.Edit(CoordinatorId, editCoordinatorRequestDto);

        /// <summary>
        /// Delete a Coordinator.
        /// </summary>
        /// <param name="CoordinatorId">The ID of the Coordinator to be deleted</param>
        /// <returns></returns>
        [HttpPost("{CoordinatorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> Delete(int CoordinatorId) =>
            await _coordinatorService.Delete(CoordinatorId);
    }
}
