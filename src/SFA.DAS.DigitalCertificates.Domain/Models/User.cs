using System;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string GovUkIdentifier { get; set; }
        public required string EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LockedAt { get; set; }

        public static implicit operator User?(UserResponse? source)
        {
            if (source == null)
            {
                return null;
            }

            return new User
            {
                Id = source.Id,
                GovUkIdentifier = source.GovUkIdentifier,
                EmailAddress = source.EmailAddress,
                PhoneNumber = source.PhoneNumber,
                LastLoginAt = source.LastLoginAt,
                LockedAt = source.LockedAt
            };
        }
    }
}
