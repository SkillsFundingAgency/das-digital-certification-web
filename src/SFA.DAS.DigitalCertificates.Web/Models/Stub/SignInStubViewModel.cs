using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Models.Stub
{
    [ExcludeFromCodeCoverage]
    public class SignInStubViewModel
    {
        public string Email { get; set; }
        public string Id { get; set; }
        public IFormFile? UserFile { get; set; }
        public string ReturnUrl { get; set; }
    }
}
