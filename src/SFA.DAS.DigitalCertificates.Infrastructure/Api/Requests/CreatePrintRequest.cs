namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreatePrintRequest
    {
        public CreatePrintAddressRequest Address { get; set; } = null!;
        public CreatePrintEmailRequest Email { get; set; } = null!;
    }

    public class CreatePrintAddressRequest
    {
        public required string ContactName { get; set; }
        public string? ContactOrganisation { get; set; }
        public string? ContactAddLine1 { get; set; }
        public string? ContactAddLine2 { get; set; }
        public string? ContactAddLine3 { get; set; }
        public string? ContactAddLine4 { get; set; }
        public required string ContactPostCode { get; set; }
    }

    public class CreatePrintEmailRequest
    {
        public required string EmailAddress { get; set; }
        public required string UserName { get; set; }
        public required string LinkDomain { get; set; }
        public required string TemplateId { get; set; }
    }
}
