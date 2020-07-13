using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Resources.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Resources
{
    public interface IResourcesService
    {
        Task<Result<string>> SaveString(SaveStringRequestDto dto);
        Task<Result<GetStringResponseDto>> GetString(string key);
        Task<Result<ListTranslationsResponseDto>> ListTranslations();
    }

    public class ResourcesService : IResourcesService
    {
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly INyssContext _nyssContext;
        private readonly INyssWebConfig _config;

        public ResourcesService(
            IStringsResourcesService stringsResourcesService,
            INyssContext nyssContext,
            INyssWebConfig config)
        {
            _stringsResourcesService = stringsResourcesService;
            _nyssContext = nyssContext;
            _config = config;
        }

        public async Task<Result<GetStringResponseDto>> GetString(string key)
        {
            if (_config.IsProduction)
            {
                return Error<GetStringResponseDto>(ResultKey.UnexpectedError);
            }

            var stringsBlob = await GetStringsBlob(key);
            var entry = stringsBlob.Strings.FirstOrDefault(x => x.Key == key);

            var contentLanguages = await _nyssContext.ContentLanguages.ToListAsync();

            var dto = new GetStringResponseDto
            {
                Key = key,
                Translations = contentLanguages.Select(cl =>
                {
                    var languageCode = cl.LanguageCode.ToLower();

                    return new GetStringResponseDto.GetEntryDto
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
            if (_config.IsProduction)
            {
                return Error<string>(ResultKey.UnexpectedError);
            }

            var stringsBlob = await GetStringsBlob(dto.Key);
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

            await SaveStringsBlob(new StringsBlob { Strings = strings.OrderBy(x => x.Key) }, dto.Key);

            return Success("Success");
        }

        public async Task<Result<ListTranslationsResponseDto>> ListTranslations()
        {
            if (_config.IsProduction)
            {
                return Error<ListTranslationsResponseDto>(ResultKey.UnexpectedError);
            }

            var stringsBlob = await _stringsResourcesService.GetStringsBlob();
            var emailContentBlob = await _stringsResourcesService.GetEmailContentBlob();
            var smsContentBlob = await _stringsResourcesService.GetSmsContentBlob();
            var translations = stringsBlob.Strings
                .Union(emailContentBlob.Strings)
                .Union(smsContentBlob.Strings)
                .ToList();

            var languages = await _nyssContext.ContentLanguages
                .Select(cl => new ListTranslationsResponseDto.LanguageResponseDto
                {
                    DisplayName = cl.DisplayName,
                    LanguageCode = cl.LanguageCode
                })
                .ToListAsync();

            var dto = new ListTranslationsResponseDto
            {
                Languages = languages,
                Translations = translations
                    .Select(t => new ListTranslationsResponseDto.TranslationsResponseDto
                    {
                        Key = t.Key,
                        Translations = t.Translations
                    })
                    .ToList()
            };

            return Success(dto);
        }

        private async Task<StringsBlob> GetStringsBlob(string key)
        {
            var blobIdentifier = key.Split(".")[0];

            if (blobIdentifier == "email")
            {
                return await _stringsResourcesService.GetEmailContentBlob();
            }
            else if (blobIdentifier == "sms")
            {
                return await _stringsResourcesService.GetSmsContentBlob();
            }
            else
            {
                return await _stringsResourcesService.GetStringsBlob();
            }
        }

        private async Task SaveStringsBlob(StringsBlob stringsBlob, string key)
        {
            var blobIdentifier = key.Split(".")[0];

            if (blobIdentifier == "email")
            {
                await _stringsResourcesService.SaveEmailContentsBlob(stringsBlob);
            }
            else if (blobIdentifier == "sms")
            {
                await _stringsResourcesService.SaveSmsContentsBlob(stringsBlob);
            }
            else
            {
                await _stringsResourcesService.SaveStringsBlob(stringsBlob);
            }
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
