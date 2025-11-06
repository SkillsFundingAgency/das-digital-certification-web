using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Types
{
    public class Certificate
    {
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public string CourseName { get; set; }
        public string CourseLevel { get; set; }
        public DateTime DateAwarded { get; set; }
    }
}
