using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SFA.DAS.DigitalCertificates.Web.Authorization;

namespace SFA.DAS.DigitalCertificates.Web.Services.User
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetUserId()
        {
            return Guid.TryParse(GetUserClaimAsString(DigitalCertificateClaimsTypes.UserId), out Guid userId) ? userId : null;
        }

        public string GetGovUkIdentifier()
        {
            return GetUserClaimAsString(ClaimTypes.NameIdentifier);
        }

        private string GetUserClaimAsString(string claim)
        {
            if (IsUserAuthenticated() && TryGetUserClaimValue(claim, out var value))
            {
                return value;
            }
            return null;
        }

        private bool IsUserAuthenticated()
        {
            return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }

        private bool TryGetUserClaimValue(string key, out string value)
        {
            var claimsIdentity = (ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity;
            var claim = claimsIdentity.FindFirst(key);
            var exists = claim != null;
            value = exists ? claim.Value : null;

            return exists;
        }
    }
}
