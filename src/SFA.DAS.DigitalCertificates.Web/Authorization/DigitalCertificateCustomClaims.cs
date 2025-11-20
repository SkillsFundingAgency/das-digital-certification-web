using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Authorization
{
    public class DigitalCertificateCustomClaims : ICustomClaims
    {
        private readonly IUserCacheService _userCacheService;

        public DigitalCertificateCustomClaims(IUserCacheService userCacheService)
        {
            _userCacheService = userCacheService;
        }

        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            return await GetClaims(tokenValidatedContext?.Principal);
        }

        public async Task<IEnumerable<Claim>> GetClaims(ClaimsPrincipal principal)
        {
            var claims = new List<Claim>();

            if (principal != null)
            {
                var user = await _userCacheService.CacheUserForGovUkIdentifier(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (user != null)
                {
                    claims.Add(new Claim(DigitalCertificateClaimsTypes.UserId,
                        user.Id.ToString()));

                    claims.Add(new Claim(ClaimTypes.AuthorizationDecision,
                        user.LockedAt.HasValue ? AuthorizationDecisions.Suspended : AuthorizationDecisions.Allowed));
                }
            }

            return claims;
        }
    }
}
