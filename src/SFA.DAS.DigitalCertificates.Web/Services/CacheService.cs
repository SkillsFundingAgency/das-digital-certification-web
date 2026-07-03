using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.UpdateUserIdentity;
using SFA.DAS.DigitalCertificates.Application.Queries.GetMatches;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.GovUK.Auth.Models;

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

        public CacheService(ICacheStorageService cacheStorageService, IMediator mediator, 
            DigitalCertificatesWebConfiguration configuration)
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

        public async Task<MatchesAndMasks?> GetMatchesAsync(string govUkIdentifier)
        {
            var key = GetScopedKey(MatchesKey, govUkIdentifier);
            var cached = await _cacheStorageService.GetAsync<MatchesAndMasks?>(key);

            if (cached != null)
            {
                if (cached.Matches?.Any() == true && cached.Matches.All(p => p.UserIdentityId != null))
                {
                    return cached;
                }

                await _cacheStorageService.RemoveAsync(key);
            }

            return null;
        }

        public async Task<MatchesAndMasks?> CreateMatchesAsync(string govUkIdentifier, Guid userId, GovUkCredentialSubject govUkCredentialSubject)
        {
            var key = GetScopedKey(MatchesKey, govUkIdentifier);
            await _mediator.Send(new UpdateUserIdentityCommand
            {
                UserId = userId,
                Names = govUkCredentialSubject.GetHistoricalNames()
                    .Select(x => new Name
                    {
                        ValidSince = x.ValidFrom,
                        ValidUntil = x.ValidUntil,
                        FamilyName = x.FamilyNames,
                        GivenNames = x.GivenNames
                    })
                    .ToList(),
                DateOfBirth = govUkCredentialSubject.BirthDates
                    .OrderByDescending(p => p.ValidUntil)
                    .First().Value
                    .ParseEnGbDateTime()
            });

            var matches = await _mediator.Send(new GetMatchesQuery { UserId = userId });
            if (matches?.Matches?.Any() ?? false)
            {
                var days = _configuration?.MatchesCacheExpiryDays ?? 30;
                return await _cacheStorageService.SetAsync(key, async e =>
                {
                    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(days);
                    return await Task.FromResult(matches);
                });
            }

            return null;
        }

        public async Task<int> GetMatchFailCountAsync(string govUkIdentifier)
        {
            var key = GetScopedKey(MatchFailCountKey, govUkIdentifier);
            var current = await _cacheStorageService.GetAsync<int?>(key) ?? 0;
            return current;
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

        public async Task ClearUser(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(nameof(User), govUkIdentifier));
        }

        public async Task ClearMatchFailCountAsync(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(MatchFailCountKey, govUkIdentifier));
        }

        public async Task ClearMatches(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(MatchesKey, govUkIdentifier));
            await ClearMatchFailCountAsync(govUkIdentifier);
        }

        internal static string GetScopedKey(string key, string identifier)
        {
            return $"{DigitalCertificates}:{key}:{identifier}";
        }
    }
}
