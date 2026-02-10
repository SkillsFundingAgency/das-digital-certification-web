using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetSharingCodeResponse
    {
        public Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public DateTime ExpiryTime { get; set; }
        public Guid? SharingId { get; set; }
        public Guid? SharingEmailId { get; set; }
    }
}
