using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Types
{
    public class Certificate
    {
        public Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public required string CourseName { get; set; }
        public required string CourseLevel { get; set; }
        public DateTime DateAwarded { get; set; }
    }
}
