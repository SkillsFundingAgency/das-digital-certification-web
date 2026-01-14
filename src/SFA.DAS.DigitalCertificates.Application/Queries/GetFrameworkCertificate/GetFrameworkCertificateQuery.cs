using MediatR;
namespace SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate
{
    public class GetFrameworkCertificateQuery : IRequest<GetFrameworkCertificateResult>
    {
        public required Guid CertificateId { get; set; }
    }
}
