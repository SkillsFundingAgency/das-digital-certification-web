using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ICertificatesOrchestrator
    {
        Task<CertificatesListViewModel> GetCertificatesListViewModel();
        Task<CertificateStandardViewModel?> GetCertificateStandardViewModel(Guid certificateId);
        Task<CertificateFrameworkViewModel?> GetCertificateFrameworkViewModel(Guid certificateId);
        Task<string?> CreateOrReuseUserActionForCertificate(Guid certificateId);
        Task<string?> CreateOrReuseUserActionForNonSpecific();
        Task<ContactUsViewModel?> GetContactUsViewModel(string referenceNumber, Guid? certificateId);
        Task<bool> ValidateSelectAddressViewModel(SelectAddressViewModel viewModel, ModelStateDictionary modelState);
        Task<bool> ValidateAddAddressManualViewModel(AddAddressManualViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectAddressViewModel?> GetSelectAddressViewModel(Guid certificateId, string? searchTerm = null);
        Task<AddAddressManualViewModel?> GetAddAddressViewModel(Guid certificateId);
        Task<bool> StoreDeliveryAddressFromLocationAsync(Guid certificateId, string selectedName, string backRoute);
        Task<CheckAndSubmitViewModel?> GetCheckAndSubmitViewModel(Guid certificateId);
        Task CreatePrintRequest(Guid certificateId);
        Task<PrintRequestConfirmationViewModel> GetPrintRequestConfirmationViewModel(Guid certificateId);
    }
}