using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail
{
    public class CreateSharingEmailCommandResult
    {
        public Guid Id { get; set; }
        public Guid EmailLinkCode { get; set; }

        public static implicit operator CreateSharingEmailCommandResult?(CreateSharingEmailResponse? source)
        {
            if (source == null)
            {
                return null;
            }

            return new CreateSharingEmailCommandResult
            {
                Id = source.Id,
                EmailLinkCode = source.EmailLinkCode
            };
        }
    }
}
