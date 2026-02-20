using System.Collections.Generic;
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
        private readonly ICacheService _cacheService;

        public DigitalCertificateCustomClaims(IMediator mediator, ICacheService cacheService)
        {
            _mediator = mediator;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
        {
            return await GetClaims(tokenValidatedContext?.Principal);
        }

        public async Task<IEnumerable<Claim>> GetClaims(ClaimsPrincipal? principal)
        {
            var claims = new List<Claim>();

            if (principal != null)
            {
                var govUkIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = principal.FindFirstValue(ClaimTypes.Email);

                if (govUkIdentifier != null && email != null)
                {
                    await _mediator.Send(new CreateOrUpdateUserCommand
                    {
                        GovUkIdentifier = govUkIdentifier,
                        EmailAddress = email,
                        PhoneNumber = principal.FindFirstValue(ClaimTypes.MobilePhone)
                    });

                    var user = await _cacheService.GetUserAsync(govUkIdentifier);
                    if (user != null)
                    {
                        claims.Add(new Claim(DigitalCertificateClaimsTypes.UserId,
                            user.Id.ToString()));

                        claims.Add(new Claim(ClaimTypes.AuthorizationDecision,
                            user.IsLocked ? AuthorizationDecisions.Suspended : AuthorizationDecisions.Allowed));
                    }
                }
            }

            return claims;
        }
    }
}
