using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ICertificateSharingOrchestrator
    {
        Task<CertificateSharingViewModel> GetCertificateSharings(Guid certificateId);
        Task<Guid> CreateCertificateSharing(Guid certificateId, string courseName);
    }
}
