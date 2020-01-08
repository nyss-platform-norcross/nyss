using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety
{
    [Route("api/nationalSociety")]
    public class NationalSocietyController : BaseController
    {
        private readonly INationalSocietyService _nationalSocietyService;

        public NationalSocietyController(INationalSocietyService nationalSocietyService)
        {
            _nationalSocietyService = nationalSocietyService;
        }

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{nationalSocietyId}/get")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<NationalSocietyResponseDto>> Get(int nationalSocietyId) =>
            await _nationalSocietyService.GetNationalSociety(nationalSocietyId);

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.TechnicalAdvisor, Role.DataConsumer)]
        public async Task<Result<List<NationalSocietyListResponseDto>>> List() =>
            await _nationalSocietyService.GetNationalSocieties();

        /// <summary>
        /// Creates a new National Society.
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Create([FromBody]CreateNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.CreateNationalSociety(nationalSociety);

        /// <summary>
        /// Edits an existing National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId}/edit")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Edit(int nationalSocietyId, [FromBody]EditNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.EditNationalSociety(nationalSocietyId, nationalSociety);

        /// <summary>
        /// Removes an existing National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId}/remove")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Remove(int nationalSocietyId) =>
            await _nationalSocietyService.RemoveNationalSociety(nationalSocietyId);

        /// <summary>
        /// Sets a user as the pending Head Manager for the National Society. Next time this user logs in, the person will get a consent form, and if the user consents the user
        /// will be the next Head Manager
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId}/setHeadManager")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.HeadManagerAccess), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> SetHeadManager(int nationalSocietyId, [FromBody]SetAsHeadManagerRequestDto requestDto) =>
            await _nationalSocietyService.SetPendingHeadManager(nationalSocietyId, requestDto.UserId);


        /// <summary>
        /// Will set the current user as the head for the given national societies
        /// </summary>
        /// <returns></returns>
        [HttpPost("consentAsHeadManager")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor)]
        public async Task<Result> ConsentAsHeadManager() =>
            await _nationalSocietyService.SetAsHeadManager();

        /// <summary>
        /// Get the current user's list of national societies that he is assigned as pending head manager
        /// </summary>
        /// <returns></returns>
        [HttpGet("pendingConsents")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor)]
        public async Task<Result> GetPendingConsents() =>
            await _nationalSocietyService.GetPendingHeadManagerConsents();
    }
}
