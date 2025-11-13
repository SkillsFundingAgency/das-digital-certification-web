using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreateOrUpdateUserRequest
    {
        public required string GovUkIdentifier { get; set; }
        public required string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public required List<Name> Names { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
