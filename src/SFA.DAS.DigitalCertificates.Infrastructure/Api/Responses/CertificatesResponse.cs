using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class CertificatesResponse
    {
        public UlnAuthorisation Authorisation { get; set; }
        public List<Certificate> Certificates { get; set; }
    }
}
