using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate
{
    public class GetSharedFrameworkCertificateQueryResult
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public string? CertificateType { get; set; }
        public List<string>? QualificationsAndAwardingBodies { get; set; }
        public string? CertificateReference { get; set; }
        public string? FrameworkCertificateNumber { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? ProviderName { get; set; }
        public string? EmployerName { get; set; }
        public DateTime? StartDate { get; set; }

        public static implicit operator GetSharedFrameworkCertificateQueryResult?(GetSharedFrameworkCertificateResponse? source)
        {
            if (source is null) return null;

            return new GetSharedFrameworkCertificateQueryResult
            {
                FamilyName = source.FamilyName,
                GivenNames = source.GivenNames,
                CertificateType = source.CertificateType,
                QualificationsAndAwardingBodies = source.QualificationsAndAwardingBodies?
                    .Select(q => FormatQualification(q))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!)
                    .ToList(),
                CertificateReference = source.CertificateReference,
                FrameworkCertificateNumber = source.FrameworkCertificateNumber,
                CourseName = source.CourseName,
                CourseOption = source.CourseOption,
                CourseLevel = source.CourseLevel,
                DateAwarded = source.DateAwarded,
                ProviderName = source.ProviderName,
                EmployerName = source.EmployerName,
                StartDate = source.StartDate
            };
        }

        private static string? FormatQualification(QualificationDetailsResponse? q)
        {
            if (q is null) return null;

            var name = (q.Name ?? string.Empty).Trim();
            var awardingBody = (q.AwardingBody ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(awardingBody))
                return null;

            if (string.IsNullOrWhiteSpace(name)) return awardingBody;
            if (string.IsNullOrWhiteSpace(awardingBody)) return name;

            return $"{name}, {awardingBody}";
        }
    }
}
