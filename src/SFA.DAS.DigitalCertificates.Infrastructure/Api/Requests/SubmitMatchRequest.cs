using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class SubmitMatchRequest
    {
        public long? Uln { get; set; }
        public required string FamilyName { get; set; }
        public required DateTime DateOfBirth { get; set; }

        public string? CertificateType { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public int? YearAwarded { get; set; }

        public string? ProviderName { get; set; }
        public int? Ukprn { get; set; }

        public bool IsMatched { get; set; }
        public bool IsFailed { get; set; }
    }
}
