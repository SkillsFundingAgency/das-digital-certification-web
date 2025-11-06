using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;
        private readonly ICacheStorageService _cacheStorageService;

        public UserCacheService(IDigitalCertificatesOuterApi outerApi, ICacheStorageService cacheStorageService)
        {
            _outerApi = outerApi;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<UserResponse> CacheUserForGovUkIdentifier(string govUkIdentifier)
        {
            var user = await _cacheStorageService.GetOrCreateAsync($"User:{govUkIdentifier}", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return await _outerApi.GetUser(govUkIdentifier);
            });

            return user;
        }
    }
}
