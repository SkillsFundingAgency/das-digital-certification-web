using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Authentication
{
    public class NotUlnAuthorisedFailureHandler : IAuthorizationFailureHandler
    {
        private readonly LinkGenerator _linkGenerator;

        public NotUlnAuthorisedFailureHandler(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public Task<bool> HandleFailureAsync(HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult result)
        {
            var isNotUlnRequirementInPolicy = policy.Requirements.OfType<NotUlnAuthorisedRequirement>().Any();

            var isNotUlnRequirementFailed =
                result.AuthorizationFailure?.FailureReasons.Any(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotUlnAuthorized) == true;

            if (isNotUlnRequirementInPolicy && isNotUlnRequirementFailed)
            {
                var route = _linkGenerator.GetPathByName(context, CertificatesController.CertificatesListRouteGet, values: null);

                if (!string.IsNullOrEmpty(route))
                {
                    context.Response.Redirect(route);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
