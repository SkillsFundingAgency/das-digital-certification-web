using System;
using SFA.DAS.DigitalCertificates.Web.Enums;
namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class SharedCertificateStandardViewModel
    {
        public Guid CertificateId { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public string? CertificateReference { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public int? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public Grade OverallGradeEnum => OverallGrade.ParseFromApi();
        public string OverallGradeBannerDisplay => OverallGradeEnum.ToBannerDisplay();
        public string OverallGradeResultDisplay => OverallGradeEnum.ToResultDisplay();
        public string? ProviderName { get; set; }
        public DateTime? StartDate { get; set; }
        public bool ShowBackLink { get; set; } = false;
        public string? FormattedExpiry { get; set; }
    }
}
