using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests
{
    public class CreateUserActionRequest
    {
        public required string ActionType { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }
        public Guid? CertificateId { get; set; }
        public string? CertificateType { get; set; }
        public string? CourseName { get; set; }
    }
}
