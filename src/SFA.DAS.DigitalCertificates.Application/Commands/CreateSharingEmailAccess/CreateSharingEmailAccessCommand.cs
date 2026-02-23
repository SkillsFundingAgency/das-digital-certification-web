using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmailAccess
{
    public class CreateSharingEmailAccessCommand : IRequest
    {
        public Guid SharingEmailId { get; set; }

        public static implicit operator CreateSharingEmailAccessRequest(CreateSharingEmailAccessCommand command)
        {
            return new CreateSharingEmailAccessRequest
            {
                SharingEmailId = command.SharingEmailId
            };
        }
    }
}
