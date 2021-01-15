using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.HeadSupervisors.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.HeadSupervisors
{
    [Route("api/headSupervisor")]
    public class HeadSupervisorController : BaseController
    {
        private readonly IHeadSupervisorService _headSupervisorService;

        public HeadSupervisorController(IHeadSupervisorService headSupervisorService)
        {
            _headSupervisorService = headSupervisorService;
        }

        /// <summary>
        /// Register a head supervisor.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the supervisor should be registered in</param>
        /// <param name="createHeadSupervisorRequestDto">The supervisor to be created</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Create(int nationalSocietyId, [FromBody] CreateHeadSupervisorRequestDto createHeadSupervisorRequestDto) =>
            await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);

        /// <summary>
        /// Get a head supervisor.
        /// </summary>
        /// <param name="headSupervisorId">The ID of the requested supervisor</param>
        /// <param name="nationalSocietyId">The ID of the national society</param>
        /// <returns></returns>
        [HttpGet("{headSupervisorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Get(int headSupervisorId, int nationalSocietyId) =>
            await _headSupervisorService.Get(headSupervisorId, nationalSocietyId);

        /// <summary>
        /// Edit a head supervisor.
        /// </summary>
        /// <param name="headSupervisorId">The id of the supervisor to be edited</param>
        /// <param name="editHeadSupervisorRequestDto">The data used to update the specified supervisor</param>
        /// <returns></returns>
        [HttpPost("{headSupervisorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Edit(int headSupervisorId, [FromBody] EditHeadSupervisorRequestDto editHeadSupervisorRequestDto) =>
            await _headSupervisorService.Edit(headSupervisorId, editHeadSupervisorRequestDto);

        /// <summary>
        /// Delete a head supervisor.
        /// </summary>
        /// <param name="headSupervisorId">The ID of the supervisor to be deleted</param>
        /// <returns></returns>
        [HttpPost("{headSupervisorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Delete(int headSupervisorId) =>
            await _headSupervisorService.Delete(headSupervisorId);
    }
}
