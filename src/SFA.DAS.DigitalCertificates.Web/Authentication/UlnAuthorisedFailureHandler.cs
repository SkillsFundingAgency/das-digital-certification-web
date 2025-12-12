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
    public class UlnAuthorisedFailureHandler : IAuthorizationFailureHandler
    {
        private readonly LinkGenerator _linkGenerator;

        public UlnAuthorisedFailureHandler(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public Task<bool> HandleFailureAsync(HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult result)
        {
            var isUlnAuthorisedRequirementInPolicy =
                policy.Requirements.OfType<UlnAuthorisedRequirement>().Any();

            var isUlnAuthorisedRequirementFailed =
                result.AuthorizationFailure?.FailureReasons.Any(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotUlnAuthorized) == true;

            if (isUlnAuthorisedRequirementInPolicy && isUlnAuthorisedRequirementFailed)
            {
                var route = _linkGenerator.GetPathByName(context, AuthoriseController.AuthoriseStartRouteGet, values: null);

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