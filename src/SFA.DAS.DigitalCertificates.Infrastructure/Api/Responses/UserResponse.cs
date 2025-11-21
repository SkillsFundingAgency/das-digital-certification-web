using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public required string GovUkIdentifier { get; set; }
        public required string EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LockedAt { get; set; }
    }
}
