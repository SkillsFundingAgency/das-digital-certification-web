using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Constants
{
    [ExcludeFromCodeCoverage]
    public static class DeliveryInformationStatuses
    {
        public const string Submitted = "Submitted";
        public const string PrintRequested = "PrintRequested";
        public const string Reprint = "Reprint";
        public const string SentToPrinter = "SentToPrinter";
        public const string Printed = "Printed";
        public const string Delivered = "Delivered";
    }
}
