using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class MatchResponse
    {
        public string? Uln { get; set; }
        public required string CertificateType { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? ProviderName { get; set; }
        public string? Ukprn { get; set; }
    }

    public class MaskResponse
    {
        public required string CertificateType { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public string? ProviderName { get; set; }
    }

    public class MatchesResponse
    {
        public List<MatchResponse> Matches { get; set; } = new List<MatchResponse>();
        public List<MaskResponse> Masks { get; set; } = new List<MaskResponse>();
    }
}
