using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.AppData
{
    [Route("api/appdata")]
    public class AppDataController : BaseController
    {
        private readonly IAppDataService _appDataService;
        public AppDataController(IAppDataService appDataService)
        {
            _appDataService = appDataService;
        }

        /// <summary>
        /// Gets all languages.
        /// </summary>
        /// <returns></returns>
        [Route("getLanguages"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<IEnumerable<ContentLanguage>> GetLanguages() => 
            await _appDataService.GetLanguages();
    }
}
