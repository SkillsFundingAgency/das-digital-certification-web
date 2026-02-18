using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate
{
    public class GetSharedFrameworkCertificateQueryHandler : IRequestHandler<GetSharedFrameworkCertificateQuery, GetSharedFrameworkCertificateQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _api;

        public GetSharedFrameworkCertificateQueryHandler(IDigitalCertificatesOuterApi api)
        {
            _api = api;
        }

        public async Task<GetSharedFrameworkCertificateQueryResult?> Handle(GetSharedFrameworkCertificateQuery request, CancellationToken cancellationToken)
        {
            var response = await _api.GetSharedFrameworkCertificate(request.Id);
            return response; 
        }
    }
}
