using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("service")]
    public class ServiceController : BaseController
    {
        private readonly IConfiguration _config;

        #region Routes
        public const string SigningOutRouteGet = nameof(SigningOutRouteGet);
        public const string SignedOutRouteGet = nameof(SignedOutRouteGet);
        #endregion Routes

        public ServiceController(IConfiguration config, IHttpContextAccessor contextAccessor)
            : base(contextAccessor) 
        {
            _config = config;
        }

        [Route("signout", Name = SigningOutRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
        public async Task<IActionResult> SigningOut()
        {
            if (HttpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("No HttpContext available.");
            }

            var idToken = await HttpContextAccessor.HttpContext
                .GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, OpenIdConnectParameterNames.IdToken);

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters[OpenIdConnectParameterNames.IdTokenHint] = idToken;

            var authenticationSchemes = new[] { CookieAuthenticationDefaults.AuthenticationScheme };
            if (!bool.TryParse(_config["StubAuth"], out var stubAuth) || !stubAuth)
            {
                authenticationSchemes = authenticationSchemes
                    .Append(OpenIdConnectDefaults.AuthenticationScheme)
                    .ToArray();
            }

            return SignOut(
                authenticationProperties,
                authenticationSchemes);
        }

        [Route("signed-out", Name = SignedOutRouteGet)]
        public IActionResult SignedOut()
        {
            if (HttpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("No HttpContext available.");
            }

            HttpContextAccessor.HttpContext.Response.Cookies.Delete("SFA.DAS.DigitalCertificates.Web.Auth");

            return View();
        }
    }
}
