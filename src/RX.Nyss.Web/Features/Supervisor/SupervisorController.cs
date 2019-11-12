using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.Supervisor.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Supervisor
{
    [Route("api")]
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
        [HttpPost("nationalSociety/{nationalSocietyId:int}/supervisor/create")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Create(int nationalSocietyId, [FromBody]CreateSupervisorRequestDto createSupervisorRequestDto) =>
            await _supervisorService.Create(nationalSocietyId, createSupervisorRequestDto);

        /// <summary>
        /// Get a supervisor.
        /// </summary>
        /// <param name="supervisorId">The ID of the requested supervisor</param>
        /// <returns></returns>
        [HttpGet("nationalSociety/supervisor/{supervisorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Get(int supervisorId) =>
            await _supervisorService.Get(supervisorId);

        /// <summary>
        /// Update a supervisor.
        /// </summary>
        /// <param name="supervisorId">The id of the supervisor to be edited</param>
        /// <param name="editSupervisorRequestDto">The data used to update the specified supervisor</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/supervisor/{supervisorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Edit(int supervisorId, [FromBody]EditSupervisorRequestDto editSupervisorRequestDto) =>
            await _supervisorService.Edit(supervisorId, editSupervisorRequestDto);

        /// <summary>
        /// Delete a supervisor.
        /// </summary>
        /// <param name="supervisorId">The ID of the supervisor to be deleted</param>
        /// <returns></returns>
        [HttpPost("nationalSociety/supervisor/{supervisorId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SupervisorAccess)]
        public async Task<Result> Remove(int supervisorId) =>
            await _supervisorService.Remove(supervisorId);
    }
}

