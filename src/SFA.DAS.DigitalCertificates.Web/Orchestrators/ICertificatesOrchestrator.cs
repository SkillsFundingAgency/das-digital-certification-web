using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;

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
    }
}