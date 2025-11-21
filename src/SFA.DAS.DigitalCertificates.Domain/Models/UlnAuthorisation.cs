using System;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class UlnAuthorisation
    {
        public Guid AuthorisationId { get; set; }
        public DateTime AuthorisedAt { get; set; }
        public required string Uln { get; set; }

        public static implicit operator UlnAuthorisation?(Infrastructure.Api.Types.UlnAuthorisation? source)
        {
            if (source == null)
            {
                return null;
            }

            return new UlnAuthorisation
            {
                AuthorisationId = source.AuthorisationId,
                AuthorisedAt = source.AuthorisedAt,
                Uln = source.Uln
            };
        }
    }
}
