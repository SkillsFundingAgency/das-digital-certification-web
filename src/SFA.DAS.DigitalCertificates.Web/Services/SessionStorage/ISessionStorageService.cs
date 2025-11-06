using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;

namespace SFA.DAS.DigitalCertificates.Web.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        Task<List<Certificate>> GetOwnedCertificatesAsync();
        Task<UlnAuthorisation> GetUlnAuthorisationAsync();
        Task<UserResponse> GetUserAsync();
    }
}