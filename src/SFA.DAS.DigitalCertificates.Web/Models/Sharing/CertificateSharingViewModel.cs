using System;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class CertificateSharingViewModel
    {
        public Guid CertificateId { get; set; }
        public required string CourseName { get; set; }
        public CertificateType CertificateType { get; set; }
        public List<CertificateSharingItemViewModel> Sharings { get; set; } = new List<CertificateSharingItemViewModel>();
        public bool HasSharings => Sharings != null && Sharings.Count > 0;
    }

    public class CertificateSharingItemViewModel
    {
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
