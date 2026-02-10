using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingAccess
{
    public class CreateSharingAccessCommandHandler : IRequestHandler<CreateSharingAccessCommand>
    {
        private readonly IDigitalCertificatesOuterApi _api;

        public CreateSharingAccessCommandHandler(IDigitalCertificatesOuterApi api)
        {
            _api = api;
        }

        public async Task Handle(CreateSharingAccessCommand request, CancellationToken cancellationToken)
        {
            if (request.SharingId.HasValue)
            {
                await _api.CreateSharingAccess(request);
            }
            else if (request.SharingEmailId.HasValue)
            {
                await _api.CreateSharingEmailAccess(request);
            }
        }
    }
}
