using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using FluentValidation;
using SFA.DAS.GovUK.Auth.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Services;

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
        public const string ShareByEmailRoutePost = nameof(ShareByEmailRoutePost);
        public const string ConfirmShareByEmailRouteGet = nameof(ConfirmShareByEmailRouteGet);
        public const string ConfirmShareByEmailRoutePost = nameof(ConfirmShareByEmailRoutePost);
        public const string EmailSentRouteGet = nameof(EmailSentRouteGet);
        #endregion

        private readonly ICertificatesOrchestrator _certificatesOrchestrator;
        private readonly ISharingOrchestrator _sharingOrchestrator;
        private readonly ISessionService _sessionService;

        public CertificatesController(IHttpContextAccessor contextAccessor, ICertificatesOrchestrator certificatesOrchestrator, ISharingOrchestrator sharingOrchestrator, ISessionService sessionService)
            : base(contextAccessor)
        {
            _certificatesOrchestrator = certificatesOrchestrator;
            _sharingOrchestrator = sharingOrchestrator;
            _sessionService = sessionService;
        }

        [HttpGet("list", Name = CertificatesListRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsUlnAuthorised))]
        public async Task<IActionResult> CertificatesList()
        {
            var viewModel = await _certificatesOrchestrator.GetCertificatesListViewModel();

            var certificates = viewModel?.Certificates;
            if (certificates == null || certificates.Count == 0)
                return View(viewModel);

            var standards = certificates.Where(c => c.CertificateType == CertificateType.Standard).ToList();
            var frameworks = certificates.Where(c => c.CertificateType == CertificateType.Framework).ToList();

            if (standards.Count == 1 && frameworks.Count == 0)
                return RedirectToRoute(CertificateStandardRouteGet, new { certificateId = standards[0].CertificateId });

            if (frameworks.Count == 1 && standards.Count == 0)
                return RedirectToRoute(CertificateFrameworkRouteGet, new { certificateId = frameworks[0].CertificateId });

            return View(viewModel);
        }

        [HttpGet("{certificateId}/standard", Name = CertificateStandardRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CertificateStandard(Guid certificateId)
        {
            var model = await _certificatesOrchestrator.GetCertificateStandardViewModel(certificateId);

            return View(model);
        }

        [HttpGet("{certificateId}/framework", Name = CertificateFrameworkRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CertificateFramework(Guid certificateId)
        {
            var model = await _certificatesOrchestrator.GetCertificateFrameworkViewModel(certificateId);
            return View(model);
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

            var email = await _sessionService.GetShareEmailAsync();

            model.EmailAddress = email ?? string.Empty;

            return View(model);
        }

        [HttpPost("{certificateId}/sharing/{sharingId}/send-email", Name = ShareByEmailRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> ShareByEmail(Guid certificateId, Guid sharingId, ShareByEmailViewModel model)
        {
            var sharingByIdModel = await _sharingOrchestrator.GetSharingById(certificateId, sharingId);

            if (sharingByIdModel == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            if (!await _sharingOrchestrator.ValidateShareByEmailViewModel(model, ModelState))
            {
                await _sessionService.SetShareEmailAsync(model.EmailAddress ?? string.Empty);
                return RedirectToRoute(CertificateSharingLinkRouteGet, new { certificateId, sharingId, emailAddress = model.EmailAddress ?? string.Empty });
            }

            await _sessionService.SetShareEmailAsync(model.EmailAddress ?? string.Empty);

            return RedirectToRoute(ConfirmShareByEmailRouteGet, new { certificateId, sharingId });
        }

        [HttpGet("{certificateId}/sharing/{sharingId}/confirm-email", Name = ConfirmShareByEmailRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> ConfirmShareByEmail(Guid certificateId, Guid sharingId)
        {
            var email = await _sessionService.GetShareEmailAsync() ?? string.Empty;

            var model = await _sharingOrchestrator.GetConfirmShareByEmail(certificateId, sharingId, email);

            if (model == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            return View(model);
        }

        [HttpPost("{certificateId}/sharing/{sharingId}/confirm-email", Name = ConfirmShareByEmailRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> ConfirmShareByEmailPost(Guid certificateId, Guid sharingId, ConfirmShareByEmailViewModel model)
        {
            var result = await _sharingOrchestrator.CreateSharingEmail(certificateId, sharingId, model.EmailAddress ?? string.Empty);

            await _sessionService.ClearShareEmailAsync();

            if (result == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            return RedirectToRoute(EmailSentRouteGet, new { certificateId, sharingId, sharingEmailId = result.Value });
        }

        [HttpGet("{certificateId}/sharing/{sharingId}/email-sent/{sharingEmailId}", Name = EmailSentRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> EmailSent(Guid certificateId, Guid sharingId, Guid sharingEmailId)
        {
            var model = await _sharingOrchestrator.GetEmailSent(certificateId, sharingId, sharingEmailId);

            if (model == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            return View(model);
        }
    }
}
