using RestEase;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Domain.Interfaces
{
    public interface IDigitalCertificatesOuterApi
    {
        [Get("/users/{govUkIdentifier}")]
        Task<User> GetUser([Path] string govUkIdentifier);

        [Post("/users/identity")]
        Task<Guid> CreateOrUpdateUser([Body] CreateOrUpdateUserRequest request);

        [Get("/ping")]
        Task Ping();
    }
}
