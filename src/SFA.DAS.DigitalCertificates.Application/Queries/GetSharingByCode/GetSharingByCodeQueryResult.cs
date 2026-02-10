using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode
{
    public class GetSharingByCodeQueryResult
    {
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public DateTime ExpiryTime { get; set; }
        public Guid? SharingId { get; set; }
        public Guid? SharingEmailId { get; set; }

        public static implicit operator GetSharingByCodeQueryResult?(GetSharingCodeResponse? source)
        {
            if (source is null)
            {
                return null;
            }

            return new GetSharingByCodeQueryResult
            {
                CertificateId = source.CertificateId,
                CertificateType = Enum.Parse<CertificateType>(source.CertificateType),
                ExpiryTime = source.ExpiryTime,
                SharingId = source.SharingId,
                SharingEmailId = source.SharingEmailId
            };
        }
    }
}
