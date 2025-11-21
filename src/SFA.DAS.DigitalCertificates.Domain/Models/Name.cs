using System;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class Name
    {
        public DateTime? ValidSince { get; set; }
        public DateTime? ValidUntil { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }

        public static implicit operator Name?(Infrastructure.Api.Types.Name? source)
        {
            if (source == null)
            {
                return null;
            }

            return new Name
            {
                ValidSince = source.ValidSince,
                ValidUntil = source.ValidUntil,
                FamilyName = source.FamilyName,
                GivenNames = source.GivenNames
            };
        }
    }
}
