using System;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class UpdateUserIdentityRequest
    {
        public List<Name>? Names { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
