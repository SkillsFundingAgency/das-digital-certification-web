using Microsoft.Extensions.Logging;
using Moq;

namespace SFA.DAS.DigitalCertificates.TestHelper.Extensions
{
    public static class LoggerExtensions
    {
        public static void VerifyLogError<T>(this Mock<ILogger<T>> loggerMock, string message, Func<Times> times)
        {
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), times);
        }
    }

}
