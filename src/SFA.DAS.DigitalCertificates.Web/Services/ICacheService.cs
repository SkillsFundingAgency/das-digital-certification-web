using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ICacheService
    {
        Task<User?> GetUserAsync(string govUkIdentifier);
        Task Clear(string govUkIdentifier);
    }
}