using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class SharedCertificateViewModel
    {
        public required CertificateType CertificateType { get; set; }
        public CertificateStandardViewModel? Standard { get; set; }
        public CertificateFrameworkViewModel? Framework { get; set; }
    }
}
