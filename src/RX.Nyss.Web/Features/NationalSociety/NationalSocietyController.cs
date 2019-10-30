using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
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
        [Route("{id}/get"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<NationalSocietyResponseDto>> Get(int id) =>
            await _nationalSocietyService.GetNationalSociety(id);

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<List<NationalSocietyListResponseDto>>> List() =>
            await _nationalSocietyService.GetNationalSocieties();

        /// <summary>
        /// Creates a new National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("create"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<int>> Create([FromBody]CreateNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.CreateNationalSociety(nationalSociety);

        /// <summary>
        /// Edits an existing National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("{id}/edit"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Edit(int id, [FromBody]EditNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.EditNationalSociety(nationalSociety);

        /// <summary>
        /// Removes an existing National Society
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/remove"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Remove(int id) =>
            await _nationalSocietyService.RemoveNationalSociety(id);
    }
}
