using System;
using System.Linq;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class SelectAddressViewModel
    {
        public Guid CertificateId { get; set; }
        public string? CourseName { get; set; }
        public string? GivenNames { get; set; }
        public string? FamilyName { get; set; }
        public string FullName => string.Join(" ", new[] { GivenNames, FamilyName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        public string? SearchTerm { get; set; }
        public string? SelectedAddress { get; set; }
    }
}
