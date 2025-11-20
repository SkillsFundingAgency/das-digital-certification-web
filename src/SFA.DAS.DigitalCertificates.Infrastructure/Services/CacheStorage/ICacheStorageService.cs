using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage
{
    public interface ICacheStorageService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<DistributedCacheEntryOptions, Task<T>> factory, CancellationToken cancellationToken = default);
        Task<T> GetAsync<T>(string key);
        Task CreateAsync<T>(string key, T item, int expirationInHours);
        Task RemoveAsync(string key);
    }
}
