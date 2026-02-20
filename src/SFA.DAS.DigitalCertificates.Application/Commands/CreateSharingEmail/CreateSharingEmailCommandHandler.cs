using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail
{
    public class CreateSharingEmailCommandHandler : IRequestHandler<CreateSharingEmailCommand, CreateSharingEmailCommandResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public CreateSharingEmailCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<CreateSharingEmailCommandResult?> Handle(CreateSharingEmailCommand request, CancellationToken cancellationToken)
        {
            var apiRequest = (CreateSharingEmailRequest)request;

            var response = await _outerApi.CreateSharingEmail(request.SharingId, apiRequest);

            if (response == null)
                return null;

            return response; 
        }
    }
}
