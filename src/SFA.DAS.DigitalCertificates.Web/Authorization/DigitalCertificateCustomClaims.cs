using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateOrUpdateUser;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.DigitalCertificates.Web.Authorization
{
    public class DigitalCertificateCustomClaims : ICustomClaims
    {
        private readonly IMediator _mediator;
        private readonly IUserCacheService _userCacheService;

        public DigitalCertificateCustomClaims(IMediator mediator, IUserCacheService userCacheService)
        {
            _mediator = mediator;
            _userCacheService = userCacheService;
        }

        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            return await GetClaims(tokenValidatedContext?.Principal);
        }

        public async Task<IEnumerable<Claim>> GetClaims(ClaimsPrincipal principal)
        {
            var claims = new List<Claim>();

            var govUkIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var mobilePhone = principal.FindFirstValue(ClaimTypes.MobilePhone);

            await _mediator.Send(new CreateOrUpdateUserCommand
            {
                GovUkIdentifier = govUkIdentifier,
                EmailAddress = email,
                PhoneNumber = mobilePhone
            });

            var user = await _userCacheService.CacheUserForGovUkIdentifier(govUkIdentifier);
            if (user != null)
            {
                claims.Add(new Claim(DigitalCertificateClaimsTypes.UserId, 
                    user.Id.ToString()));
                
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, 
                    user.LockedAt.HasValue ? AuthorizationDecisions.Suspended : AuthorizationDecisions.Allowed));
            }

            return claims;
        }
    }
}
