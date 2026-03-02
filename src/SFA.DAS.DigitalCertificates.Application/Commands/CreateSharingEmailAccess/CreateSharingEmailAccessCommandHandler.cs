using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmailAccess
{
    public class CreateSharingEmailAccessCommandHandler : IRequestHandler<CreateSharingEmailAccessCommand>
    {
        private readonly IDigitalCertificatesOuterApi _api;

        public CreateSharingEmailAccessCommandHandler(IDigitalCertificatesOuterApi api)
        {
            _api = api;
        }

        public async Task Handle(CreateSharingEmailAccessCommand request, CancellationToken cancellationToken)
        {
            await _api.CreateSharingEmailAccess(request);
        }
    }
}
