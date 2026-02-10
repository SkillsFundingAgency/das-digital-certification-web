using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingAccess
{
    public class CreateSharingAccessCommand : IRequest
    {
        public Guid? SharingId { get; set; }
        public Guid? SharingEmailId { get; set; }

        public static implicit operator CreateSharingAccessRequest(CreateSharingAccessCommand command)
        {
            return new CreateSharingAccessRequest
            {
                SharingId = command.SharingId ?? Guid.Empty
            };
        }

        public static implicit operator CreateSharingEmailAccessRequest(CreateSharingAccessCommand command)
        {
            return new CreateSharingEmailAccessRequest
            {
                SharingEmailId = command.SharingEmailId ?? Guid.Empty
            };
        }
    }
}
