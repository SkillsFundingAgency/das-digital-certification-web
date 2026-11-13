using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route(BaseRoute)]
    public class SharedCertificatesController : BaseController
    {
        #region Routes
        public const string BaseRoute = "certificates";
        public const string CheckQualificationRouteGet = nameof(CheckQualificationRouteGet);
        public const string CheckQualificationRoutePost = nameof(CheckQualificationRoutePost);
        public const string CheckQualificationExpiredRouteGet = nameof(CheckQualificationExpiredRouteGet);
        public const string SharedCertificateStandardRouteGet = nameof(SharedCertificateStandardRouteGet);
        public const string SharedCertificateFrameworkRouteGet = nameof(SharedCertificateFrameworkRouteGet);
        public const string DownloadSharedCertificateFrameworkPdfRouteGet = nameof(DownloadSharedCertificateFrameworkPdfRouteGet);
        public const string DownloadSharedCertificateStandardPdfRouteGet = nameof(DownloadSharedCertificateStandardPdfRouteGet);
        #endregion

        private readonly ICertificatesOrchestrator _certificatesOrchestrator;
        private readonly ISharingOrchestrator _sharingOrchestrator;

        public SharedCertificatesController(IHttpContextAccessor contextAccessor, ICertificatesOrchestrator certificatesOrchestrator, ISharingOrchestrator sharingOrchestrator)
            : base(contextAccessor)
        {
            _certificatesOrchestrator = certificatesOrchestrator;
            _sharingOrchestrator = sharingOrchestrator;
        }

        [HttpGet("sharing/{sharingLinkCode}/check-code", Name = CheckQualificationRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> CheckQualification(Guid sharingLinkCode)
        {
            var sharingInfo = await _sharingOrchestrator.GetCheckQualificationViewModel(sharingLinkCode);

            if (sharingInfo == null)
            {
                return RedirectToRoute(CheckQualificationExpiredRouteGet);
            }

            return View(sharingInfo);
        }

        [HttpPost("sharing/{sharingLinkCode}/check-code", Name = CheckQualificationRoutePost)]
        [AllowAnonymous]
        public async Task<IActionResult> CheckQualificationPost(Guid sharingLinkCode)
        {
            var sharingInfo = await _sharingOrchestrator.GetCheckQualificationViewModelAndRecordAccess(sharingLinkCode);

            if (sharingInfo == null)
            {
                return RedirectToRoute(CheckQualificationExpiredRouteGet);
            }

            if (sharingInfo.CertificateType == CertificateType.Standard)
            {
                return RedirectToRoute(SharedCertificateStandardRouteGet, new { sharingLinkCode });
            }

            if (sharingInfo.CertificateType == CertificateType.Framework)
            {
                return RedirectToRoute(SharedCertificateFrameworkRouteGet, new { sharingLinkCode });
            }

            return RedirectToRoute(CheckQualificationExpiredRouteGet);
        }

        [HttpGet("shared/{sharingLinkCode}/standard", Name = SharedCertificateStandardRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> SharedCertificateStandard(Guid sharingLinkCode)
        {
            var model = await _sharingOrchestrator.GetSharedStandardCertificateViewModel(sharingLinkCode);
            if (model == null)
            {
                return RedirectToRoute(CheckQualificationExpiredRouteGet);
            }

            return View(model);
        }

        [HttpGet("shared/{sharingLinkCode}/framework", Name = SharedCertificateFrameworkRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> SharedCertificateFramework(Guid sharingLinkCode)
        {
            var model = await _sharingOrchestrator.GetSharedFrameworkCertificateViewModel(sharingLinkCode);
            if (model == null)
            {
                return RedirectToRoute(CheckQualificationExpiredRouteGet);
            }

            return View(model);
        }

        [HttpGet("shared/{sharingLinkCode}/framework/download", Name = DownloadSharedCertificateFrameworkPdfRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadSharedCertificateFrameworkPdf(Guid sharingLinkCode)
        {
            var model = await _sharingOrchestrator.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode);

            if (model == null)
            {
                throw new InvalidOperationException(CertificatesController.PdfCertificateCannotBeProduced);
            }

            var pdfBytes = await _certificatesOrchestrator.GenerateCertificateAsync(model);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new InvalidOperationException(CertificatesController.PdfCertificateCannotBeProduced);
            }

            return File(pdfBytes, "application/pdf", model.SanitisedAnonymousCertificateName);
        }

        [HttpGet("shared/{sharingLinkCode}/standard/download", Name = DownloadSharedCertificateStandardPdfRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadSharedCertificateStandardPdf(Guid sharingLinkCode)
        {
            var model = await _sharingOrchestrator.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode);          

            if (model == null)
            {
                throw new InvalidOperationException(CertificatesController.PdfCertificateCannotBeProduced);
            }

            var pdfBytes = await _certificatesOrchestrator.GenerateCertificateAsync(model);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new InvalidOperationException(CertificatesController.PdfCertificateCannotBeProduced);
            }

            return File(pdfBytes, "application/pdf", model.SanitisedAnonymousCertificateName);
        }

        [HttpGet("/certificates/expired", Name = CheckQualificationExpiredRouteGet)]
        [AllowAnonymous]
        public IActionResult CheckQualificationExpired()
        {
            return View();
        }
    }
}
