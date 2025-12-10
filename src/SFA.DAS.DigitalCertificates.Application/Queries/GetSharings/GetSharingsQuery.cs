using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharings
{
    public class GetSharingsQuery : IRequest<GetSharingsQueryResult?>
    {
        public required Guid UserId { get; set; }
        public required Guid CertificateId { get; set; }
        public int? Limit { get; set; }
    }
}