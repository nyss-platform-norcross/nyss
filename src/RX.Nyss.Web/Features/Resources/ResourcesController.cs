using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Resources.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Resources
{
    [Route("api/resources")]
    public class ResourcesController : BaseController
    {
        private readonly IResourcesService _resourcesService;
        private readonly IInMemoryCache _inMemoryCache;

        public ResourcesController(
            IResourcesService resourcesService,
            IInMemoryCache inMemoryCache)
        {
            _resourcesService = resourcesService;
            _inMemoryCache = inMemoryCache;
        }

        [Route("saveString"), HttpPost, NeedsRole(Role.Administrator)]
        public async Task<Result> SaveString([FromBody]SaveStringRequestDto dto)
        {
            var result = await _resourcesService.SaveString(dto);

            if (result.IsSuccess)
            {
                foreach (var translation in dto.Translations)
                {
                    _inMemoryCache.Remove($"GetStrings.{translation.LanguageCode.ToLower()}");
                }
            }

            return result;
        }

        [Route("getString/{key}"), HttpGet, NeedsRole(Role.Administrator)]
        public async Task<Result<GetStringResponseDto>> GetString(string key) =>
            await _resourcesService.GetString(key);
    }
}
