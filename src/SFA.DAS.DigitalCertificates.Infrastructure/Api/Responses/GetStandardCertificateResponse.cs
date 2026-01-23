using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetStandardCertificateResponse
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public long? Uln { get; set; }
        public required string CertificateType { get; set; }
        public string? CertificateReference { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public int? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public string? ProviderName { get; set; }
        public string? Ukprn { get; set; }
        public string? EmployerName { get; set; }
        public string? AssessorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PrintRequestedAt { get; set; }
        public string? PrintRequestedBy { get; set; }
    }
}