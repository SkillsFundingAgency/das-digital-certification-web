using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetSharedStandardCertificateResponse
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public required string CertificateType { get; set; }
        public string? CertificateReference { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public int? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public string? ProviderName { get; set; }
        public DateTime? StartDate { get; set; }

        
    }
}
