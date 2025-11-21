using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ISessionStorageService
    {
        Task<User?> GetUserAsync(string govUkIdentifier);
        Task<List<Certificate>?> GetOwnedCertificatesAsync(string govUkIdentifier);
        Task<UlnAuthorisation?> GetUlnAuthorisationAsync(string govUkIdentifier);
    }
}