using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ISharingOrchestrator
    {
        Task<CreateCertificateSharingViewModel> GetSharings(Guid certificateId);
        Task<CertificateSharingLinkViewModel> GetSharingById(Guid certificateId, Guid sharingId);
        Task<Guid> CreateSharing(Guid certificateId);
    }
}
