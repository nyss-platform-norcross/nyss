using System;
using Microsoft.Extensions.Caching.Memory;

namespace RX.Nyss.Web.Services
{
    public interface IInMemoryCache
    {
        void Store<T>(string key, T value, TimeSpan validFor)
            where T : class;

        T? Get<T>(string key)
            where T : class;

        void Remove(string key);
    }

    public class InMemoryCache : IInMemoryCache
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public void Store<T>(string key, T value, TimeSpan validFor) where T : class
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) return;

            _memoryCache.Set(key, value, validFor);
        }

        public T? Get<T>(string key) where T : class
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return _memoryCache.TryGetValue(key, out T? value) ? value : null;
        }

        public void Remove(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            _memoryCache.Remove(key);
        }
    }
}