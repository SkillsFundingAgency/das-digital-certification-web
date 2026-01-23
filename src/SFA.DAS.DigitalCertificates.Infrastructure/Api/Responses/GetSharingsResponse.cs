using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetSharingsResponse
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public required string CourseName { get; set; }
        public List<SharingItem> Sharings { get; set; } = new List<SharingItem>();
    }

    public class SharingItem
    {
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LinkCode { get; set; }
        public DateTime ExpiryTime { get; set; }
        public List<DateTime> SharingAccess { get; set; } = new List<DateTime>();
        public List<SharingEmailItem> SharingEmails { get; set; } = new List<SharingEmailItem>();
    }

    public class SharingEmailItem
    {
        public Guid SharingEmailId { get; set; }
        public required string EmailAddress { get; set; }
        public Guid EmailLinkCode { get; set; }
        public DateTime SentTime { get; set; }
        public List<DateTime> SharingEmailAccess { get; set; } = new List<DateTime>();
    }
}
