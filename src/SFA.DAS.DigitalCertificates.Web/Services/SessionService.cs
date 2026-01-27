using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionStorageService _sessionStorageService;

        public SessionService(ISessionStorageService sessionStorageService)
        {
            _sessionStorageService = sessionStorageService;
        }

        public Task SetUsernameAsync(string username)
        {
            return _sessionStorageService.SetUsernameAsync(username);
        }

        public Task<string?> GetUserNameAsync()
        {
            return _sessionStorageService.GetUsernameAsync();
        }

        public Task ClearUsernameAsync()
        {
            return _sessionStorageService.ClearUsernameAsync();
        }

        public Task SetShareEmailAsync(string email)
        {
            return _sessionStorageService.SetShareEmailAsync(email);
        }

        public Task<string?> GetShareEmailAsync()
        {
            return _sessionStorageService.GetShareEmailAsync();
        }

        public Task ClearShareEmailAsync()
        {
            return _sessionStorageService.ClearShareEmailAsync();
        }

        public async Task ClearSessionDataAsync()
        {
            await _sessionStorageService.ClearShareEmailAsync();
            await _sessionStorageService.ClearUsernameAsync();
        }
    }
}
