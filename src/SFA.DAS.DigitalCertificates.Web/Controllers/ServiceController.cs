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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("service")]
    public class ServiceController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly ILogger<ServiceController> _logger;

        #region Routes
        public const string SignOutRouteGet = nameof(SignOutRouteGet);
        public const string SignedOutRouteGet = nameof(SignedOutRouteGet);
        #endregion Routes

        public ServiceController(IUserService userService, ICacheService cacheService, ISessionService sessionService, 
            IConfiguration config, ILogger<ServiceController> logger, IHttpContextAccessor contextAccessor)
            : base(contextAccessor)
        {
            _userService = userService;
            _cacheService = cacheService;
            _sessionService = sessionService;
            _config = config;
            _logger = logger;
        }

        [Route("signout", Name = SignOutRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
        public async Task<IActionResult> ServiceSignOut()
        {
            var govUkIdentifier = _userService.GetGovUkIdentifier();
            if (govUkIdentifier != null)
            {
                await TryCleanupAsync(
                    () => _cacheService.ClearUser(govUkIdentifier),
                    "Failed to clear the user cache during sign out.");
            }

            await TryCleanupAsync(
                () => _sessionService.ClearSessionDataAsync(),
                "Failed to clear session data during sign out.");

            var authenticationProperties = new AuthenticationProperties();

            var idToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                authenticationProperties.Parameters[
                    OpenIdConnectParameterNames.IdTokenHint] = idToken;
            }

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
            return View();
        }

        private async Task TryCleanupAsync(Func<Task> cleanup, string warningMessage)
        {
            try
            {
                await cleanup();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "{WarningMessage}", warningMessage);
            }
        }
    }
}
