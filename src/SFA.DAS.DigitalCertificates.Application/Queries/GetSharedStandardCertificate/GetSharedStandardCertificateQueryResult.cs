using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate
{
    public class GetSharedStandardCertificateQueryResult
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public string? CertificateType { get; set; }
        public string? CertificateReference { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public int? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public string? ProviderName { get; set; }
        public DateTime? StartDate { get; set; }

        public static implicit operator GetSharedStandardCertificateQueryResult?(GetSharedStandardCertificateResponse? source)
        {
            if (source is null) return null;

            return new GetSharedStandardCertificateQueryResult
            {
                FamilyName = source.FamilyName,
                GivenNames = source.GivenNames,
                CertificateType = source.CertificateType,
                CertificateReference = source.CertificateReference,
                CourseName = source.CourseName,
                CourseOption = source.CourseOption,
                CourseLevel = source.CourseLevel,
                DateAwarded = source.DateAwarded,
                OverallGrade = source.OverallGrade,
                ProviderName = source.ProviderName,
                StartDate = source.StartDate
            };
        }
    }
}
