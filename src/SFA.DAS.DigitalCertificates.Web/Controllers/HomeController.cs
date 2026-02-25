using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Domain.Extensions;
using SFA.DAS.DigitalCertificates.Web.Exceptions;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.DigitalCertificates.Web.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Home;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        private readonly IHomeOrchestrator _homeOrchestrator;
        private readonly IConfiguration _config;
        private readonly IGovUkAuthenticationService _govUkAuthenticationService;
        private readonly ILogger<HomeController> _logger;
        private readonly ISessionService _sessionService;

        #region Routes
        public const string VerifiedRouteGet = nameof(VerifiedRouteGet);
        public const string CheckRouteGet = nameof(CheckRouteGet);
        public const string LockedRouteGet = nameof(LockedRouteGet);
        public const string CookiesRouteGet = nameof(CookiesRouteGet);
        public const string CookieDetailsRouteGet = nameof(CookieDetailsRouteGet);
        public const string ErrorRouteGet = nameof(ErrorRouteGet);
        public const string SignOutRouteGet = nameof(SignOutRouteGet);
        public const string UserSignedOutRouteGet = nameof(UserSignedOutRouteGet);
        #endregion Routes

        public HomeController(IHomeOrchestrator homeOrchestrator,
            IConfiguration config, IGovUkAuthenticationService govUkAuthenticationService,
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger,
            ISessionService sessionService)
            : base(contextAccessor)
        {
            _homeOrchestrator = homeOrchestrator;
            _config = config;
            _govUkAuthenticationService = govUkAuthenticationService;
            _logger = logger;
            _sessionService = sessionService;
        }

        [Route("start-page")]
        public IActionResult Index()
        {
            if (!_config.IsRunningInProd())
            {
                // this view is replaced with a public page on GOV.UK in production
                return View();
            }

            return RedirectToRoute(CheckRouteGet);
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
            if (HttpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("No HttpContext available.");
            }

            var token = await HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var details = await _govUkAuthenticationService.GetAccountDetails(token);

            if (details == null)
                throw new VerifyException("Unable to load verify details");

            await _homeOrchestrator.CreateOrUpdateUser(new CreateOrUpdateUserModel
            {
                GovUkIdentifier = details.Sub,
                EmailAddress = details.Email,
                PhoneNumber = details.PhoneNumber,
                Names = details.CoreIdentityJwt.Vc.CredentialSubject
                    .GetHistoricalNames().Select(x => new NameModel
                    {
                        ValidSince = x.ValidFrom,
                        ValidUntil = x.ValidUntil,
                        FamilyName = x.FamilyNames,
                        GivenNames = x.GivenNames
                    }).ToList(),
                DateOfBirth = details.CoreIdentityJwt.Vc.CredentialSubject.BirthDates
                        .OrderByDescending(p => p.ValidUntil)
                        .First().Value
                        .ParseEnGbDateTime()
            });

            // TODO: DisplayName is currently handled temporarily to support testing of the sharing email functionality. It will be updated to align with the agreed requirements as part of an upcoming ticket.

            var historicalNames = details.CoreIdentityJwt?.Vc?.CredentialSubject?.GetHistoricalNames();
            var selectedName = (historicalNames != null)
                ? historicalNames.FirstOrDefault(n =>
                {
                    var now = DateTime.UtcNow;
                    var validFrom = n.ValidFrom;
                    var validUntil = n.ValidUntil;
                    var fromOk = validFrom == null || validFrom <= now;
                    var untilOk = validUntil == null || validUntil >= now;
                    return fromOk && untilOk;
                })
                : null;

            // fallback to the first historical name if no currently valid one found
            if (selectedName == null && historicalNames != null)
            {
                selectedName = historicalNames.FirstOrDefault();
            }

            var given = selectedName?.GivenNames ?? string.Empty;
            var family = selectedName?.FamilyNames ?? string.Empty;
            var displayName = string.IsNullOrWhiteSpace(given) ? family : (string.IsNullOrWhiteSpace(family) ? given : $"{given} {family}");

            await _sessionService.SetUserDetailsAsync(new UserDetails { GivenNames = given, FamilyName = family, FullName = displayName });

            return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
        }

        [Route("locked", Name = LockedRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
        public IActionResult Locked()
        {
            return View();
        }

        [Route("cookies", Name = CookiesRouteGet)]
        public IActionResult Cookies()
        {
            return View();
        }

        [Route("cookie-details", Name = CookieDetailsRouteGet)]
        public IActionResult CookieDetails()
        {
            return View();
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
    }
}