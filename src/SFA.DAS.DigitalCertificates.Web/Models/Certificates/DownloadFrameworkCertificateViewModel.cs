using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class DownloadFrameworkCertificateViewModelDelete
    {
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }
        public string FullName => $"{GivenNames} \n {FamilyName}";
        public required string CourseName { get; set; }
        public string? CourseOption { get; set; }
        public required string Level { get; set; }       
        public required DateTime DateAwarded { get; set; }
        public required string FrameworkCertificateNumber { get; set; }
    }
}