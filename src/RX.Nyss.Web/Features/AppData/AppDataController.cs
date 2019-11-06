using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.AppData.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.StringsResources;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.AppData
{
    [Route("api/appdata")]
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
        [Route("getAppData"), HttpGet]
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
    }
}
