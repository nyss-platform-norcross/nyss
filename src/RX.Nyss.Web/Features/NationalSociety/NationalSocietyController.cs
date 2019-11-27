using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;

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
        [Route("{nationalSocietyId}/get"), HttpGet]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<NationalSocietyResponseDto>> Get(int nationalSocietyId) =>
            await _nationalSocietyService.GetNationalSociety(nationalSocietyId);

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpGet]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.TechnicalAdvisor, Role.DataConsumer)]
        public async Task<Result<List<NationalSocietyListResponseDto>>> List() =>
            await _nationalSocietyService.GetNationalSocieties(User.Identity.Name, User.GetRoles());

        /// <summary>
        /// Creates a new National Society.
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("create"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<int>> Create([FromBody]CreateNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.CreateNationalSociety(nationalSociety);

        /// <summary>
        /// Edits an existing National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("{nationalSocietyId}/edit"), HttpPost]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Edit(int nationalSocietyId, [FromBody]EditNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.EditNationalSociety(nationalSocietyId, nationalSociety);

        /// <summary>
        /// Removes an existing National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <returns></returns>
        [Route("{nationalSocietyId}/remove"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Remove(int nationalSocietyId) =>
            await _nationalSocietyService.RemoveNationalSociety(nationalSocietyId);

        /// <summary>
        /// Sets a user as the pending Head Manager for the National Society. Next time this user logs in, the person will get a consent form, and if the user consents the user
        /// will be the next Head Manager
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [Route("{nationalSocietyId}/setHeadManager"), HttpPost, NeedsPolicy(Policy.HeadManagerAccess)]
        public async Task<Result> SetHeadManager(int nationalSocietyId, [FromBody]SetAsHeadManagerRequestDto requestDto) =>
            await _nationalSocietyService.SetPendingHeadManager(nationalSocietyId, requestDto.UserId);


        /// <summary>
        /// Will set the current user as the head for the given national societies
        /// </summary>
        /// <returns></returns>
        [Route("consentAsHeadManager"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor)]
        public async Task<Result> ConsentAsHeadManager() =>
            await _nationalSocietyService.SetAsHeadManager(User.Identity.Name);

        /// <summary>
        /// Get the current user's list of national societies that he is assigned as pending head manager
        /// </summary>
        /// <returns></returns>
        [Route("pendingConsents"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor)]
        public async Task<Result> GetPendingConsents() =>
            await _nationalSocietyService.GetPendingHeadManagerConsents(User.Identity.Name);
    }
}
