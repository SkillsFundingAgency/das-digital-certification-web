using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateById
{
    public class GetCertificateByIdQueryResult
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public string? Uln { get; set; }
        public required string CertificateType { get; set; }
        public string? CertificateReference { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? OverallGrade { get; set; }
        public string? ProviderName { get; set; }
        public string? Ukprn { get; set; }
        public string? EmployerName { get; set; }
        public string? AssessorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PrintRequestedAt { get; set; }
        public string? PrintRequestedBy { get; set; }

        public static implicit operator GetCertificateByIdQueryResult?(GetCertificateByIdResponse? source)
        {
            if (source == null)
            {
                return null;
            }

            return new GetCertificateByIdQueryResult
            {
                FamilyName = source.FamilyName,
                GivenNames = source.GivenNames,
                Uln = source.Uln,
                CertificateType = source.CertificateType,
                CertificateReference = source.CertificateReference,
                CourseCode = source.CourseCode,
                CourseName = source.CourseName,
                CourseOption = source.CourseOption,
                CourseLevel = source.CourseLevel,
                DateAwarded = source.DateAwarded,
                OverallGrade = source.OverallGrade,
                ProviderName = source.ProviderName,
                Ukprn = source.Ukprn,
                EmployerName = source.EmployerName,
                AssessorName = source.AssessorName,
                StartDate = source.StartDate,
                PrintRequestedAt = source.PrintRequestedAt,
                PrintRequestedBy = source.PrintRequestedBy
            };
        }
    }
}