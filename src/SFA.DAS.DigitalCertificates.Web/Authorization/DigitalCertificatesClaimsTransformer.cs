using Microsoft.AspNetCore.Authentication;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Authorization
{
    public sealed class DigitalCertificatesClaimsTransformer : IClaimsTransformation
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;
        private readonly ICacheStorageService _cacheStorageService;

        public DigitalCertificatesClaimsTransformer(IDigitalCertificatesOuterApi outerApi, ICacheStorageService cacheStorageService)
        {
            _outerApi = outerApi;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true) return principal;

            var userId = principal.FindFirst(DigitalCertificateClaimsTypes.UserId)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var govUkIdentifier = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(govUkIdentifier)) return principal;

                var user = await _cacheStorageService.GetOrCreateAsync($"user:{govUkIdentifier}", async e =>
                {
                    e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                    return await _outerApi.GetUser(govUkIdentifier);
                });

                var authorizationDecisionClaim = principal.FindFirst(ClaimTypes.AuthorizationDecision);
                if (authorizationDecisionClaim != null)
                {
                    var authorizationDecision = authorizationDecisionClaim.Value;
                    if (!string.IsNullOrEmpty(authorizationDecision))
                    {
                        var userSuspended = user.LockedAt.HasValue ? "Suspended" : string.Empty;
                        if (userSuspended != authorizationDecision)
                        {
                            principal.Identities.First().RemoveClaim(authorizationDecisionClaim);
                            principal.Identities.First().AddClaim(new Claim(ClaimTypes.AuthorizationDecision, userSuspended));
                        }
                    }
                }
            }

            return principal;
        }
    }
}
