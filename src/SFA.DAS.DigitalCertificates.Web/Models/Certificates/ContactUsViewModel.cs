using System;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class ContactUsViewModel
    {
        public required string ReferenceNumber { get; set; }
        public Guid? CertificateId { get; set; }
        public CertificateType CertificateType { get; set; } = CertificateType.Unknown;
    }
}