using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage
{
    public class CacheStorageService : ICacheStorageService
    {
        private readonly IDistributedCache _distributedCache;

        public CacheStorageService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<DistributedCacheEntryOptions, Task<T>> factory,
            CancellationToken cancellationToken = default)
        {
            var cached = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<T>(cached);
            }

            var options = new DistributedCacheEntryOptions();
            var value = await factory(options);

            if (!EqualityComparer<T>.Default.Equals(value, default))
            {
                var json = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, json, options, cancellationToken);
            }

            return value;
        }

        public async Task CreateAsync<T>(string key, T item, int expirationInHours)
        {
            var json = JsonConvert.SerializeObject(item);

            await _distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(expirationInHours)
            });
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var json = await _distributedCache.GetStringAsync(key);
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}
