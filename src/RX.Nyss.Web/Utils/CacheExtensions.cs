using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Utils
{
    public static class CacheExtensions
    {
        public static async Task<TResult> GetCachedValue<TResult>(this IInMemoryCache memoryCache, string key, TimeSpan validFor, Func<Task<TResult>> value)
            where TResult : class
        {
            var cachedResult = memoryCache.Get<TResult>(key);

            if (cachedResult != null)
            {
                return cachedResult;
            }

            var result = await value();

            if (result != null)
            {
                memoryCache.Store(key, result, validFor);
            }

            return result;
        }

        public static async Task<Result<TResult>> GetCachedResult<TResult>(this IInMemoryCache memoryCache, string key, TimeSpan validFor, Func<Task<Result<TResult>>> value)
            where TResult : class
        {
            var cachedResult = memoryCache.Get<TResult>(key);

            if (cachedResult != null)
            {
                return Success(cachedResult);
            }

            var result = await value();

            if (result.IsSuccess && result.Value != null)
            {
                memoryCache.Store(key, result.Value, validFor);
            }

            return result;
        }
    }
}
