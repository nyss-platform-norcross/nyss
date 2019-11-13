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
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer), NeedsPolicy(Policy.NationalSocietyAccess)]
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
    }
}
