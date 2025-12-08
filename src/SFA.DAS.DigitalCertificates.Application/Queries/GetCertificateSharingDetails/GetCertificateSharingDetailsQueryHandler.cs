using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateSharingDetails
{
    public class GetCertificateSharingDetailsQueryHandler : IRequestHandler<GetCertificateSharingDetailsQuery, GetCertificateSharingDetailsQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetCertificateSharingDetailsQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetCertificateSharingDetailsQueryResult?> Handle(GetCertificateSharingDetailsQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetCertificateSharings(request.UserId.ToString(), request.CertificateId, request.Limit);
            return (GetCertificateSharingDetailsQueryResult?)response;
        }
    }
}