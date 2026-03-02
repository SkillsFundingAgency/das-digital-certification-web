using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail
{
    public class CreateSharingEmailCommand : IRequest<CreateSharingEmailCommandResult?>
    {
        public Guid SharingId { get; set; }
        public required string EmailAddress { get; set; }
        public required string UserName { get; set; }
        public required string LinkDomain { get; set; }
        public required string MessageText { get; set; }
        public required string TemplateId { get; set; }

        public static implicit operator CreateSharingEmailRequest(CreateSharingEmailCommand command)
        {
            return new CreateSharingEmailRequest
            {
                EmailAddress = command.EmailAddress,
                UserName = command.UserName,
                LinkDomain = command.LinkDomain,
                MessageText = command.MessageText,
                TemplateId = command.TemplateId
            };
        }
    }
}
