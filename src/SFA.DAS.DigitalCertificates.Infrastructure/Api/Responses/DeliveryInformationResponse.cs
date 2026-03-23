using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class DeliveryInformationResponse
    {
        public string? Id { get; set; }
        public string? Action { get; set; }
        public string? Status { get; set; }
        public DateTime? EventTime { get; set; }
    }
}
