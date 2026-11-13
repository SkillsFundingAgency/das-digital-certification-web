using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
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
        #endregion

        public const string PdfCertificateCannotBeProduced = "PDF certificate cannot be produced";

        private readonly ICertificatesOrchestrator _certificatesOrchestrator;
        private readonly ISessionService _sessionService;

        public CertificatesController(IHttpContextAccessor contextAccessor, ICertificatesOrchestrator certificatesOrchestrator, ISessionService sessionService)
            : base(contextAccessor)
        {
            _certificatesOrchestrator = certificatesOrchestrator;
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
    }
}
