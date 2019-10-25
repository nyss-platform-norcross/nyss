using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety
{
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
        [Route("GetCountries"), HttpGet, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<IEnumerable<Country>> GetCountries() => 
            await _nationalSocietyService.GetCountries();

        /// <summary>
        /// Gets all languages.
        /// </summary>
        /// <returns></returns>
        [Route("GetLanguages"), HttpGet, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<IEnumerable<ContentLanguage>> GetLanguages() => 
            await _nationalSocietyService.GetLanguages();

        /// <summary>
        /// Creates a new National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("CreateNationalSociety"), HttpPost, Roles(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> CreateAndSaveNationalSociety([FromBody]NationalSocietyRequest nationalSociety) => 
            await _nationalSocietyService.CreateAndSaveNationalSociety(nationalSociety);
    }
}
