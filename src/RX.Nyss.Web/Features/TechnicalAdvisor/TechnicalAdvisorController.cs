using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.TechnicalAdvisor.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.TechnicalAdvisor
{
    [Route("api/technicalAdvisor")]
    public class TechnicalAdvisorController : BaseController
    {
        private readonly ITechnicalAdvisorService _technicalAdvisorService;

        public TechnicalAdvisorController(ITechnicalAdvisorService technicalAdvisorService)
        {
            _technicalAdvisorService = technicalAdvisorService;
        }

        /// <summary>
        /// Register a technical advisor.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the technical advisor should be registered in</param>
        /// <param name="createTechnicalAdvisorRequestDto">The technical advisor to be created</param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> CreateTechnicalAdvisor(int nationalSocietyId, [FromBody]CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto) =>
            await _technicalAdvisorService.CreateTechnicalAdvisor(nationalSocietyId, createTechnicalAdvisorRequestDto);

        /// <summary>
        /// Get a technical advisor.
        /// </summary>
        /// <param name="technicalAdvisorId">The ID of the requested technical advisor</param>
        /// <returns></returns>
        [HttpGet("{technicalAdvisorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Get(int technicalAdvisorId) =>
            await _technicalAdvisorService.GetTechnicalAdvisor(technicalAdvisorId);

        /// <summary>
        /// Update a technical advisor.
        /// </summary>
        /// ///
        /// <param name="technicalAdvisorId">The ID of the technical advisor to be edited</param>
        /// <param name="editTechnicalAdvisorRequestDto">The data used to update the specified technical advisor</param>
        /// <returns></returns>
        [HttpPost("{technicalAdvisorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Edit(int technicalAdvisorId, [FromBody]EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto) =>
            await _technicalAdvisorService.UpdateTechnicalAdvisor(technicalAdvisorId, editTechnicalAdvisorRequestDto);

        /// <summary>
        /// Remove a technical advisor from a national society.
        /// If the technical advisor is also in other national societies, he/she will be removed from the provided national society, but the user will not be deleted.
        /// If this is the only national society of the technical advisor, the technical advisor will be deleted.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the technical advisor should be removed from</param>
        /// <param name="technicalAdvisorId">The ID of the technical advisor to be removed</param>
        /// <returns></returns>
        [HttpPost("{technicalAdvisorId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.TechnicalAdvisorAccess), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Delete(int nationalSocietyId, int technicalAdvisorId) =>
            await _technicalAdvisorService.DeleteTechnicalAdvisor(nationalSocietyId, technicalAdvisorId, User.GetRoles());
    }
}
