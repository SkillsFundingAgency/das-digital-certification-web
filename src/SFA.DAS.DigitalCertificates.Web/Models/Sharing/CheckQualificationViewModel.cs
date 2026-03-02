using System;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class CheckQualificationViewModel
    {
        public required Guid Code { get; set; }
        public string? FormattedExpiry { get; set; }
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
    }
}
