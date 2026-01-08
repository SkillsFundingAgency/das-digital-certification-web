using System;
using System.Threading.Tasks;
using RestEase;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Domain.Interfaces
{
    public interface IDigitalCertificatesOuterApi
    {
        [Get("/users/{govUkIdentifier}")]
        Task<UserResponse> GetUser([Path] string govUkIdentifier);

        [Post("/users/identity")]
        Task<Guid> CreateOrUpdateUser([Body] CreateOrUpdateUserRequest request);

        [Get("/ping")]
        Task Ping();

        [Get("/users/{userId}/certificates")]
        Task<CertificatesResponse> GetCertificates([Path] Guid userId);

        [Get("/certificates/{certificateId}")]
        Task<GetCertificateByIdResponse> GetCertificateById([Path] Guid certificateId);

        [Get("users/{userId}/sharings")]
        Task<GetSharingsResponse> GetSharings([Path] string userId, [Query("certificateId")] Guid certificateId, [Query("limit")] int? limit);

        [Post("/sharing")]
        Task<CreateSharingResponse> CreateSharing([Body] CreateSharingRequest request);

        [Get("/sharing/{sharingId}")]
        Task<GetSharingByIdResponse> GetSharingById([Path] Guid sharingId, [Query("limit")] int? limit);
    }
}
