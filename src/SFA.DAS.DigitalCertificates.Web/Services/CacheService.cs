using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Application.Queries.GetMatches;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class CacheService : ICacheService
    {
        private const string DigitalCertificates = nameof(DigitalCertificates);
        private const string MatchesKey = "Matches";
        private const string MatchFailCountKey = "MatchFailCount";

        private readonly ICacheStorageService _cacheStorageService;
        private readonly IMediator _mediator;
        private readonly DigitalCertificatesWebConfiguration _configuration;

        public CacheService(ICacheStorageService cacheStorageService, IMediator mediator, DigitalCertificatesWebConfiguration configuration)
        {
            _cacheStorageService = cacheStorageService;
            _mediator = mediator;
            _configuration = configuration;
        }

        public async Task<User?> GetUserAsync(string govUkIdentifier)
        {
            var user = await _cacheStorageService.GetOrCreateAsync(GetScopedKey(nameof(User), govUkIdentifier), async e =>
            {
                // the user timeout is shorter because we need to detect a locked account
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return await _mediator.Send(new GetUserQuery { GovUkIdentifier = govUkIdentifier });
            });

            return user;
        }

        public async Task Clear(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(nameof(User), govUkIdentifier));
        }

        public async Task<MatchesAndMasks?> GetOrCreateMatchesAsync(string govUkIdentifier, Guid userId)
        {
            var days = _configuration?.MatchesCacheExpiryDays ?? 30;
            var expiry = TimeSpan.FromDays(days);

            return await _cacheStorageService.GetOrCreateAsync(GetScopedKey(MatchesKey, govUkIdentifier), async e =>
            {
                e.AbsoluteExpirationRelativeToNow = expiry;
                return await _mediator.Send(new GetMatchesQuery { UserId = userId });
            });
        }

        public async Task ClearMatches(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(MatchesKey, govUkIdentifier));
            await _cacheStorageService.RemoveAsync(GetScopedKey(MatchFailCountKey, govUkIdentifier));
        }

        public async Task<int> IncrementMatchFailCountAsync(string govUkIdentifier)
        {
            var key = GetScopedKey(MatchFailCountKey, govUkIdentifier);
            var current = await _cacheStorageService.GetAsync<int?>(key) ?? 0;
            var updated = current + 1;
            var days = _configuration?.MatchesCacheExpiryDays ?? 30;
            var expiry = TimeSpan.FromDays(days);

            await _cacheStorageService.SetAsync(key, async e =>
            {
                e.AbsoluteExpirationRelativeToNow = expiry;
                return await Task.FromResult(updated);
            });

            return updated;
        }

        internal static string GetScopedKey(string key, string identifier)
        {
            return $"{DigitalCertificates}:{key}:{identifier}";
        }
    }
}
