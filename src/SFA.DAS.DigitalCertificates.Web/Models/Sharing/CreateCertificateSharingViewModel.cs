using System;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class CreateCertificateSharingViewModel
    {
        public Guid CertificateId { get; set; }
        public required string CourseName { get; set; }
        public CertificateType CertificateType { get; set; }
        public List<CreateCertificateSharingItemViewModel> Sharings { get; set; } = new List<CreateCertificateSharingItemViewModel>();
        public bool HasSharings => Sharings != null && Sharings.Count > 0;
    }

    public class CreateCertificateSharingItemViewModel
    {
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
