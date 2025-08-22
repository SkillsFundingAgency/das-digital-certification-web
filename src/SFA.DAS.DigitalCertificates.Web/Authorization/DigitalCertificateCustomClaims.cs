using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.GovUK.Auth.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Authorization
{
    public class DigitalCertificateCustomClaims : ICustomClaims
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public DigitalCertificateCustomClaims(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<IEnumerable<Claim?>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            return await GetClaims(tokenValidatedContext?.Principal);
        }

        public async Task<IEnumerable<Claim>> GetClaims(ClaimsPrincipal? principal)
        {
            var govUkIdentifier = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _outerApi.GetUser(govUkIdentifier);

            var claims = new List<Claim>();
            claims.Add(new Claim(DigitalCertificateClaimsTypes.UserId, user.Id.ToString()));

            if (user.LockedAt.HasValue)
            {    
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
            }

            return await Task.FromResult(claims);
        }
    }
}
