using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Types
{
    public class UlnAuthorisation
    {
        public Guid AuthorisationId { get; set; }
        public DateTime AuthorisedAt { get; set; }
        public required string Uln { get; set; }
    }
}
