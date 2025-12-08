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
        #region Routes
        public const string BaseRoute = "certificates";
        public const string CertificatesListRouteGet = nameof(CertificatesListRouteGet);
        public const string CertificateStandardRouteGet = nameof(CertificateStandardRouteGet);
        public const string CertificateFrameworkRouteGet = nameof(CertificateFrameworkRouteGet);
        public const string CreateCertificateSharingRouteGet = nameof(CreateCertificateSharingRouteGet);
        public const string CreateCertificateSharingRoutePost = nameof(CreateCertificateSharingRoutePost);
        public const string CertificateSharingLinkRouteGet = nameof(CertificateSharingLinkRouteGet);
        public const string CertificateSharingLinkRoutePost = nameof(CertificateSharingLinkRoutePost);
        #endregion

        private readonly ICertificatesOrchestrator _certificatesOrchestrator;
        private readonly ICertificateSharingOrchestrator _certificateSharingOrchestrator;

        public CertificatesController(IHttpContextAccessor contextAccessor, ICertificatesOrchestrator certificatesOrchestrator, ICertificateSharingOrchestrator certificateSharingOrchestrator)
            : base(contextAccessor)
        {
            _certificatesOrchestrator = certificatesOrchestrator;
            _certificateSharingOrchestrator = certificateSharingOrchestrator;
        }

        [HttpGet("list", Name = CertificatesListRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsUlnAuthorised))]
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

        [HttpGet("{certificateId}/sharing", Name = CreateCertificateSharingRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CreateCertificateSharing(Guid certificateId)
        {
            var model = await _certificateSharingOrchestrator.GetCertificateSharings(certificateId);
            return View(model);
        }

        [HttpPost("{certificateId}/sharing", Name = CreateCertificateSharingRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CreateCertificateSharingPost(Guid certificateId)
        {
            var result = await _certificateSharingOrchestrator.CreateCertificateSharing(certificateId);

            return RedirectToRoute(CertificateSharingLinkRouteGet, new { certificateId });
        }

        [HttpGet("{certificateId}/sharingLink", Name = CertificateSharingLinkRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public IActionResult CertificateSharingLink(Guid certificateId)
        {
            return View();
        }

        [HttpPost("{certificateId}/sharingLink", Name = CertificateSharingLinkRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public IActionResult CertificateSharingLinkPost(Guid certificateId)
        {
            // Handle form submission for sharing link
            return View();
        }
    }
}
