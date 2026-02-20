using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class CacheService : ICacheService
    {
        private const string DigitalCertificates = nameof(DigitalCertificates);

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

        public async Task Clear(string govUkIdentifier)
        {
            await _cacheStorageService.RemoveAsync(GetScopedKey(nameof(User), govUkIdentifier));
        }

        internal static string GetScopedKey(string key, string identifier)
        {
            return $"{DigitalCertificates}:{key}:{identifier}";
        }
    }
}
