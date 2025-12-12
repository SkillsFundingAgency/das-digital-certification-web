using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Web.Models.Home
{
    public class CreateOrUpdateUserModel
    {
        public required string GovUkIdentifier { get; set; }
        public required string EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }

        public List<NameModel>? Names { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
