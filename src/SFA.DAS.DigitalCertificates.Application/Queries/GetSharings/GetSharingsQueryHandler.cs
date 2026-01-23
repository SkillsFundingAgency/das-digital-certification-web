using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharings
{
    public class GetSharingsQueryHandler : IRequestHandler<GetSharingsQuery, GetSharingsQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetSharingsQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetSharingsQueryResult?> Handle(GetSharingsQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetSharings(request.UserId.ToString(), request.CertificateId, request.Limit);
            return (GetSharingsQueryResult?)response;
        }
    }
}