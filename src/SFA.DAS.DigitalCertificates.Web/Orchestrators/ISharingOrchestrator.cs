using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ISharingOrchestrator
    {
        Task<CertificateSharingViewModel> GetSharings(Guid certificateId);
        Task<Guid> CreateSharing(Guid certificateId);
    }
}
