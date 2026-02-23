using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode
{
    public class GetSharingByCodeQueryHandler : IRequestHandler<GetSharingByCodeQuery, GetSharingByCodeQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _api;

        public GetSharingByCodeQueryHandler(IDigitalCertificatesOuterApi api)
        {
            _api = api;
        }

        public async Task<GetSharingByCodeQueryResult?> Handle(GetSharingByCodeQuery request, CancellationToken cancellationToken)
        {
            var response = await _api.GetSharingByCode(request.Code);
            return (GetSharingByCodeQueryResult?)response;
        }
    }
}
