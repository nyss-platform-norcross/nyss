using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.AppData.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

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
        [Route("get"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<AppDataResponseDto>> GetAppData() => 
            await _appDataService.GetAppData();
    }
}
