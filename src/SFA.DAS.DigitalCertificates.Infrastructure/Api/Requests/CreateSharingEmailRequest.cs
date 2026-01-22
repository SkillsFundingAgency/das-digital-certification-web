namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreateSharingEmailRequest
    {
        public required string EmailAddress { get; set; }
        public required string UserName { get; set; }
        public required string LinkDomain { get; set; }
        public required string MessageText { get; set; }
        public required string TemplateId { get; set; }
    }
}
