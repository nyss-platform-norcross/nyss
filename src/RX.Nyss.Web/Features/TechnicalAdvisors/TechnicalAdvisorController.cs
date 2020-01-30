using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.TechnicalAdvisors.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.TechnicalAdvisors
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
        public async Task<Result> Create(int nationalSocietyId, [FromBody] CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto) =>
            await _technicalAdvisorService.Create(nationalSocietyId, createTechnicalAdvisorRequestDto);

        /// <summary>
        /// Get a technical advisor.
        /// </summary>
        /// <param name="technicalAdvisorId">The ID of the requested technical advisor</param>
        /// <returns></returns>
        [HttpGet("{technicalAdvisorId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Get(int technicalAdvisorId) =>
            await _technicalAdvisorService.Get(technicalAdvisorId);

        /// <summary>
        /// Update a technical advisor.
        /// </summary>
        /// ///
        /// <param name="technicalAdvisorId">The ID of the technical advisor to be edited</param>
        /// <param name="editTechnicalAdvisorRequestDto">The data used to update the specified technical advisor</param>
        /// <returns></returns>
        [HttpPost("{technicalAdvisorId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.TechnicalAdvisorAccess)]
        public async Task<Result> Edit(int technicalAdvisorId, [FromBody] EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto) =>
            await _technicalAdvisorService.Edit(technicalAdvisorId, editTechnicalAdvisorRequestDto);

        /// <summary>
        /// Delete a technical advisor in a national society.
        /// If the technical advisor is also in other national societies, he/she will be removed from the provided national
        /// society, but the user will not be deleted.
        /// If this is the only national society of the technical advisor, the technical advisor will be deleted.
        /// </summary>
        /// <param name="nationalSocietyId">The ID of the national society the technical advisor should be deleted from</param>
        /// <param name="technicalAdvisorId">The ID of the technical advisor to be deleted</param>
        /// <returns></returns>
        [HttpPost("{technicalAdvisorId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.TechnicalAdvisorAccess), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Delete(int nationalSocietyId, int technicalAdvisorId) =>
            await _technicalAdvisorService.Delete(nationalSocietyId, technicalAdvisorId);
    }
}
