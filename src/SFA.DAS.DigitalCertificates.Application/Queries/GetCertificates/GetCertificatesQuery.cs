using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates
{
    public class GetCertificatesQuery : IRequest<GetCertificatesQueryResult>
    {
        public required Guid UserId { get; set; }
    }
}
