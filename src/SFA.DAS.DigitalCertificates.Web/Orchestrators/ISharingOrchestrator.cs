using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ISharingOrchestrator
    {
        Task<CreateCertificateSharingViewModel> GetSharings(Guid certificateId);
        Task<CertificateSharingLinkViewModel> GetSharingById(Guid certificateId, Guid sharingId);
        Task<EmailSentViewModel?> GetEmailSent(Guid certificateId, Guid sharingId, Guid sharingEmailId);
        Task<Guid> CreateSharing(Guid certificateId);
        Task<ConfirmShareByEmailViewModel?> GetConfirmShareByEmail(Guid certificateId, Guid sharingId, string emailAddress);
        Task<Guid?> CreateSharingEmail(Guid certificateId, Guid sharingId, string emailAddress);
        Task DeleteSharing(Guid certificateId, Guid sharingId);
        Task<bool> ValidateShareByEmailViewModel(ShareByEmailViewModel viewModel, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState);
        Task<CheckQualificationViewModel?> GetCheckQualificationViewModelAndRecordAccess(Guid code);
    }
}
