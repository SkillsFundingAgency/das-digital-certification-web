using System.Threading.Tasks;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;
using System;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ISessionService
    {
        Task SetUserDetailsAsync(UserDetails userDetails);
        Task<UserDetails?> GetUserDetailsAsync();
        Task SetShareEmailAsync(string email);
        Task<string?> GetShareEmailAsync();
        Task ClearShareEmailAsync();
        Task SetDeliveryAddressAsync(CheckAndSubmitViewModel address);
        Task<CheckAndSubmitViewModel?> GetDeliveryAddressAsync();
        Task ClearDeliveryAddressAsync();
        Task<List<Certificate>?> GetOwnedCertificatesAsync();
        Task<UlnAuthorisation?> GetUlnAuthorisationAsync();
        Task ClearSessionDataAsync();
        Task SetContactReferenceAsync(string referenceNumber);
        Task<string?> GetContactReferenceAsync();
        Task ClearContactReferenceAsync();
        Task AddRecordedSharingAccessCodeAsync(Guid code);
        Task<bool> IsSharingAccessCodeRecordedAsync(Guid code);
    }
}