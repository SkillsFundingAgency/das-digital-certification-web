using System.Threading.Tasks;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;
using System;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ISessionService
    {
        Task SetUsernameAsync(string username);
        Task<string?> GetUserNameAsync();
        Task SetShareEmailAsync(string email);
        Task<string?> GetShareEmailAsync();
        Task ClearShareEmailAsync();
        Task<List<Certificate>?> GetOwnedCertificatesAsync(string govUkIdentifier);
        Task<UlnAuthorisation?> GetUlnAuthorisationAsync(string govUkIdentifier);
        Task ClearSessionDataAsync(string govUkIdentifier);
        Task AddRecordedSharingAccessCodeAsync(Guid code);
        Task<bool> IsSharingAccessCodeRecordedAsync(Guid code);
    }
}