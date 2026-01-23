using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate
{
    public class GetStandardCertificateQueryHandler : IRequestHandler<GetStandardCertificateQuery, GetStandardCertificateQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetStandardCertificateQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetStandardCertificateQueryResult?> Handle(GetStandardCertificateQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetStandardCertificate(request.CertificateId);
            return (GetStandardCertificateQueryResult?)response;
        }
    }
}