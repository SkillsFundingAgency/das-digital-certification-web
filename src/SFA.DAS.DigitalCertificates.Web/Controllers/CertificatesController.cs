using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("certificates")]
    [Authorize(Policy = nameof(PolicyNames.IsVerified))]
    public class CertificatesController : BaseController
    {
        public CertificatesController(IHttpContextAccessor contextAccessor)
            : base(contextAccessor) { }

        #region Routes
        public const string CertificatesListRouteGet = nameof(CertificatesListRouteGet);
        public const string CertificateRouteGet = nameof(CertificateRouteGet);
        #endregion

        [Route("list", Name = CertificatesListRouteGet)]
        public IActionResult CertificatesList()
        {
            return View();
        }

        [Route("certificate", Name = CertificateRouteGet)]
        public IActionResult Certificate()
        {
            return View();
        }
    }
}
