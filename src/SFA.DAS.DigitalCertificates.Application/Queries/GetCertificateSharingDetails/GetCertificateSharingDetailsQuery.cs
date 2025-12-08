using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateSharingDetails
{
    public class GetCertificateSharingDetailsQuery : IRequest<GetCertificateSharingDetailsQueryResult?>
    {
        public required Guid UserId { get; set; }
        public required Guid CertificateId { get; set; }
        public int Limit { get; set; } = 10;
    }
}