using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services.SessionStorage;

namespace SFA.DAS.DigitalCertificates.Web.Authentication
{
    public class UlnAuthorisedAuthorizationHandler : AuthorizationHandler<UlnAuthorisedRequirement>
    {
        private readonly ISessionStorageService _sessionStorageService;

        public UlnAuthorisedAuthorizationHandler(ISessionStorageService sessionStorageService)
        {
            _sessionStorageService = sessionStorageService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UlnAuthorisedRequirement requirement)
        {
            var ulnAuthorised = await _sessionStorageService.GetUlnAuthorisationAsync();
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
