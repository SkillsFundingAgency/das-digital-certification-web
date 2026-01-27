using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ISessionService
    {
        Task SetUsernameAsync(string username);
        Task<string?> GetUserNameAsync();
        Task ClearUsernameAsync();

        Task SetShareEmailAsync(string email);
        Task<string?> GetShareEmailAsync();
        Task ClearShareEmailAsync();

        Task ClearSessionDataAsync();
    }
}