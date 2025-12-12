using System;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class Certificate
    {
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public required string CourseName { get; set; }
        public required string CourseLevel { get; set; }
        public DateTime DateAwarded { get; set; }

        public static implicit operator Certificate?(Infrastructure.Api.Types.Certificate? source)
        {
            if (source == null)
            {
                return null;
            }

            return new Certificate
            {
                CertificateId = source.CertificateId,
                CertificateType = Enum.Parse<CertificateType>(source.CertificateType),
                CourseName = source.CourseName,
                CourseLevel = source.CourseLevel,
                DateAwarded = source.DateAwarded
            };
        }
    }
}
