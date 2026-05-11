using SFA.DAS.DigitalCertificates.Domain.Models;
using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class DownloadCertificateRequestViewModel
    {
        public Guid CertificateId { get; set; }

        public CertificateType CertificateType { get; set; }

        public string? FamilyName { get; set; }

        public string? GivenNames { get; set; }

        public string? CourseName { get; set; }

        public string? CourseOption { get; set; }

        public string? CourseLevel { get; set; }

        public DateTime? DateAwarded { get; set; }

        public string? OverallGrade { get; set; }

        public required string CertificateNumber { get; set; }
        
        public bool CoronationEmblem { get; set; }
    }
}
