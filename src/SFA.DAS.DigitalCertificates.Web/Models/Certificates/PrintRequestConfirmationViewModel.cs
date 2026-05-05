using System;

namespace SFA.DAS.DigitalCertificates.Web.Models.Certificates
{
    public class PrintRequestConfirmationViewModel
    {
        public Guid CertificateId { get; set; }
        public required string CourseName { get; set; }
    }
}
