using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    public class AsposeLicenseServiceTests
    {
        [Test]
        public async Task GetAsposeLicense_WhenBlobThrows_LogsError()
        {
            // Arrange
            var blobMock = new Mock<IBlobService>();
            var config = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                MasterPassword = "master-password",
                LicenseBlobName = "license.xml"

            };

            var loggerMock = new Mock<ILogger<AsposeLicenseService>>();

            var exception = new InvalidOperationException("blob failed");
            blobMock
                .Setup(b => b.OpenBlobReadAsync(It.IsAny<string>()))
                .ThrowsAsync(exception);
            var licenseWrapperMock = new Mock<IAsposeLicenseWrapper>();

            var service = new AsposeLicenseService(blobMock.Object, config, loggerMock.Object, licenseWrapperMock.Object);

            // Act
            await service.GetAsposeLicense();

            // Assert
            blobMock.Verify(b => b.OpenBlobReadAsync("license.xml"), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("error occured retrieving the aspose license")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetAsposeLicense_WhenBlobReturnsStream_LogsInformation()
        {
            // Arrange
            var blobMock = new Mock<IBlobService>();
            var config = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                MasterPassword = "master-password",
                LicenseBlobName = "license.xml"
            };
            var loggerMock = new Mock<ILogger<AsposeLicenseService>>();

            var licenseWrapperMock = new Mock<IAsposeLicenseWrapper>();

            var sampleStream = new MemoryStream(new byte[] { 1, 2, 3 });
            blobMock
                .Setup(b => b.OpenBlobReadAsync(It.IsAny<string>()))
                .ReturnsAsync(sampleStream);

            var service = new AsposeLicenseService(blobMock.Object, config, loggerMock.Object, licenseWrapperMock.Object);

            // Act
            await service.GetAsposeLicense();

            // Assert
            blobMock.Verify(b => b.OpenBlobReadAsync("license.xml"), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Aspose license loaded successfully.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
