using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions
{
    public class GetUserActionsQueryHandler : IRequestHandler<GetUserActionsQuery, GetUserActionsQueryResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetUserActionsQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetUserActionsQueryResult?> Handle(GetUserActionsQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetUserActions(request.UserId);
            return (GetUserActionsQueryResult?)response;
        }
    }
}
