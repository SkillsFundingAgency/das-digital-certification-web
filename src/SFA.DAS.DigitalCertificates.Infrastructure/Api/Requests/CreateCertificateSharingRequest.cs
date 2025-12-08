using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreateCertificateSharingRequest
    {
        public Guid Userid { get; set; }
        public Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public required string CourseName { get; set; }
    }
}
