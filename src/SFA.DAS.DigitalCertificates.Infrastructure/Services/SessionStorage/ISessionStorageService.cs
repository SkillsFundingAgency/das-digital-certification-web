using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        Task SetUsernameAsync(string username);
        Task<string?> GetUsernameAsync();
        Task ClearUsernameAsync();

        Task SetShareEmailAsync(string email);
        Task<string?> GetShareEmailAsync();
        Task ClearShareEmailAsync();
    }
}
