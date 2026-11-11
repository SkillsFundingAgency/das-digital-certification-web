using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage
{
    public class SessionStorageService : ISessionStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task SetAsync(string key, string value)
        {
            SetString(key, value);
            return Task.CompletedTask;
        }

        public Task<string?> GetAsync(string key)
        {
            return Task.FromResult(GetString(key));
        }

        public Task ClearAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public Task ClearByPrefixAsync(string prefix)
        {
            var session = Session;

            if (session is null)
                return Task.CompletedTask;

            var keys = session.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.Ordinal))
                .ToArray();

            foreach (var key in keys)
            {
                session.Remove(key);
            }

            return Task.CompletedTask;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        private string? GetString(string key) => Session?.GetString(key);

        private void SetString(string key, string value) => Session?.SetString(key, value);

        private void Remove(string key) => Session?.Remove(key);
    }
}
