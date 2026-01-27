using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage
{
    public class SessionStorageService : ISessionStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string UsernameKey = "DigitalCertificates:Username";
        private const string ShareEmailKey = "DigitalCertificates:ShareEmail";

        public SessionStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task SetUsernameAsync(string username)
        {
            SetString(UsernameKey, username);
            return Task.CompletedTask;
        }

        public Task<string?> GetUsernameAsync() =>
            Task.FromResult(GetString(UsernameKey));

        public Task ClearUsernameAsync()
        {
            Remove(UsernameKey);
            return Task.CompletedTask;
        }

        public Task SetShareEmailAsync(string email)
        {
            SetString(ShareEmailKey, email);
            return Task.CompletedTask;
        }

        public Task<string?> GetShareEmailAsync() =>
            Task.FromResult(GetString(ShareEmailKey));

        public Task ClearShareEmailAsync()
        {
            Remove(ShareEmailKey);
            return Task.CompletedTask;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        private string? GetString(string key) => Session?.GetString(key);

        private void SetString(string key, string value) => Session?.SetString(key, value);

        private void Remove(string key) => Session?.Remove(key);
    }
}
