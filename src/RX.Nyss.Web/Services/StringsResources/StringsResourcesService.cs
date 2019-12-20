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
        Task<Result<IDictionary<string, string>>> GetSmsContentResources(string languageCode);
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
            try
            {
                var stringBlob = await GetStringsBlob();

                var dictionary = stringBlob.Strings
                    .Select(entry => new
                    {
                        entry.Key,
                        Value = entry.GetTranslation(languageCode)
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
            try
            {
                var emailContentsBlob = await GetEmailContentBlob();

                var dictionary = emailContentsBlob.Strings
                    .Select(entry => new
                    {
                        entry.Key,
                        Value = entry.GetTranslation(languageCode)
                    })
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Value);

                return Success<IDictionary<string, string>>(dictionary);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "There was a problem during fetching the email contents resources");
                return Error<IDictionary<string, string>>(ResultKey.UnexpectedError);
            }
        }

        public async Task<Result<IDictionary<string, string>>> GetSmsContentResources(string languageCode)
        {
            try
            {
                var smsContentBlob = await GetSmsContentBlob();

                var dictionary = smsContentBlob.Strings
                    .Select(entry => new
                    {
                        entry.Key,
                        Value = entry.GetTranslation(languageCode)
                    })
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Value);

                return Success<IDictionary<string, string>>(dictionary);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "There was a problem during fetching the Sms contents resources");
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

        private async Task<StringsBlob> GetEmailContentBlob()
        {
            var blobValue = await _nyssBlobProvider.GetEmailContentResources();

            return JsonSerializer.Deserialize<StringsBlob>(blobValue, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        private async Task<StringsBlob> GetSmsContentBlob()
        {
            var blobValue = await _nyssBlobProvider.GetSmsContentResources();

            return JsonSerializer.Deserialize<StringsBlob>(blobValue, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
