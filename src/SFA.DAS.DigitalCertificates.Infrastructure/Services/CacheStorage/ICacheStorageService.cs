using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage
{
    public interface ICacheStorageService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<DistributedCacheEntryOptions, Task<T>> factory, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string key);
        Task<T> SetAsync<T>(string key, Func<DistributedCacheEntryOptions, Task<T>> factory, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key);
    }
}
