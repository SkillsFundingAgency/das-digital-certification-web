using System;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class DeliveryInformationResponse
    {
        public required string Id { get; set; }
        public required string Action { get; set; }
        public required string Status { get; set; }
        public required DateTime EventTime { get; set; }
    }
}
