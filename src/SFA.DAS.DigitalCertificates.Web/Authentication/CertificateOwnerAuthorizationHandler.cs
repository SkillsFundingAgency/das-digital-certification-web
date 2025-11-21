using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Authentication
{
    public class CertificateOwnerAuthorizationHandler : AuthorizationHandler<CertificateOwnerRequirement>
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly IUserService _userService;

        public CertificateOwnerAuthorizationHandler(ISessionStorageService sessionStorageService, IUserService userService)
        {
            _sessionStorageService = sessionStorageService;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CertificateOwnerRequirement requirement)
        {
            var httpContext = context.GetHttpContext();

            if (!httpContext.Request.RouteValues.TryGetValue("certificateId", out var certificateIdFromRoute)
                || !Guid.TryParse(certificateIdFromRoute?.ToString(), out var certificateId))
            {
                context.Fail(new AuthorizationFailureReason(this, DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner));
                return;
            }

            var endpoint = httpContext.GetEndpoint();
            var routeName = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.RouteNameMetadata>()?.RouteName;

            CertificateType? certificateType = routeName switch
            {
                CertificatesController.CertificateStandardRouteGet => CertificateType.Standard,
                CertificatesController.CertificateFrameworkRouteGet => CertificateType.Framework,
                _ => null
            };

            var certificates = await _sessionStorageService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier()) ?? new List<Certificate>();

            var match = certificates.FirstOrDefault(p =>
                p.CertificateId == certificateId &&
                (certificateType == null || p.CertificateType == certificateType));

            if (match != null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(this, DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner));
            }
        }

    }
}
