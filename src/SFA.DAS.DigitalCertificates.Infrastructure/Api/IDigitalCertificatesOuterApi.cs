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
    }
}
