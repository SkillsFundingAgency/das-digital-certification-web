using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.SubmitMatch
{
    public class SubmitMatchCommand : IRequest<Unit>
    {
        public required Guid UserId { get; set; }

        public long? Uln { get; set; }
        public required string FamilyName { get; set; }
        public required DateTime DateOfBirth { get; set; }

        public string? CertificateType { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public int? YearAwarded { get; set; }

        public string? ProviderName { get; set; }
        public int? Ukprn { get; set; }

        public bool IsMatched { get; set; }
        public bool IsFailed { get; set; }

        public static implicit operator SubmitMatchRequest(SubmitMatchCommand c)
        {
            return new SubmitMatchRequest
            {
                Uln = c.Uln,
                FamilyName = c.FamilyName,
                DateOfBirth = c.DateOfBirth,
                CertificateType = c.CertificateType,
                CourseCode = c.CourseCode,
                CourseName = c.CourseName,
                CourseLevel = c.CourseLevel,
                YearAwarded = c.YearAwarded,
                ProviderName = c.ProviderName,
                Ukprn = c.Ukprn,
                IsMatched = c.IsMatched,
                IsFailed = c.IsFailed
            };
        }
    }
}
