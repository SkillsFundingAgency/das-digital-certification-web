using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class SharedCertificateFrameworkViewModel
    {
        public Guid CertificateId { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public string? CertificateReference { get; set; }
        public string? FrameworkCertificateNumber { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public string? ProviderName { get; set; }
        public DateTime? StartDate { get; set; }
        public List<string>? QualificationsAndAwardingBodies { get; set; }
        public bool ShowBackLink { get; set; } = false;
        public string? FormattedExpiry { get; set; }
    }
}
