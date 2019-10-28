using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator
{
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
        public async Task<Result> Create([FromBody]RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto) => 
            await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);

        /// <summary>
        /// Get the data of a global coordinator user
        /// </summary>
        /// <param name="id">The ID of the requested global coordinator</param>
        /// <returns></returns>
        [HttpGet("get"), NeedsRole(Role.Administrator)]
        public async Task<Result> Get(int id) =>
            await _globalCoordinatorService.GetGlobalCoordinator(id);

        /// <summary>
        /// Edit a global coordinator user
        /// </summary>
        /// <param name="editGlobalCoordinatorRequestDto">The global coordinator user to be edited</param>
        /// <returns></returns>
        [HttpPost("edit"), NeedsRole(Role.Administrator)]
        public async Task<Result> Edit([FromBody]EditGlobalCoordinatorRequestDto editGlobalCoordinatorRequestDto) =>
            await _globalCoordinatorService.UpdateGlobalCoordinator(editGlobalCoordinatorRequestDto);


        /// <summary>
        /// Lists all global coordinators available in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("list"), NeedsRole(Role.Administrator)]
        public async Task<Result> List() =>
            await _globalCoordinatorService.GetGlobalCoordinators();
    }
}
