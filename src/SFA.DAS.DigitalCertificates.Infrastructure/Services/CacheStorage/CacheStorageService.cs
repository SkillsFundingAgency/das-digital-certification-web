using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

            return await SetAsync(key, factory, cancellationToken);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var json = await _distributedCache.GetStringAsync(key);
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T> SetAsync<T>(string key, 
            Func<DistributedCacheEntryOptions, Task<T>> factory,
            CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions();
            var value = await factory(options);

            if (!EqualityComparer<T>.Default.Equals(value, default))
            {
                var json = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, json, options, cancellationToken);
            }

            return value;
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}
