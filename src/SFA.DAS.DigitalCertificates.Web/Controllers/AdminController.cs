using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("admin")]
    public class AdminController : BaseController
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;

        #region Routes
        public const string ClearMatchesRouteGet = nameof(ClearMatchesRouteGet);
        public const string ShowMatchesRouteGet = nameof(ShowMatchesRouteGet);
        #endregion Routes

        public AdminController(ICacheService cacheService, IUserService userService,
            DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration, IHttpContextAccessor contextAccessor)
            : base(contextAccessor)
        {
            _cacheService = cacheService;
            _userService = userService;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
        }

        [Route("clear-matches", Name = ClearMatchesRouteGet)]
        public async Task<IActionResult> RevertAuthorisation()
        {
            if (_digitalCertificatesWebConfiguration.AdminIsEnabled)
            {
                var govUkIdentifier = _userService.GetGovUkIdentifier();
                await _cacheService.ClearMatches(govUkIdentifier);
            }

            return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
        }

        [Route("show-matches", Name = ShowMatchesRouteGet)]
        public async Task<IActionResult> ShowMatches()
        {
            if (_digitalCertificatesWebConfiguration.AdminIsEnabled)
            {
                var govUkIdentifier = _userService.GetGovUkIdentifier();
                var matches = await _cacheService.GetOrCreateMatchesAsync(govUkIdentifier, 
                    _userService.GetUserId().GetValueOrDefault());

                var matchFailCount = await _cacheService.GetMatchFailCountAsync(govUkIdentifier);

                return Json(new
                {
                    matches,
                    matchFailCount
                });
            }

            return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
        }
    }
}