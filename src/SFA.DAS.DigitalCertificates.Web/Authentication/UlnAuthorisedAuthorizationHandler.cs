using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Authentication
{
    public class UlnAuthorisedAuthorizationHandler : AuthorizationHandler<UlnAuthorisedRequirement>
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;

        public UlnAuthorisedAuthorizationHandler(ICacheService cacheService, IUserService userService)
        {
            _cacheService = cacheService;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UlnAuthorisedRequirement requirement)
        {
            var ulnAuthorised = await _cacheService.GetUlnAuthorisationAsync(_userService.GetGovUkIdentifier());
            if (ulnAuthorised != null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(this, DigitalCertificatesAuthorizationFailureMessages.NotUlnAuthorized));
            }
        }

    }
}
