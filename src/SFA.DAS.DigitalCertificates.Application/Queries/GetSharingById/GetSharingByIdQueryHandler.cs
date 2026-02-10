using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById
{
    public class GetSharingByIdQueryHandler : IRequestHandler<GetSharingByIdQuery, GetSharingByIdQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetSharingByIdQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetSharingByIdQueryResult?> Handle(GetSharingByIdQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetSharingById(request.SharingId, request.Limit);
            return (GetSharingByIdQueryResult?)response;
        }
    }
}
