using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.DeleteSharing
{
    public class DeleteSharingCommandHandler : IRequestHandler<DeleteSharingCommand, Unit>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public DeleteSharingCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<Unit> Handle(DeleteSharingCommand request, CancellationToken cancellationToken)
        {
            await _outerApi.DeleteSharing(request.SharingId);
            return Unit.Value;
        }
    }
}
