using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("authorise")]
    public class AuthoriseController : BaseController
    {
        #region Routes
        public const string NeedMoreInformationRouteGet = nameof(NeedMoreInformationRouteGet);
        #endregion

        private readonly ISessionService _sessionService;
        private readonly IAuthoriseOrchestrator _authoriseOrchestrator;

        public AuthoriseController(IHttpContextAccessor httpContextAccessor, ISessionService sessionService, IAuthoriseOrchestrator authoriseOrchestrator)
            : base(httpContextAccessor)
        {
            _sessionService = sessionService;
            _authoriseOrchestrator = authoriseOrchestrator;
        }

        [HttpGet("need-more-information", Name = NeedMoreInformationRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> NeedMoreInformation()
        {
            await _authoriseOrchestrator.PrepareNeedMoreInformationAsync();

            return View();
        }
    }
}
