using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateById
{
    public class GetCertificateByIdQueryHandler : IRequestHandler<GetCertificateByIdQuery, GetCertificateByIdQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetCertificateByIdQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetCertificateByIdQueryResult?> Handle(GetCertificateByIdQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetCertificateById(request.CertificateId);
            return (GetCertificateByIdQueryResult?)response;
        }
    }
}