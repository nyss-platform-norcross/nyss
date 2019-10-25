using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
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
        [Route("getCountries"), HttpGet, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<IEnumerable<Country>> GetCountries() => 
            await _nationalSocietyService.GetCountries();

        /// <summary>
        /// Gets all languages.
        /// </summary>
        /// <returns></returns>
        [Route("getLanguages"), HttpGet, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<IEnumerable<ContentLanguage>> GetLanguages() => 
            await _nationalSocietyService.GetLanguages();

        /// <summary>
        /// Creates a new National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("createNationalSociety"), HttpPost, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> CreateNationalSociety([FromBody]CreateNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.CreateNationalSociety(nationalSociety);

        /// <summary>
        /// Edits an existing National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("editNationalSociety"), HttpPost, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> EditNationalSociety([FromBody]EditNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.EditNationalSociety(nationalSociety);
    }
}
