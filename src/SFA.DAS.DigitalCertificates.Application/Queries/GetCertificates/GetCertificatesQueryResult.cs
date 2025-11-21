using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates
{
    public class GetCertificatesQueryResult
    {
        public UlnAuthorisation? Authorisation { get; set; }
        public List<Certificate>? Certificates { get; set; }

        public static implicit operator GetCertificatesQueryResult?(CertificatesResponse? source)
        {
            if (source == null)
            {
                return null;
            }

            return new GetCertificatesQueryResult
            {
                Authorisation = (UlnAuthorisation?)source.Authorisation,
                Certificates = source.Certificates != null
                    ? source.Certificates
                        .Where(c => c is not null)
                        .Select(c => (Certificate)c!)
                        .ToList()
                    : null
            };
        }
    }
}
