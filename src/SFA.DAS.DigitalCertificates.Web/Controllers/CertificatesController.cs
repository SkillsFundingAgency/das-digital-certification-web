using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public const string DownloadCertificateStandardPdfRouteGet = nameof(DownloadCertificateStandardPdfRouteGet);
        public const string DownloadCertificateFrameworkPdfRouteGet = nameof(DownloadCertificateFrameworkPdfRouteGet);
        public const string CreateCertificateSharingRouteGet = nameof(CreateCertificateSharingRouteGet);
        public const string CreateCertificateSharingRoutePost = nameof(CreateCertificateSharingRoutePost);
        public const string CertificateSharingLinkRouteGet = nameof(CertificateSharingLinkRouteGet);
        public const string ShareByEmailRoutePost = nameof(ShareByEmailRoutePost);
        public const string ConfirmShareByEmailRouteGet = nameof(ConfirmShareByEmailRouteGet);
        public const string ConfirmShareByEmailRoutePost = nameof(ConfirmShareByEmailRoutePost);
        public const string EmailSentRouteGet = nameof(EmailSentRouteGet);
        public const string DeleteSharingRouteGet = nameof(DeleteSharingRouteGet);
        public const string DeleteSharingRoutePost = nameof(DeleteSharingRoutePost);
        public const string CheckQualificationRouteGet = nameof(CheckQualificationRouteGet);
        public const string CheckQualificationRoutePost = nameof(CheckQualificationRoutePost);
        public const string CheckQualificationExpiredRouteGet = nameof(CheckQualificationExpiredRouteGet);
        public const string SharedCertificateStandardRouteGet = nameof(SharedCertificateStandardRouteGet);
        public const string SharedCertificateFrameworkRouteGet = nameof(SharedCertificateFrameworkRouteGet);
        public const string ContactUsRouteGet = nameof(ContactUsRouteGet);
        public const string ContactUsForCertificateRouteGet = nameof(ContactUsForCertificateRouteGet);
        public const string ContactUsCreateRoutePost = nameof(ContactUsCreateRoutePost);
        public const string ContactUsForCertificateCreateRoutePost = nameof(ContactUsForCertificateCreateRoutePost);
        public const string SelectAddressRouteGet = nameof(SelectAddressRouteGet);
        public const string AddAddressRouteGet = nameof(AddAddressRouteGet);
        public const string CheckAndSubmitRouteGet = nameof(CheckAndSubmitRouteGet);
        public const string CheckAndSubmitRoutePost = nameof(CheckAndSubmitRoutePost);
        public const string SelectAddressRoutePost = nameof(SelectAddressRoutePost);
        public const string AddAddressRoutePost = nameof(AddAddressRoutePost);
        public const string PrintRequestConfirmationRouteGet = nameof(PrintRequestConfirmationRouteGet);
        public const string DownloadSharedCertificateFrameworkPdfRouteGet = nameof(DownloadSharedCertificateFrameworkPdfRouteGet);
        public const string DownloadSharedCertificateStandardPdfRouteGet = nameof(DownloadSharedCertificateStandardPdfRouteGet);
        #endregion

        public const string PdfCertificateCannotBeProduced = "PDF certificate cannot be produced";

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

        [HttpGet("{certificateId}/delivery-request/add-address", Name = AddAddressRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> AddAddress(Guid certificateId, string? backRoute = null)
        {
            var model = await _certificatesOrchestrator.GetAddAddressViewModel(certificateId);

            if (model == null)
            {
                return RedirectToRoute(CertificateStandardRouteGet, new { certificateId });
            }

            if (!string.IsNullOrWhiteSpace(backRoute))
            {
                model.BackRoute = backRoute;
            }

            return View(model);
        }

        [HttpGet("{certificateId}/delivery-request/select-address", Name = SelectAddressRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> SelectAddress(Guid certificateId)
        {
            var model = await _certificatesOrchestrator.GetSelectAddressViewModel(certificateId);

            if (model == null)
            {
                return RedirectToRoute(CertificateStandardRouteGet, new { certificateId });
            }

            await _sessionService.ClearDeliveryAddressAsync();

            return View(model);
        }

        [HttpPost("{certificateId}/delivery-request/select-address", Name = SelectAddressRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> SelectAddressPost(Guid certificateId, SelectAddressViewModel model)
        {
            model.CertificateId = certificateId;

            if (!await _certificatesOrchestrator.ValidateSelectAddressViewModel(model, ModelState))
            {
                return RedirectToRoute(SelectAddressRouteGet, new { certificateId });
            }

            var storedAddress = await _certificatesOrchestrator.StoreDeliveryAddressFromLocationAsync(certificateId, model.SelectedAddress ?? string.Empty, SelectAddressRouteGet);

            if (!storedAddress)
            {
                return RedirectToRoute(SelectAddressRouteGet, new { certificateId });
            }

            return RedirectToRoute(CheckAndSubmitRouteGet, new { certificateId });

        }

        [HttpPost("{certificateId}/delivery-request/add-address", Name = AddAddressRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> AddAddressPost(Guid certificateId, AddAddressManualViewModel model)
        {
            model.CertificateId = certificateId;

            if (!await _certificatesOrchestrator.ValidateAddAddressManualViewModel(model, ModelState))
            {
                return RedirectToRoute(AddAddressRouteGet, new { certificateId, backRoute = model.BackRoute });
            }

            var addr = new CheckAndSubmitViewModel
            {
                CertificateId = certificateId,
                Organisation = model.Organisation,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                TownOrCity = model.TownOrCity,
                County = model.County,
                Postcode = model.Postcode,
                BackRoute = AddAddressRouteGet
            };

            await _sessionService.SetDeliveryAddressAsync(addr);

            return RedirectToRoute(CheckAndSubmitRouteGet, new { certificateId });
        }

        [HttpGet("{certificateId}/delivery-request/check-and-submit", Name = CheckAndSubmitRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CheckAndSubmit(Guid certificateId)
        {
            var vm = await _certificatesOrchestrator.GetCheckAndSubmitViewModel(certificateId, CertificateStandardRouteGet);
            if (vm == null)
            {
                return RedirectToRoute(CertificateStandardRouteGet, new { certificateId });
            }

            return View(vm);
        }

        [HttpPost("{certificateId}/delivery-request/check-and-submit", Name = CheckAndSubmitRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CheckAndSubmitPost(Guid certificateId)
        {
            await _certificatesOrchestrator.CreatePrintRequest(certificateId);

            return RedirectToRoute(PrintRequestConfirmationRouteGet, new { certificateId });
        }

        [HttpGet("{certificateId}/delivery-request/confirmation", Name = PrintRequestConfirmationRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> PrintRequestConfirmation(Guid certificateId)
        {
            var model = await _certificatesOrchestrator.GetPrintRequestConfirmationViewModel(certificateId);

            return View(model);
        }

        [HttpGet("list", Name = CertificatesListRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsUlnAuthorised))]
        public async Task<IActionResult> CertificatesList()
        {
            await _sessionService.ClearContactReferenceAsync();

            var viewModel = await _certificatesOrchestrator.GetCertificatesListViewModel();

            var certificates = viewModel?.Certificates;
            
            if (certificates == null || certificates.Count == 0)
            {                
                return View(viewModel);
            }
                
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
            await _sessionService.ClearContactReferenceAsync();

            var model = await _certificatesOrchestrator.GetCertificateStandardViewModel(certificateId);

            return View(model);
        }

        [HttpGet("{certificateId}/framework", Name = CertificateFrameworkRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> CertificateFramework(Guid certificateId)
        {            
            await _sessionService.ClearContactReferenceAsync();

            var model = await _certificatesOrchestrator.GetCertificateFrameworkViewModel(certificateId);
            
            return View(model);
        }

        [HttpGet("{certificateId}/framework/download", Name = DownloadCertificateFrameworkPdfRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> DownloadCertificateFrameworkPdf(Guid certificateId)
        {
            var model = await _certificatesOrchestrator.GetDownloadFrameworkCertificateViewModelAsync(certificateId);

            if(model == null)
            {
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
            }
            
            var pdfBytes = await _certificatesOrchestrator.GenerateCertificateAsync(model);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
            }

            return File(pdfBytes, "application/pdf", $"CertificateNumber{model.CertificateNumber}.pdf");
        }

        [HttpGet("{certificateId}/standard/download", Name = DownloadCertificateStandardPdfRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> DownloadCertificateStandardPdf(Guid certificateId)
        {
            var model = await _certificatesOrchestrator.GetDownloadCertificateViewModelAsync(certificateId);

            if (model == null)
            {
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
            }
            
            var pdfBytes = await _certificatesOrchestrator.GenerateCertificateAsync(model);
            
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
               throw new InvalidOperationException(PdfCertificateCannotBeProduced);
            }

            return File(pdfBytes, "application/pdf", $"CertificateNumber{model.CertificateNumber}.pdf");
        }
                

        [HttpGet("contact", Name = ContactUsRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsUlnAuthorised))]
        public async Task<IActionResult> ContactUs()
        {
            var referenceNumber = await _sessionService.GetContactReferenceAsync();
            if (string.IsNullOrEmpty(referenceNumber))
            {
                return RedirectToRoute(CertificatesListRouteGet);
            }

            var model = await _certificatesOrchestrator.GetContactUsViewModel(referenceNumber, null);
            if (model == null)
            {
                return RedirectToRoute(CertificatesListRouteGet);
            }

            return View(model);
        }

        [HttpGet("{certificateId}/contact", Name = ContactUsForCertificateRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> ContactUsForCertificate(Guid certificateId)
        {
            var referenceNumber = await _sessionService.GetContactReferenceAsync();
            if (string.IsNullOrEmpty(referenceNumber))
            {
                return RedirectToRoute(CertificateStandardRouteGet, new { certificateId });
            }

            var model = await _certificatesOrchestrator.GetContactUsViewModel(referenceNumber, certificateId);
            if (model == null)
            {
                return RedirectToRoute(CertificateStandardRouteGet, new { certificateId });
            }

            return View("ContactUs", model);
        }

        [HttpPost("contact/create", Name = ContactUsCreateRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsUlnAuthorised))]
        public async Task<IActionResult> ContactUsCreate()
        {
            var referenceNumber = await _certificatesOrchestrator.CreateUserActionForNonSpecific();
            if (string.IsNullOrEmpty(referenceNumber))
                return RedirectToRoute(CertificatesListRouteGet);

            return RedirectToRoute(ContactUsRouteGet);
        }

        [HttpPost("{certificateId}/contact/create", Name = ContactUsForCertificateCreateRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> ContactUsForCertificateCreate(Guid certificateId, ActionType actionType = ActionType.Help)
        {
            var result = await _certificatesOrchestrator.CreateUserActionForCertificate(certificateId, actionType);

            if (string.IsNullOrEmpty(result.ReferenceNumber))
            {
                return result.CertificateType switch
                {
                    CertificateType.Standard  => RedirectToRoute(CertificateStandardRouteGet,  new { certificateId }),
                    CertificateType.Framework => RedirectToRoute(CertificateFrameworkRouteGet, new { certificateId }),
                    _                        => RedirectToRoute(CertificatesListRouteGet)
                };
            }

            return RedirectToRoute(ContactUsForCertificateRouteGet, new { certificateId });
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

        [HttpGet("{certificateId}/sharing/{sharingId}/delete", Name = DeleteSharingRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> DeleteSharing(Guid certificateId, Guid sharingId)
        {
            var model = await _sharingOrchestrator.GetSharingById(certificateId, sharingId);

            if (model == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            return View(model);
        }

        [HttpPost("{certificateId}/sharing/{sharingId}/delete", Name = DeleteSharingRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.IsCertificateOwner))]
        public async Task<IActionResult> DeleteSharingPost(Guid certificateId, Guid sharingId)
        {
            var model = await _sharingOrchestrator.GetSharingById(certificateId, sharingId);

            if (model == null)
            {
                return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
            }

            await _sharingOrchestrator.DeleteSharing(certificateId, sharingId);

            TempData.AddFlashMessage($"Sharing link {model.SharingNumber} deleted", string.Empty, TempDataDictionaryExtensions.FlashMessageLevel.Success);

            return RedirectToRoute(CreateCertificateSharingRouteGet, new { certificateId });
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
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
            }

            var pdfBytes = await _certificatesOrchestrator.GenerateCertificateAsync(model);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
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
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
            }

            var pdfBytes = await _certificatesOrchestrator.GenerateCertificateAsync(model);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new InvalidOperationException(PdfCertificateCannotBeProduced);
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
