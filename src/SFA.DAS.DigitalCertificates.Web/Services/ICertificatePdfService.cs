using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ICertificatePdfService
    {
        Task<CertificatePdfFile> CreateStandardPdf(CertificateStandardViewModel model);
        Task<CertificatePdfFile> CreateFrameworkPdf(CertificateFrameworkViewModel model);
    }
}
