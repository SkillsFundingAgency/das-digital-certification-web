using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.StartupExtensions;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("admin")]
    public class AdminController : BaseController
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        #region Routes
        public const string ClearMatchesRouteGet = nameof(ClearMatchesRouteGet);
        #endregion Routes

        public AdminController(ICacheService cacheService, IUserService userService,
            IConfiguration config, IHttpContextAccessor contextAccessor)
            : base(contextAccessor)
        {
            _cacheService = cacheService;
            _userService = userService;
            _config = config;
        }

        [Route("clear-matches", Name = ClearMatchesRouteGet)]
        public IActionResult RevertAuthorisation()
        {
            if (!_config.IsRunningInProd())
            {
                var govUkIdentifier = _userService.GetGovUkIdentifier();
                _cacheService.ClearMatches(govUkIdentifier);
            }

            return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
        }
    }
}