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

        public string? SecureLink { get; set; }
        public string EmailAddress { get; set; } = string.Empty;

        public List<SharingAccessHistoryItem> AccessHistory { get; set; } = new List<SharingAccessHistoryItem>();
    }

    public class SharingAccessHistoryItem
    {
        public AccessType AccessType { get; set; }
        public DateTime AccessedAt { get; set; }
        public string? EmailAddress { get; set; }
        public string Activity { get; set; } = string.Empty;
        public string FormattedTime { get; set; } = string.Empty;
    }

    public enum AccessType
    {
        Created,
        DirectLink,
        EmailLink,
        EmailSent
    }
}