namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreatePrintRequest
    {
        public PrintAddressDto Address { get; set; } = null!;
        public PrintEmailDto Email { get; set; } = null!;
    }

    public class PrintAddressDto
    {
        public string? ContactName { get; set; }
        public string? ContactOrganisation { get; set; }
        public string? ContactAddLine1 { get; set; }
        public string? ContactAddLine2 { get; set; }
        public string? ContactAddLine3 { get; set; }
        public string? ContactAddLine4 { get; set; }
        public string? ContactPostCode { get; set; }
    }

    public class PrintEmailDto
    {
        public string? EmailAddress { get; set; }
        public string? UserName { get; set; }
        public string? LinkDomain { get; set; }
        public string? TemplateId { get; set; }
    }
}
