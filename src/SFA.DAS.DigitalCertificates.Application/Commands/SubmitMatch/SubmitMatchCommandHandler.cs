using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.SubmitMatch
{
    public class SubmitMatchCommandHandler : IRequestHandler<SubmitMatchCommand, Unit>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public SubmitMatchCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<Unit> Handle(SubmitMatchCommand request, CancellationToken cancellationToken)
        {
            var req = (SubmitMatchRequest)request;

            await _outerApi.SubmitMatch(request.UserId, req);
            return Unit.Value;
        }
    }
}
