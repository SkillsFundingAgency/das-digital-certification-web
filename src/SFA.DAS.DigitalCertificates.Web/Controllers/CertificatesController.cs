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
        private readonly ISharingOrchestrator _sharingOrchestrator;

        public CertificatesController(IHttpContextAccessor contextAccessor, ICertificatesOrchestrator certificatesOrchestrator, ISharingOrchestrator sharingOrchestrator)
            : base(contextAccessor)
        {
            _certificatesOrchestrator = certificatesOrchestrator;
            _sharingOrchestrator = sharingOrchestrator;
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
            var model = await _sharingOrchestrator.GetSharings(certificateId);
            return View(model);
        }

        [HttpPost("{certificateId}/sharing", Name = CreateCertificateSharingRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CreateCertificateSharingPost(Guid certificateId)
        {
            var result = await _sharingOrchestrator.CreateSharing(certificateId);

            return RedirectToRoute(CertificateSharingLinkRouteGet, new { certificateId, sharingId = result });
        }

        [HttpGet("{certificateId}/sharing/{sharingId}", Name = CertificateSharingLinkRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CertificateSharingLink(Guid certificateId, Guid sharingId)
        {
            var model = await _sharingOrchestrator.GetSharingById(certificateId, sharingId);

            if (model == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            return View(model);
        }

        [HttpPost("{certificateId}/sharing/{sharingId}", Name = CertificateSharingLinkRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public IActionResult CertificateSharingLinkPost(Guid certificateId, Guid sharingId)
        {
            return RedirectToRoute(CertificateSharingLinkRouteGet, new { certificateId, sharingId });
        }
    }
}
