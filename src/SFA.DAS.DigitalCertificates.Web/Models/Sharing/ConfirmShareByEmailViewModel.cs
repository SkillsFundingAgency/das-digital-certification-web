using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class ConfirmShareByEmailViewModel
    {
        public Guid CertificateId { get; set; }
        public Guid SharingId { get; set; }
        public string? CourseName { get; set; } = string.Empty;
        public int SharingNumber { get; set; }
        public string? EmailAddress { get; set; } = string.Empty;
        public string? FormattedExpiry { get; set; }
    }
}
