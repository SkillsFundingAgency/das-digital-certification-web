using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.DigitalCertificates.Web.Models.Stub
{
    [ExcludeFromCodeCoverage]
    public class SignInStubViewModel
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public IFormFile? UserFile { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
