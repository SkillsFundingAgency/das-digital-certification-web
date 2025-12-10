using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharings
{
    public class GetSharingsQueryResult
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public required string CourseName { get; set; }
        public List<CertificateSharingDetailsQueryResultItem> Sharings { get; set; } = new List<CertificateSharingDetailsQueryResultItem>();

        public static implicit operator GetSharingsQueryResult?(GetSharingsResponse? source)
        {
            if (source == null)
            {
                return null;
            }

            return new GetSharingsQueryResult
            {
                UserId = source.UserId,
                CertificateId = source.CertificateId,
                CertificateType = source.CertificateType,
                CourseName = source.CourseName,
                Sharings = source.Sharings?.Select(s => new CertificateSharingDetailsQueryResultItem
                {
                    SharingId = s.SharingId,
                    SharingNumber = s.SharingNumber,
                    CreatedAt = s.CreatedAt,
                    LinkCode = s.LinkCode,
                    ExpiryTime = s.ExpiryTime,
                    SharingAccess = s.SharingAccess,
                    SharingEmails = s.SharingEmails?.Select(e => new SharingEmailQueryResultItem
                    {
                        SharingEmailId = e.SharingEmailId,
                        EmailAddress = e.EmailAddress,
                        EmailLinkCode = e.EmailLinkCode,
                        SentTime = e.SentTime,
                        SharingEmailAccess = e.SharingEmailAccess
                    }).ToList() ?? new List<SharingEmailQueryResultItem>()
                }).ToList() ?? new List<CertificateSharingDetailsQueryResultItem>()
            };
        }
    }

    public class CertificateSharingDetailsQueryResultItem
    {
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LinkCode { get; set; }
        public DateTime ExpiryTime { get; set; }
        public List<DateTime> SharingAccess { get; set; } = new List<DateTime>();
        public List<SharingEmailQueryResultItem> SharingEmails { get; set; } = new List<SharingEmailQueryResultItem>();
    }

    public class SharingEmailQueryResultItem
    {
        public Guid SharingEmailId { get; set; }
        public required string EmailAddress { get; set; }
        public Guid EmailLinkCode { get; set; }
        public DateTime SentTime { get; set; }
        public List<DateTime> SharingEmailAccess { get; set; } = new List<DateTime>();
    }
}