using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class CacheService : ICacheService
    {
        private const string DigitalCertificates = nameof(DigitalCertificates);
        private const int SessionTimeoutMinutes = 20;

        private readonly ICacheStorageService _cacheStorageService;
        private readonly IMediator _mediator;

        public CacheService(ICacheStorageService cacheStorageService, IMediator mediator)
        {
            _cacheStorageService = cacheStorageService;
            _mediator = mediator;
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

        public async Task<List<Certificate>?> GetOwnedCertificatesAsync(string govUkIdentifier)
        {
            var response = await GetCertificatesAsync(govUkIdentifier);
            return response?.Certificates;
        }

        public async Task<UlnAuthorisation?> GetUlnAuthorisationAsync(string govUkIdentifier)
        {
            var response = await GetCertificatesAsync(govUkIdentifier);
            return response?.Authorisation;
        }

        private async Task<GetCertificatesQueryResult?> GetCertificatesAsync(string govUkIdentifier)
        {
            var response = await _cacheStorageService.GetOrCreateAsync(GetScopedKey(nameof(CertificatesResponse), govUkIdentifier), async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionTimeoutMinutes);

                var user = await GetUserAsync(govUkIdentifier);
                if (user != null)
                {
                    return await _mediator.Send(new GetCertificatesQuery { UserId = user.Id });
                }

                return null;
            });

            return response;
        }

        public async Task Clear(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(nameof(User), govUkIdentifier));
            await _cacheStorageService.RemoveAsync(GetScopedKey(nameof(CertificatesResponse), govUkIdentifier));
        }

        private async Task<T?> Get<T>(string key, string govUkIdentifier)
        {
            var scopedKey = GetScopedKey(key, govUkIdentifier);
            return await _cacheStorageService.GetAsync<T?>(scopedKey);
        }

        private async Task Set<T>(string key, string govUkIdentifier, T value)
        {
            var scopedKey = GetScopedKey(key, govUkIdentifier);
            await _cacheStorageService.SetAsync($"{scopedKey}", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionTimeoutMinutes);
                return await Task.FromResult(value);
            });
        }

        internal static string GetScopedKey(string key, string identifier)
        {
            return $"{DigitalCertificates}:{key}:{identifier}";
        }
    }
}
