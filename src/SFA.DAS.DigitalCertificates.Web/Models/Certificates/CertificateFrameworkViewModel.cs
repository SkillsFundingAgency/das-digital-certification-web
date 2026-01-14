using System;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class CertificateFrameworkViewModel
    {
        public Guid CertificateId { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public long? Uln { get; set; }
        public CertificateType CertificateType { get; set; }
        public string? CertificateReference { get; set; }
        public string? FrameworkCertificateNumber { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public string? ProviderName { get; set; }
        public long? Ukprn { get; set; }
        public string? EmployerName { get; set; }
        public string? AssessorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PrintRequestedAt { get; set; }
        public string? PrintRequestedBy { get; set; }
        public bool ShowBackLink { get; set; } = true;
        public List<string>? QualificationsAndAwardingBodies { get; set; }
        public List<string>? DeliveryInformation { get; set; }
    }
}
