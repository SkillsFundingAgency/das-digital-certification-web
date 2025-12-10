using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing
{
    public class CreateSharingCommandResult
    {
        public Guid Userid { get; set; }
        public Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public required string CourseName { get; set; }
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LinkCode { get; set; }
        public DateTime ExpiryTime { get; set; }

        public static implicit operator CreateSharingCommandResult?(CreateSharingResponse? source)
        {
            if (source == null)
            {
                return null;
            }

            return new CreateSharingCommandResult
            {
                Userid = source.Userid,
                CertificateId = source.CertificateId,
                CertificateType = source.CertificateType,
                CourseName = source.CourseName,
                SharingId = source.SharingId,
                SharingNumber = source.SharingNumber,
                CreatedAt = source.CreatedAt,
                LinkCode = source.LinkCode,
                ExpiryTime = source.ExpiryTime
            };
        }
    }
}