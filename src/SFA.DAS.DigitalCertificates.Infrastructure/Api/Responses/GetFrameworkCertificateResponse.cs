using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetFrameworkCertificateResponse
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public long? Uln { get; set; }
        public required string CertificateType { get; set; }
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
        public List<QualificationDetailsResponse>? QualificationsAndAwardingBodies { get; set; }
        public List<string>? DeliveryInformation { get; set; }
    }

    public class QualificationDetailsResponse
    {
        public string? Name { get; set; }
        public string? AwardingBody { get; set; }
    }
}
