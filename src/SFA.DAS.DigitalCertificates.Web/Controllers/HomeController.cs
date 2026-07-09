using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Domain.Extensions;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Infrastructure;
using SFA.DAS.DigitalCertificates.Web.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        private readonly IHomeOrchestrator _homeOrchestrator;
        private readonly ILogger<HomeController> _logger;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;

        #region Routes
        public const string VerifiedRouteGet = nameof(VerifiedRouteGet);
        public const string CheckRouteGet = nameof(CheckRouteGet);
        public const string HelpRouteGet = nameof(HelpRouteGet);
        public const string LockedRouteGet = nameof(LockedRouteGet);
        public const string CookiesRouteGet = nameof(CookiesRouteGet);
        public const string CookiesRoutePost = nameof(CookiesRoutePost);
        public const string CookieDetailsRouteGet = nameof(CookieDetailsRouteGet);
        public const string ErrorRouteGet = nameof(ErrorRouteGet);
        public const string SignOutRouteGet = nameof(SignOutRouteGet);
        public const string UserSignedOutRouteGet = nameof(UserSignedOutRouteGet);
        public const string AccessibilityStatementRouteGet = nameof(AccessibilityStatementRouteGet);
        #endregion Routes

        public HomeController(IHttpContextAccessor contextAccessor, IHomeOrchestrator homeOrchestrator, ILogger<HomeController> logger, DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration)
            : base(contextAccessor)
        {
            _homeOrchestrator = homeOrchestrator;
            _logger = logger;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
        }

        [Route("start-page")]
        public IActionResult Index()
        {
            if(!string.IsNullOrWhiteSpace(_digitalCertificatesWebConfiguration.ExternalStartPage))
            {
                return Redirect(_digitalCertificatesWebConfiguration.ExternalStartPage);
            }

            return View();
        }

        [Route("check", Name = CheckRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsActiveAccount))]
        public IActionResult Check()
        {
            return View();
        }

        [Route("verified", Name = VerifiedRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsVerified))]
        public async Task<IActionResult> Verified()
        {
            return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
        }

        [Route("locked", Name = LockedRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
        public IActionResult Locked()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("cookies", Name = CookiesRouteGet)]
        public IActionResult Cookies(string? returnUrl = null)
        {
            var analyticsCookieValue = Request.Cookies[CookieKeys.AnalyticsConsent];            

            _ = bool.TryParse(analyticsCookieValue, out var isAnalyticsCookieConsentGiven);            

            var cookieViewModel = new CookiesViewModel
            {                
                ConsentAnalyticsCookie = isAnalyticsCookieConsentGiven,
                BackUrl = GetSafeReturnUrl(returnUrl)
            };
            return View(cookieViewModel);
        }
        
        [AllowAnonymous]
        [Route("help", Name = HelpRouteGet)]
        public IActionResult Help(string? returnUrl = null)
        {
            var model = new PageViewModel
            {
                BackUrl = GetSafeReturnUrl(returnUrl)
            };

            return View(model);
        }

        [Route("cookie-details", Name = CookieDetailsRouteGet)]
        public IActionResult CookieDetails()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("accessibility-statement", Name = AccessibilityStatementRouteGet)]
        public IActionResult AccessibilityStatement(string? returnUrl = null)
        {
            var model = new PageViewModel
            {
                BackUrl = GetSafeReturnUrl(returnUrl)
            };

            return View(model);
        }

        [Route("error/403")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Route("error", Name = ErrorRouteGet)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string errorMessage)
        {
            _logger.LogError(errorMessage.SanitizeLogData());
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContextAccessor?.HttpContext?.TraceIdentifier, ErrorMessage = errorMessage });
        }

        private string GetSafeReturnUrl(string? returnUrl, string fallbackUrl = "")
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return fallbackUrl;
            }

            return Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : fallbackUrl;
        }
    }
}