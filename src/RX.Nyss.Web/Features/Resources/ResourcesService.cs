using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Resources.Dto;
using RX.Nyss.Web.Services.StringsResources;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Resources
{
    public interface IResourcesService
    {
        Task<Result<string>> SaveString(SaveStringRequestDto dto);
        Task<Result<GetStringResponseDto>> GetString(string key);
    }

    public class ResourcesService : IResourcesService
    {
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly INyssContext _nyssContext;

        public ResourcesService(
            IStringsResourcesService stringsResourcesService,
            INyssContext nyssContext)
        {
            _stringsResourcesService = stringsResourcesService;
            _nyssContext = nyssContext;
        }

        public async Task<Result<GetStringResponseDto>> GetString(string key)
        {
            var stringsBlob = await _stringsResourcesService.GetStringsBlob();
            var entry = stringsBlob.Strings.FirstOrDefault(x => x.Key == key);

            var contentLanguages = await _nyssContext.ContentLanguages.ToListAsync();

            var dto = new GetStringResponseDto
            {
                Key = key,
                Translations = contentLanguages.Select(cl =>
                {
                    var languageCode = cl.LanguageCode.ToLower();

                    return new GetStringResponseDto.EntryDto
                    {
                        LanguageCode = languageCode,
                        Name = cl.DisplayName,
                        Value = entry?.Translations?.ContainsKey(languageCode) == true
                            ? entry.Translations[languageCode]
                            : ""
                    };
                }).ToList()
            };

            return Success(dto);
        }

        public async Task<Result<string>> SaveString(SaveStringRequestDto dto)
        {
            var stringsBlob =  await _stringsResourcesService.GetStringsBlob();
            var strings = stringsBlob.Strings.ToList();
            var entry = strings.FirstOrDefault(x => x.Key == dto.Key) ?? CreateEntry(strings, dto.Key);

            foreach (var dtoTranslation in dto.Translations)
            {
                var languageCode = dtoTranslation.LanguageCode.ToLower();

                if (entry.Translations.ContainsKey(languageCode))
                {
                    entry.Translations[languageCode] = dtoTranslation.Value;
                }
                else
                {
                    entry.Translations.Add(languageCode, dtoTranslation.Value);
                }
            }

            await _stringsResourcesService.SaveStringsBlob(new StringsBlob
            {
                Strings = strings
            });

            return Success("Success");
        }

        private static StringsBlob.Entry CreateEntry(ICollection<StringsBlob.Entry> strings, string key)
        {
            var entry = new StringsBlob.Entry
            {
                Key = key,
                Translations = new Dictionary<string, string>()
            };

            strings.Add(entry);
            return entry;
        }
    }
}
