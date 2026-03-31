using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class DownloadCertificateViewModel
    {
        public string FamilyName { get; set; }
        public string GivenNames { get; set; }
        public string FullName => $"{GivenNames} {FamilyName}";
        public required string StandardName { get; set; }
        public string? OptionName { get; set; }
        public required string Level { get; set; }
        public required string Result { get; set; }
        public required DateTime DateAwarded { get; set; }
        public required string CertificateNumber { get; set; }
        public bool CoronationEmblem { get; set; }
    }
}
