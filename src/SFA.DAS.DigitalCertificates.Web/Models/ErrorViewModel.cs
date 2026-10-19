using System;

namespace SFA.DAS.DigitalCertificates.Web.Models
{
    public class ErrorViewModel
    {
        public const string SupportEmailAddress =
            "helpdesk@manage-apprenticeships.service.gov.uk";

        public required string ProblemReference { get; init; }

        public string SupportEmailSubject =>
            $"Support with Apprenticeship certificates, reference: {ProblemReference}";

        public string SupportEmailUrl =>
            $"mailto:{SupportEmailAddress}?subject={Uri.EscapeDataString(SupportEmailSubject)}";
    }
}
