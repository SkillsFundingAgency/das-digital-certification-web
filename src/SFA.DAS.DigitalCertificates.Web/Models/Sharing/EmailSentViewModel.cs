using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class EmailSentViewModel
    {
        public Guid CertificateId { get; set; }
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string FormattedExpiry { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public bool IsSingleCertificate { get; set; }
    }
}
