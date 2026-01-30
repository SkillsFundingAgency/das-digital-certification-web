using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing
{
    public class CreateSharingCommandHandler : IRequestHandler<CreateSharingCommand, CreateSharingCommandResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public CreateSharingCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<CreateSharingCommandResult?> Handle(CreateSharingCommand command, CancellationToken cancellationToken)
        {
            var response = await _outerApi.CreateSharing(command);
            return response;
        }
    }
}