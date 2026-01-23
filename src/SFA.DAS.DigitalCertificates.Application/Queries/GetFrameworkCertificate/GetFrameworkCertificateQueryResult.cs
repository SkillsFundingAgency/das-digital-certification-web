using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate
{
    public class GetFrameworkCertificateQueryResult
    {
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public long? Uln { get; set; }
        public required string CertificateType { get; set; }
        public string? FrameworkCertificateNumber { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? ProviderName { get; set; }
        public string? EmployerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PrintRequestedAt { get; set; }
        public string? PrintRequestedBy { get; set; }
        public List<string>? QualificationsAndAwardingBodies { get; set; }

        // TODO: The fields below are not available from the outer API and are not required for P2-2551.
        // They can be populated in future tickets if needed or it can be removed if not required for the upcoming tickets
        public string? CertificateReference { get; set; }
        public string? OverallGrade { get; set; }
        public string? CourseCode { get; set; }
        public string? AssessorName { get; set; }
        public long? Ukprn { get; set; }
        public List<string>? DeliveryInformation { get; set; }

        public static implicit operator GetFrameworkCertificateQueryResult?(GetFrameworkCertificateResponse? source)
        {
            if (source is null) return null;

            return new GetFrameworkCertificateQueryResult
            {
                FamilyName = source.FamilyName,
                GivenNames = source.GivenNames,
                Uln = source.Uln,
                CertificateType = source.CertificateType,
                CertificateReference = source.CertificateReference,
                FrameworkCertificateNumber = source.FrameworkCertificateNumber,
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
                PrintRequestedBy = source.PrintRequestedBy,
                QualificationsAndAwardingBodies = source.QualificationsAndAwardingBodies?
                    .Select(FormatQualification)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!)
                    .ToList(),
                DeliveryInformation = source.DeliveryInformation
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