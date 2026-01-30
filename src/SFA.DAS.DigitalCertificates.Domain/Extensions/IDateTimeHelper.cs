using System;

namespace SFA.DAS.DigitalCertificates.Domain.Extensions
{
    public interface IDateTimeHelper
    {
        DateTime Now { get; }
    }

    public class UtcTimeProvider : IDateTimeHelper
    {
        public DateTime Now => DateTime.UtcNow;
    }
}