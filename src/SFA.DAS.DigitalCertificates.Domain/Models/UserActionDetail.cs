using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class UserActionDetail
    {
        public required long Id { get; set; }
        public required Guid UserId { get; set; }
        public required ActionType ActionType { get; set; }
        public required DateTime ActionTime { get; set; }
        public required UserActionStatus ActionStatus { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }
        public Guid? CertificateId { get; set; }
        public CertificateType? CertificateType { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public required string ActionCode { get; set; }
        public IEnumerable<AdminActionDetail> AdminActions { get; set; } = new List<AdminActionDetail>();
    }

    public class AdminActionDetail
    {
        public required string Username { get; set; }
        public required DateTime ActionTime { get; set; }
        public required AdminActionType Action { get; set; }
    }
}
