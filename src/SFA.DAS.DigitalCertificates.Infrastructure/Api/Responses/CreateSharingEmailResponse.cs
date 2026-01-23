using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class CreateSharingEmailResponse
    {
        public Guid Id { get; set; }
        public Guid EmailLinkCode { get; set; }
    }
}
