using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ICertificatesOrchestrator
    {
        Task<CertificatesListViewModel> GetCertificatesListViewModel();
    }
}