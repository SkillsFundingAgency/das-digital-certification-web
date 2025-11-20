using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.User
{
    public class NameModel
    {
        public DateTime? ValidSince { get; set; }
        public DateTime? ValidUntil { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }

    }
}
