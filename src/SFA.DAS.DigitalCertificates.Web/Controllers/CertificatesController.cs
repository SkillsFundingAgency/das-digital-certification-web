using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route(BaseRoute)]
    [Authorize(Policy = nameof(PolicyNames.IsVerified))]
    public class CertificatesController : BaseController
    {
        private readonly ICertificatesOrchestrator _certificatesOrchestrator;
        public CertificatesController(IHttpContextAccessor contextAccessor, ICertificatesOrchestrator certificatesOrchestrator)
            : base(contextAccessor) 
        {
            _certificatesOrchestrator = certificatesOrchestrator;
        }

        #region Routes
        public const string BaseRoute = "certificates";
        public const string CertificatesListRouteGet = nameof(CertificatesListRouteGet);
        public const string CertificateStandardRouteGet = nameof(CertificateStandardRouteGet);
        public const string CertificateFrameworkRouteGet = nameof(CertificateFrameworkRouteGet);
        #endregion

        [HttpGet("list", Name = CertificatesListRouteGet)]
        public async Task<IActionResult> CertificatesList()
        {
            var viewModel = await _certificatesOrchestrator.GetCertificatesListViewModel();
            return View(viewModel);
        }

        [HttpGet("{certificateId}/standard", Name = CertificateStandardRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public IActionResult CertificateStandard(Guid certificateId)
        {
            return View();
        }

        [HttpGet("{certificateId}/framework", Name = CertificateFrameworkRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public IActionResult CertificateFramework(Guid certificateId)
        {
            return View();
        }
    }
}
