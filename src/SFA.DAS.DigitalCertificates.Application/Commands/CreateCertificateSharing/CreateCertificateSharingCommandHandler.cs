using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateCertificateSharing
{
    public class CreateCertificateSharingCommandHandler : IRequestHandler<CreateCertificateSharingCommand, CreateCertificateSharingCommandResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public CreateCertificateSharingCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<CreateCertificateSharingCommandResult?> Handle(CreateCertificateSharingCommand command, CancellationToken cancellationToken)
        {
            var response = await _outerApi.CreateCertificateSharing(command);
            return response;
        }
    }
}