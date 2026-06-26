using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class Match
    {
        public required long Uln { get; set; }
        public required Guid? UserIdentityId { get; set; }
        public CertificateType CertificateType { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? ProviderName { get; set; }
        public long? Ukprn { get; set; }
    }

    public class Mask
    {
        public CertificateType CertificateType { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public string? ProviderName { get; set; }
    }

    public class MatchesAndMasks
    {
        public List<Match> Matches { get; set; } = new List<Match>();
        public List<Mask> Masks { get; set; } = new List<Mask>();
    }
}
