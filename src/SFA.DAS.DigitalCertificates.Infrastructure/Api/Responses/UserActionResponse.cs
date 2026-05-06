using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class UserActionResponse
    {
        public required long Id { get; set; }
        public required Guid UserId { get; set; }
        public required string ActionType { get; set; }
        public required DateTime ActionTime { get; set; }
        public required string ActionStatus { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }
        public Guid? CertificateId { get; set; }
        public string? CertificateType { get; set; }
        public string? CourseName { get; set; }
        public required string ActionCode { get; set; }
        public List<AdminActionResponse>? AdminActions { get; set; }
        public bool Actioned { get; set; }
    }

    public class AdminActionResponse
    {
        public required string Username { get; set; }
        public required DateTime ActionTime { get; set; }
        public required string Action { get; set; }
    }
}
