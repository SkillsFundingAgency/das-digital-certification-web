using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Types
{
    public class Name
    {
        public DateTime? ValidSince { get; set; }
        public DateTime? ValidUntil { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }

    }
}
