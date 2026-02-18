using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate
{
    public class GetSharedFrameworkCertificateQuery : IRequest<GetSharedFrameworkCertificateQueryResult?>
    {
        public required Guid Id { get; set; }
    }
}
