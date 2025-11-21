using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates
{
    public class GetCertificatesQueryHandler : IRequestHandler<GetCertificatesQuery, GetCertificatesQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetCertificatesQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetCertificatesQueryResult?> Handle(GetCertificatesQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetCertificates(request.UserId);
            return (GetCertificatesQueryResult?)response;
        }
    }
}
