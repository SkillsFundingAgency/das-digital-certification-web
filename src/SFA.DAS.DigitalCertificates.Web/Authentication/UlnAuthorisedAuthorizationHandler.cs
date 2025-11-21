using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Authentication
{
    public class UlnAuthorisedAuthorizationHandler : AuthorizationHandler<UlnAuthorisedRequirement>
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly IUserService _userService;

        public UlnAuthorisedAuthorizationHandler(ISessionStorageService sessionStorageService, IUserService userService)
        {
            _sessionStorageService = sessionStorageService;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UlnAuthorisedRequirement requirement)
        {
            var ulnAuthorised = await _sessionStorageService.GetUlnAuthorisationAsync(_userService.GetGovUkIdentifier());
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
