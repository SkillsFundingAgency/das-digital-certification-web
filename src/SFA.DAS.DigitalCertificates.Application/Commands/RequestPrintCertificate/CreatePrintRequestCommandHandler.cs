using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate
{
    public class CreatePrintRequestCommandHandler : IRequestHandler<CreatePrintRequestCommand>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public CreatePrintRequestCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task Handle(CreatePrintRequestCommand request, CancellationToken cancellationToken)
        {
            await _outerApi.CreatePrintRequest(request.CertificateId, request.Request);
        }
    }
}
