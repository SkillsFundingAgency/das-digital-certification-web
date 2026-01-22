using MediatR;

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
    }
}
