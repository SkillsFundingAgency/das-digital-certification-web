using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetSharedFrameworkCertificateResponse
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public required string CertificateType { get; set; }
        public string? CertificateReference { get; set; }
        public string? FrameworkCertificateNumber { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? ProviderName { get; set; }
        public string? EmployerName { get; set; }
        public DateTime? StartDate { get; set; }
        public List<QualificationDetailsResponse>? QualificationsAndAwardingBodies { get; set; }
    }

}
