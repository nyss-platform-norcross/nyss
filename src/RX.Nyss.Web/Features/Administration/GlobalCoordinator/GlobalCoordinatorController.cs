using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
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
        /// Register a global coordinator user.
        /// </summary>
        /// <param name="registerGlobalCoordinatorRequestDto">The global coordinator to be created</param>
        /// <returns></returns>
        [HttpPost("registerGlobalCoordinator")]
        [NeedsRole(Role.Administrator)]
        public async Task<Result> RegisterGlobalCoordinator([FromBody]RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto) => 
            await _globalCoordinatorService.RegisterGlobalCoordinator(registerGlobalCoordinatorRequestDto);
    }
}
