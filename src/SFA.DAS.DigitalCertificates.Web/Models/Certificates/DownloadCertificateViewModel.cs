using SFA.DAS.DigitalCertificates.Domain.Models;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class DownloadCertificateViewModel
    {
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }
        public string FullName => $"{GivenNames} \n {FamilyName}";
        public required string CourseName { get; set; }
        public string? CourseOption { get; set; }
        public required string CourseLevel { get; set; }
        public string? OverallGrade { get; set; }
        public required DateTime DateAwarded { get; set; }
        public required string CertificateNumber { get; set; }
        public bool CoronationEmblem { get; set; }
        public CertificateType CertificateType { get; set; }
        public string SanitisedAnonymousCertificateName
        {
            get
            {
                var safeGivenNames = Regex.Replace(GivenNames, "[^a-zA-Z0-9]+", "_").Trim('_');
                var safeFamilyName = Regex.Replace(FamilyName, "[^a-zA-Z0-9]+", "_").Trim('_');

                return $"{safeGivenNames}_{safeFamilyName}_CertificateNumber{CertificateNumber}.pdf";
            }
        }
    }
}
