using System;
using System.Linq;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class AddAddressManualViewModel
    {
        public Guid CertificateId { get; set; }
        public string? CourseName { get; set; }
        public string? GivenNames { get; set; }
        public string? FamilyName { get; set; }
        public string? BackRoute { get; set; }
        public string FullName => string.Join(" ", new[] { GivenNames, FamilyName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
        public string? Organisation { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? TownOrCity { get; set; }
        public string? County { get; set; }
        public string? Postcode { get; set; }
    }
}
