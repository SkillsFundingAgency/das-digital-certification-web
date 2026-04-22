using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Authentication
{
    public class NotUlnAuthorisedAuthorizationHandler : AuthorizationHandler<NotUlnAuthorisedRequirement>
    {
        private readonly ISessionService _sessionService;

        public NotUlnAuthorisedAuthorizationHandler(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotUlnAuthorisedRequirement requirement)
        {
            var ulnAuthorised = await _sessionService.GetUlnAuthorisationAsync();
            if (ulnAuthorised == null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
