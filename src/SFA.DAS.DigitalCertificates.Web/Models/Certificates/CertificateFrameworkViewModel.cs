using System;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class CertificateFrameworkViewModel
    {
        public Guid CertificateId { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenNames { get; set; }
        public long? Uln { get; set; }
        public CertificateType CertificateType { get; set; }
        public string? FrameworkCertificateNumber { get; set; }
        public string? CourseName { get; set; }
        public string? CourseOption { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? DateAwarded { get; set; }
        public string? ProviderName { get; set; }
        public string? EmployerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? PrintRequestedAt { get; set; }
        public string? PrintRequestedBy { get; set; }
        public bool ShowBackLink { get; set; } = true;
        public List<string>? QualificationsAndAwardingBodies { get; set; }
        public List<DeliveryInformationResponse>? DeliveryInformation { get; set; }
        public Enums.PrintStatus PrintStatus { get; set; } = Enums.PrintStatus.None;
        public DateTime? PrintStatusDate { get; set; }
        public string? PrintStatusMessage { get; set; }
        public string? PrintStatusDisplay { get; set; }
        public string? PrintStatusCssClass { get; set; }
        public bool ShowPrintHeader { get; set; }
    }
}
