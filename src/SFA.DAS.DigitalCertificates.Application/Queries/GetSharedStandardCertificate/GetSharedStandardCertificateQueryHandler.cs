using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate
{
    public class GetSharedStandardCertificateQueryHandler : IRequestHandler<GetSharedStandardCertificateQuery, GetSharedStandardCertificateQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _api;

        public GetSharedStandardCertificateQueryHandler(IDigitalCertificatesOuterApi api)
        {
            _api = api;
        }

        public async Task<GetSharedStandardCertificateQueryResult?> Handle(GetSharedStandardCertificateQuery request, CancellationToken cancellationToken)
        {
            var response = await _api.GetSharedStandardCertificate(request.Id);
            return response;
        }
    }
}
