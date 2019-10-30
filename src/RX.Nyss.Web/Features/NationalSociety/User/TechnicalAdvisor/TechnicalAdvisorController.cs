using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor
{
    [Route("api/nationalSociety/technicalAdvisor")]
    public class TechnicalAdvisorController
    {
        private readonly ITechnicalAdvisorService _technicalAdvisorService;

        public TechnicalAdvisorController(ITechnicalAdvisorService technicalAdvisorService)
        {
            _technicalAdvisorService = technicalAdvisorService;
        }

        /// <summary>
        /// Register a technical advisor.
        /// </summary>
        /// <param name="createTechnicalAdvisorRequestDto">The technical advisor to be created</param>
        /// <param name="nationalSocietyId">The ID of the national society the technical advisor should be registered in</param>
        /// <returns></returns>
        [HttpPost("create"), NeedsRole(Role.GlobalCoordinator, Role.DataManager, Role.DataManager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateTechnicalAdvisor([FromQuery]int nationalSocietyId, [FromBody]CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto) =>
            await _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, createTechnicalAdvisorRequestDto);

        /// <summary>
        /// Get a technical advisor.
        /// </summary>
        /// <param name="id">The ID of the requested technical advisor</param>
        /// <returns></returns>
        [HttpGet("get"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Get(int id) =>
            await _technicalAdvisorService.GetTechnicalAdvisor(id);

        /// <summary>
        /// Update a technical advisor.
        /// </summary>
        /// <param name="editTechnicalAdvisorRequestDto">The technical advisor to be updated</param>
        /// <returns></returns>
        [HttpPost("edit"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Edit([FromBody]EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto) =>
            await _technicalAdvisorService.UpdateTechnicalAdvisor(editTechnicalAdvisorRequestDto);


        /// <summary>
        /// Delete a technical advisor.
        /// </summary>
        /// <param name="id">The ID of the technical advisor to be deleted</param>
        /// <returns></returns>
        [HttpGet("delete"), NeedsRole(Role.GlobalCoordinator, Role.DataManager), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Delete(int id) =>
            await _technicalAdvisorService.DeleteTechnicalAdvisor(id);
    }
}

