using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingAccess
{
    public class CreateSharingAccessCommand : IRequest
    {
        public Guid SharingId { get; set; }

        public static implicit operator CreateSharingAccessRequest(CreateSharingAccessCommand command)
        {
            return new CreateSharingAccessRequest
            {
                SharingId = command.SharingId
            };
        }
    }
}
