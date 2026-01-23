using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate
{
    public class GetStandardCertificateQuery : IRequest<GetStandardCertificateQueryResult>
    {
        public required Guid CertificateId { get; set; }
    }
}