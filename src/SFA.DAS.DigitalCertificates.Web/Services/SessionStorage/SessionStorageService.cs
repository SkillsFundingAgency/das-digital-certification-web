using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services.User;

namespace SFA.DAS.DigitalCertificates.Web.Services.SessionStorage
{
    public class SessionStorageService : ISessionStorageService
    {
        private const string DigitalCertificates = nameof(DigitalCertificates);
        private const int SessionTimeoutMinutes = 20;

        private readonly IUserService _userService;
        private readonly ICacheStorageService _sessionCache;
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public SessionStorageService(IUserService userService, ICacheStorageService sessionCache, IDigitalCertificatesOuterApi outerApi)
        {
            _userService = userService;
            _sessionCache = sessionCache;
            _outerApi = outerApi;
        }

        public async Task<UserResponse> GetUserAsync()
        {
            return await GetUserAsync(_userService.GetGovUkIdentifier());
        }

        public async Task<List<Certificate>> GetOwnedCertificatesAsync()
        {
            var response = await GetCertificatesAsync();
            return response.Certificates;
        }

        public async Task<UlnAuthorisation> GetUlnAuthorisationAsync()
        {
            var response = await GetCertificatesAsync();
            return response.Authorisation;
        }

        private async Task<UserResponse> GetUserAsync(string govUkIdentifier)
        {
            var user = await _sessionCache.GetOrCreateAsync(GetScopedKey(nameof(UserResponse), govUkIdentifier), async e =>
            {
                // the user timeout is shorter because we need to detect a locked account
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return await _outerApi.GetUser(govUkIdentifier);
            });

            return user;
        }

        private async Task<CertificatesResponse> GetCertificatesAsync()
        {
            var govUkIdentifier = _userService.GetGovUkIdentifier();
            var response = await _sessionCache.GetOrCreateAsync(GetScopedKey(nameof(CertificatesResponse), govUkIdentifier), async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionTimeoutMinutes);
                
                var user = await GetUserAsync(govUkIdentifier);
                return await _outerApi.GetCertificates(user.Id);
            });

            return response;
        }

        private async Task<T> Get<T>(string key)
        {
            var scopedKey = GetScopedKey(key, _userService.GetGovUkIdentifier());
            return await _sessionCache.GetAsync<T>(scopedKey);
        }

        private async Task Set<T>(string key, T value)
        {
            var scopedKey = GetScopedKey(key, _userService.GetGovUkIdentifier());
            await _sessionCache.SetAsync($"{scopedKey}", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SessionTimeoutMinutes);
                return await Task.FromResult(value);
            });
        }

        private static string GetScopedKey(string key, string identifier)
        {
            return $"{DigitalCertificates}:{key}:{identifier}";
        }
    }
}
