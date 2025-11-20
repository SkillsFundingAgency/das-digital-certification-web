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
using SFA.DAS.DigitalCertificates.Web.Models.User;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        private readonly IHomeOrchestrator _homeOrchestrator;
        private readonly IConfiguration _config;
        private readonly IGovUkAuthenticationService _govUkAuthenticationService;
        private readonly ILogger<HomeController> _logger;
        
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
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger)
            : base(contextAccessor)
        {
            _homeOrchestrator = homeOrchestrator;
            _config = config;
            _govUkAuthenticationService = govUkAuthenticationService;
            _logger = logger;
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContextAccessor.HttpContext.TraceIdentifier, ErrorMessage = errorMessage });
        }
    }
}