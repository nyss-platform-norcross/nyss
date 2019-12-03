using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.GlobalCoordinator.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.GlobalCoordinator
{
    [Route("api/globalcoordinator")]
    public class GlobalCoordinatorController : BaseController
    {
        private readonly IGlobalCoordinatorService _globalCoordinatorService;

        public GlobalCoordinatorController(IGlobalCoordinatorService globalCoordinatorService)
        {
            _globalCoordinatorService = globalCoordinatorService;
        }

        /// <summary>
        ///     Register a global coordinator user.
        /// </summary>
        /// <param name="dto">The global coordinator to be created</param>
        /// <returns></returns>
        [HttpPost("create"), NeedsRole(Role.Administrator)]
        public async Task<Result> Create([FromBody]CreateGlobalCoordinatorRequestDto dto) =>
            await _globalCoordinatorService.RegisterGlobalCoordinator(dto);

        /// <summary>
        /// Get the data of a global coordinator user
        /// </summary>
        /// <param name="id">The ID of the requested global coordinator</param>
        /// <returns></returns>
        [HttpGet("{id:int}/get"), NeedsRole(Role.Administrator)]
        public async Task<Result> Get(int id) =>
            await _globalCoordinatorService.GetGlobalCoordinator(id);

        /// <summary>
        /// Edit a global coordinator user
        /// </summary>
        /// <param name="editGlobalCoordinatorRequestDto">The global coordinator user to be edited</param>
        /// <returns></returns>
        [HttpPost("{id:int}/edit"), NeedsRole(Role.Administrator)]
        public async Task<Result> Edit([FromBody]EditGlobalCoordinatorRequestDto editGlobalCoordinatorRequestDto) =>
            await _globalCoordinatorService.UpdateGlobalCoordinator(editGlobalCoordinatorRequestDto);

        /// <summary>
        /// Remove a global coordinator user
        /// </summary>
        /// <param name="id">The global coordinator user's ID to be removed</param>
        /// <returns></returns>
        [HttpPost("{id:int}/remove"), NeedsRole(Role.Administrator)]
        public async Task<Result> Remove(int id) =>
            await _globalCoordinatorService.RemoveGlobalCoordinator(id, User.GetRoles());

        /// <summary>
        /// Lists all global coordinators available in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("list"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> List() =>
            await _globalCoordinatorService.GetGlobalCoordinators();
    }
}
