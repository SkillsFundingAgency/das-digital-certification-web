using System;
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
    public class CertificateOwnerFailureHandler : IAuthorizationFailureHandler
    {
        private readonly LinkGenerator _linkGenerator;

        public CertificateOwnerFailureHandler(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public Task<bool> HandleFailureAsync(HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult result)
        {
            var isCertificateOwnerRequirementInPolicy =
                policy.Requirements.OfType<CertificateOwnerRequirement>().Any();

            var isCertificateOwnerRequirementFailed =
                result.AuthorizationFailure?.FailureReasons.Any(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner) == true;

            if (isCertificateOwnerRequirementInPolicy && isCertificateOwnerRequirementFailed)
            {
                var path = context.Request.Path;
                if (path.StartsWithSegments("/" + CertificatesController.BaseRoute, StringComparison.OrdinalIgnoreCase))
                {
                    var route = _linkGenerator.GetPathByName(context, CertificatesController.CertificatesListRouteGet, values: null);

                    if (!string.IsNullOrEmpty(route))
                    {
                        context.Response.Redirect(route);
                        return Task.FromResult(true);
                    }
                }
            }

            return Task.FromResult(false);
        }
    }
}