using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode
{
    public class GetSharingByCodeQueryHandler : IRequestHandler<GetSharingByCodeQuery, GetSharingByCodeQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _api;
        private readonly ILogger<GetSharingByCodeQueryHandler> _logger;

        public GetSharingByCodeQueryHandler(IDigitalCertificatesOuterApi api, ILogger<GetSharingByCodeQueryHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<GetSharingByCodeQueryResult?> Handle(GetSharingByCodeQuery request, CancellationToken cancellationToken)
        {
            var response = await _api.GetSharingByCode(request.Code);
            return (GetSharingByCodeQueryResult?)response;
        }
    }
}
