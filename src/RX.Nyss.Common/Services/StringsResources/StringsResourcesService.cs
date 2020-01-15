using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RX.Nyss.Common.Utils.DataContract;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Common.Services.StringsResources
{
    public interface IStringsResourcesService
    {
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
