using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.DigitalCertificates.Web.Models.Stub
{
    [ExcludeFromCodeCoverage]
    public class SignedInStubViewModel
    {
        public const string HashedAccountIdPlaceholder = "{{hashedAccountId}}";
        private readonly ClaimsPrincipal? _claimsPrinciple;

        public SignedInStubViewModel(IHttpContextAccessor httpContextAccessor, string returnUrl)
        {
            _claimsPrinciple = httpContextAccessor?.HttpContext?.User;
            ReturnUrl = returnUrl;
        }

        public string? StubEmail => _claimsPrinciple?.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
        public string? StubId => _claimsPrinciple?.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

        public string ReturnUrl { get; }

    }
}
