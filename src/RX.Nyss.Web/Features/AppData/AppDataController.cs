using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.AppData.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.AppData
{
    [Route("api/appData")]
    public class AppDataController : BaseController
    {
        private readonly IAppDataService _appDataService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IInMemoryCache _inMemoryCache;

        public AppDataController(
            IAppDataService appDataService,
            IStringsResourcesService stringsResourcesService,
            IInMemoryCache inMemoryCache)
        {
            _appDataService = appDataService;
            _stringsResourcesService = stringsResourcesService;
            _inMemoryCache = inMemoryCache;
        }

        /// <summary>
        /// Gets application data
        /// </summary>
        /// <returns></returns>
        [Route("getAppData"), HttpGet, AllowAnonymous]
        public async Task<Result<AppDataResponseDto>> GetAppData() =>
            await _appDataService.GetAppData();

        /// <summary>
        /// Gets strings
        /// </summary>
        /// <returns></returns>
        [Route("getStrings/{languageCode}"), HttpGet, AllowAnonymous]
        public async Task<Result<IDictionary<string, string>>> GetStrings(string languageCode) =>
            await _inMemoryCache.GetCachedResult(
                $"{nameof(GetStrings)}.{languageCode}",
                TimeSpan.FromMinutes(5),
                () => _stringsResourcesService.GetStringsResources(languageCode));

        [HttpGet("version")]
        public async Task<ActionResult> GetVersion()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var version = assemblyName.Version;

            return Ok(new
            {
                assemblyName.Name,
                Version = $"{version.Major}.{version.Minor}.{version.Build}",
                Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
            });
        }
    }
}
