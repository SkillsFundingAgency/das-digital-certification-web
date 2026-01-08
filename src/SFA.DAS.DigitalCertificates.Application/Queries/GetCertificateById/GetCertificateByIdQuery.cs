using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateById
{
    public class GetCertificateByIdQuery : IRequest<GetCertificateByIdQueryResult>
    {
        public required Guid CertificateId { get; set; }
    }
}