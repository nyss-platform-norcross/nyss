using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.GlobalCoordinators.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.GlobalCoordinators
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
        public async Task<Result> Create([FromBody] CreateGlobalCoordinatorRequestDto dto) =>
            await _globalCoordinatorService.Create(dto);

        /// <summary>
        /// Get the data of a global coordinator user
        /// </summary>
        /// <param name="id">The ID of the requested global coordinator</param>
        /// <returns></returns>
        [HttpGet("{id:int}/get"), NeedsRole(Role.Administrator)]
        public async Task<Result> Get(int id) =>
            await _globalCoordinatorService.Get(id);

        /// <summary>
        /// Edit a global coordinator user
        /// </summary>
        /// <param name="editGlobalCoordinatorRequestDto">The global coordinator user to be edited</param>
        /// <returns></returns>
        [HttpPost("{id:int}/edit"), NeedsRole(Role.Administrator)]
        public async Task<Result> Edit([FromBody] EditGlobalCoordinatorRequestDto editGlobalCoordinatorRequestDto) =>
            await _globalCoordinatorService.Edit(editGlobalCoordinatorRequestDto);

        /// <summary>
        /// Delete a global coordinator user
        /// </summary>
        /// <param name="id">The global coordinator user's ID to be deleted</param>
        /// <returns></returns>
        [HttpPost("{id:int}/delete"), NeedsRole(Role.Administrator)]
        public async Task<Result> Delete(int id) =>
            await _globalCoordinatorService.Delete(id);

        /// <summary>
        /// Lists all global coordinators available in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("list"), NeedsRole(Role.Administrator, Role.GlobalCoordinator)]
        public async Task<Result> List() =>
            await _globalCoordinatorService.List();
    }
}
