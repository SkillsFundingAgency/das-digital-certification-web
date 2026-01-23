using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
namespace SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate
{
    public class GetFrameworkCertificateQueryHandler : IRequestHandler<GetFrameworkCertificateQuery, GetFrameworkCertificateQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetFrameworkCertificateQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetFrameworkCertificateQueryResult?> Handle(GetFrameworkCertificateQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetFrameworkCertificate(request.CertificateId);
            return (GetFrameworkCertificateQueryResult?)response;
        }
    }
}
