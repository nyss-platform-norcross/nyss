using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Supervisors.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Supervisors
{
    [Route("api/supervisor")]
    public class SupervisorController : BaseController
    {
        private readonly ISupervisorService _supervisorService;

        public SupervisorController(ISupervisorService supervisorService)
        {
            _supervisorService = supervisorService;
        }

        /// <summary>
        /// Register a supervisor.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the supervisor should be registered in</param>
        /// <param name="createSupervisorRequestDto">The supervisor to be created</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Create(int nationalSocietyId, [FromBody] CreateSupervisorRequestDto createSupervisorRequestDto) =>
            await _supervisorService.Create(nationalSocietyId, createSupervisorRequestDto);

        /// <summary>
        /// Get a supervisor.
        /// </summary>
        /// <param name="supervisorId">The ID of the requested supervisor</param>
        /// <returns></returns>
        [HttpGet("{supervisorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Get(int supervisorId, int nationalSocietyId) =>
            await _supervisorService.Get(supervisorId, nationalSocietyId);

        /// <summary>
        /// Edit a supervisor.
        /// </summary>
        /// <param name="supervisorId">The id of the supervisor to be edited</param>
        /// <param name="editSupervisorRequestDto">The data used to update the specified supervisor</param>
        /// <returns></returns>
        [HttpPost("{supervisorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Edit(int supervisorId, [FromBody] EditSupervisorRequestDto editSupervisorRequestDto) =>
            await _supervisorService.Edit(supervisorId, editSupervisorRequestDto);

        /// <summary>
        /// Delete a supervisor.
        /// </summary>
        /// <param name="supervisorId">The ID of the supervisor to be deleted</param>
        /// <returns></returns>
        [HttpPost("{supervisorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Delete(int supervisorId) =>
            await _supervisorService.Delete(supervisorId);
    }
}
