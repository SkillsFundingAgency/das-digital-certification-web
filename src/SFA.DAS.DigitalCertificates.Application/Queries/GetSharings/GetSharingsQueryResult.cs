using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharings
{
    public class GetSharingsQueryResult
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public required string CourseName { get; set; }
        public List<SharingDetailsQueryResultItem> Sharings { get; set; } = new List<SharingDetailsQueryResultItem>();

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
                CertificateType = Enum.Parse<CertificateType>(source.CertificateType),
                CourseName = source.CourseName,
                Sharings = source.Sharings?.Select(s => new SharingDetailsQueryResultItem
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
                }).ToList() ?? new List<SharingDetailsQueryResultItem>()
            };
        }
    }

    public class SharingDetailsQueryResultItem
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