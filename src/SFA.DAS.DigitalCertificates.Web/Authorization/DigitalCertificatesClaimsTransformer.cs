using Microsoft.AspNetCore.Authentication;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Authorization
{
    public sealed class DigitalCertificatesClaimsTransformer : IClaimsTransformation
    {
        private readonly ICacheService _cacheService;

        public DigitalCertificatesClaimsTransformer(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true) return principal!;

            var userId = principal.FindFirst(DigitalCertificateClaimsTypes.UserId)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var govUkIdentifier = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(govUkIdentifier)) return principal;

                var user = await _cacheService.GetUserAsync(govUkIdentifier);
                if (user != null)
                {
                    var authorizationDecisionClaim = principal.FindFirst(ClaimTypes.AuthorizationDecision);
                    if (authorizationDecisionClaim != null)
                    {
                        var authorizationDecision = authorizationDecisionClaim.Value;
                        if (!string.IsNullOrEmpty(authorizationDecision))
                        {
                            var userAuthorizationDecision = user.LockedAt.HasValue ? AuthorizationDecisions.Suspended : AuthorizationDecisions.Allowed;
                            if (userAuthorizationDecision != authorizationDecision)
                            {
                                principal.Identities.First().RemoveClaim(authorizationDecisionClaim);
                                principal.Identities.First().AddClaim(new Claim(ClaimTypes.AuthorizationDecision, userAuthorizationDecision));
                            }
                        }
                    }
                }
            }

            return principal;
        }
    }
}
