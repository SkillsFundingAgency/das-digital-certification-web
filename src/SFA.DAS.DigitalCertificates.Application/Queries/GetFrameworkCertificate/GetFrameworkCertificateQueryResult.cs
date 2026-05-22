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
        public List<DeliveryInformationResponse>? DeliveryInformation { get; set; }

        public static implicit operator GetFrameworkCertificateQueryResult?(GetFrameworkCertificateResponse? source)
        {
            if (source is null) return null;

            return new GetFrameworkCertificateQueryResult
            {
                FamilyName = source.FamilyName,
                GivenNames = source.GivenNames,
                Uln = source.Uln,
                CertificateType = source.CertificateType,
                FrameworkCertificateNumber = source.FrameworkCertificateNumber,
                CourseName = source.CourseName,
                CourseOption = source.CourseOption,
                CourseLevel = source.CourseLevel,
                DateAwarded = source.DateAwarded,
                ProviderName = source.ProviderName,
                EmployerName = source.EmployerName,
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