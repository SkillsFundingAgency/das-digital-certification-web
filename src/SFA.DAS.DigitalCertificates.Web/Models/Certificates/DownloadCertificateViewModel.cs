using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class DownloadCertificateViewModel
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public string FullName => $"{FamilyName} \n {GivenNames}";
        public required string StandardName { get; set; }
        public required string OptionName { get; set; }
        public required string Level { get; set; }
        public required string Result { get; set; }
        public required DateTime? DateAwarded { get; set; }
        public required string CertificationNumber { get; set; }
        public bool CoronationEmblem { get; set; }
    }
}
