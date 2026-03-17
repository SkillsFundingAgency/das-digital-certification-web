using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class CreateUserActionForCertificateResult
    {
        public string? ReferenceNumber { get; set; }
        public CertificateType CertificateType { get; set; }
    }
}