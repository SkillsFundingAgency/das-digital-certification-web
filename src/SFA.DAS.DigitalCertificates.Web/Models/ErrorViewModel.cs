using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ErrorMessage { get; set; }
    }
}
