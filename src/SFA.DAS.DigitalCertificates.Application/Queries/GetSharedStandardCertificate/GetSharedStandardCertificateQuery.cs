using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate
{
    public class GetSharedStandardCertificateQuery : IRequest<GetSharedStandardCertificateQueryResult?>
    {
        public required Guid Id { get; set; }
    }
}
