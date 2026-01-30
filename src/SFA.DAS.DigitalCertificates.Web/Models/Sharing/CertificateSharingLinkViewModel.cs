using System;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class CertificateSharingLinkViewModel
    {
        public Guid CertificateId { get; set; }
        public required string CourseName { get; set; }
        public CertificateType CertificateType { get; set; }

        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryTime { get; set; }
        public Guid LinkCode { get; set; }

        public string? FormattedExpiry { get; set; }
        public string? FormattedCreated { get; set; }
        public List<string> FormattedAccessTimes { get; set; } = new List<string>();

        public string? SecureLink { get; set; }
    }
}