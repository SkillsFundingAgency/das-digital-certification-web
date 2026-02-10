using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class CheckQualificationViewModel
    {
        public required Guid Code { get; set; }
        public string? FormattedExpiry { get; set; }
    }
}
