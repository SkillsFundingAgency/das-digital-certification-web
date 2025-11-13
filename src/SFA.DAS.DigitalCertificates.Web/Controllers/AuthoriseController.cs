using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("authorise")]
    public class AuthoriseController : BaseController
    {
        #region Routes
        public const string AuthoriseStartRouteGet = nameof(AuthoriseStartRouteGet);
        #endregion

        public AuthoriseController(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        [HttpGet("start", Name = AuthoriseStartRouteGet)]
        [Authorize(Policy = nameof(PolicyNames.IsVerified))]
        public IActionResult Start(Guid certificateId)
        {
            return View();
        }
    }
}
