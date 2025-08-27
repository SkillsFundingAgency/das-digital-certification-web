using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface IUserCacheService
    {
        Task<User> CacheUserForGovUkIdentifier(string govUkIdentifier);
    }
}