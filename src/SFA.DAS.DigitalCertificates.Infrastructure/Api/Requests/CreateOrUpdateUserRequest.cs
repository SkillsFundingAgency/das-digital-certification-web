using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreateOrUpdateUserRequest
    {
        public required string GovUkIdentifier { get; set; }
        public required string EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
