using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Commands.DeleteSharing
{
    public class DeleteSharingCommand : IRequest<Unit>
    {
        public required Guid SharingId { get; set; }
    }
}
