using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Services.StringsResources
{
    public interface IStringsResourcesService
    {
        Task<Result<IDictionary<string, string>>> GetStringsResources(string languageCode);
        Task<Result<IDictionary<string, string>>> GetEmailContentResources(string languageCode);
        Task<StringsBlob> GetStringsBlob();
        Task SaveStringsBlob(StringsBlob blob);
    }

    public class StringsResourcesService : IStringsResourcesService
    {
        private readonly INyssBlobProvider _nyssBlobProvider;
        private readonly ILogger<StringsResourcesService> _logger;

        public StringsResourcesService(
            INyssBlobProvider nyssBlobProvider,
            ILogger<StringsResourcesService> logger)
        {
            _nyssBlobProvider = nyssBlobProvider;
            _logger = logger;
        }

        public async Task<Result<IDictionary<string, string>>> GetStringsResources(string languageCode)
        {
            string GetTranslation(IDictionary<string, string> translations) =>
                translations.ContainsKey(languageCode) ? translations[languageCode] : default;

            try
            {
                var stringBlob = await GetStringsBlob();

                var dictionary = stringBlob.Strings
                    .Select(entry => new
                    {
                        entry.Key,
                        Value = GetTranslation(entry.Translations) ?? entry.DefaultValue ?? entry.Key
                    }).ToDictionary(x => x.Key, x => x.Value);

                return Success<IDictionary<string, string>>(dictionary);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "There was a problem during fetching the strings resources");
                return Error<IDictionary<string, string>>(ResultKey.UnexpectedError);
            }
        }

        public async Task<Result<IDictionary<string, string>>> GetEmailContentResources(string languageCode)
        {
            string GetTranslation(IDictionary<string, string> translations) =>
                translations.ContainsKey(languageCode) ? translations[languageCode] : default;

            try
            {
                var emailContentsBlob = await GetEmailContentBlob();

                var dictionary = emailContentsBlob.Strings
                    .Select(entry => new
                    {
                        entry.Key,
                        Value = GetTranslation(entry.Translations) ?? entry.DefaultValue ?? entry.Key
                    }).ToDictionary(x => x.Key, x => x.Value);

                return Success<IDictionary<string, string>>(dictionary);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "There was a problem during fetching the email contents resources");
                return Error<IDictionary<string, string>>(ResultKey.UnexpectedError);
            }
        }

        public async Task<StringsBlob> GetStringsBlob()
        {
            var blobValue = await _nyssBlobProvider.GetStringsResources();

            return JsonSerializer.Deserialize<StringsBlob>(blobValue, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public async Task SaveStringsBlob(StringsBlob blob)
        {
            var blobValue = JsonSerializer.Serialize(blob, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _nyssBlobProvider.SaveStringsResources(blobValue);
        }

        public async Task<StringsBlob> GetEmailContentBlob()
        {
            var blobValue = await _nyssBlobProvider.GetEmailContentResources();

            return JsonSerializer.Deserialize<StringsBlob>(blobValue, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
