using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById
{
    public class GetSharingByIdQueryResult
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public required string CourseName { get; set; }
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LinkCode { get; set; }
        public DateTime ExpiryTime { get; set; }
        public List<DateTime>? SharingAccess { get; set; }
        public List<SharingEmail>? SharingEmails { get; set; }

        public static implicit operator GetSharingByIdQueryResult?(GetSharingByIdResponse? source)
        {
            if (source is null)
            {
                return null;
            }

            return new GetSharingByIdQueryResult
            {
                UserId = source.UserId,
                CertificateId = source.CertificateId,
                CertificateType = Enum.Parse<CertificateType>(source.CertificateType),
                CourseName = source.CourseName,
                SharingId = source.SharingId,
                SharingNumber = source.SharingNumber,
                CreatedAt = source.CreatedAt,
                LinkCode = source.LinkCode,
                ExpiryTime = source.ExpiryTime,
                SharingAccess = source.SharingAccess ?? new List<DateTime>(),
                SharingEmails = source.SharingEmails != null
                    ? source.SharingEmails.Where(e => e is not null).Select(e => (SharingEmail)e!).ToList()
                    : new List<SharingEmail>()
            };
        }
    }
}
