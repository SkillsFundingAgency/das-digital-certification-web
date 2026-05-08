using System;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class UserDetails
    {
        public string GivenNames { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
    }
}
