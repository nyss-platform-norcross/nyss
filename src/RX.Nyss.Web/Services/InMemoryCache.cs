using System;
using Microsoft.Extensions.Caching.Memory;

namespace RX.Nyss.Web.Services
{
    public interface IInMemoryCache
    {
        void Store<T>(string key, T value, TimeSpan validFor)
            where T : class;

        T Get<T>(string key)
            where T : class;

        void Remove(string key);
    }

    public class InMemoryCache : IInMemoryCache
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Store<T>(string key, T value, TimeSpan validFor) where T : class
        {
            if (value == default(T))
            {
                return;
            }

            _memoryCache.Set(key, value, DateTimeOffset.UtcNow.AddMilliseconds(validFor.TotalMilliseconds));
        }

        public T Get<T>(string key) where T : class =>
            _memoryCache.TryGetValue(key, out T value)
                ? value
                : null;

        public void Remove(string key) =>
            _memoryCache.Remove(key);
    }
}
