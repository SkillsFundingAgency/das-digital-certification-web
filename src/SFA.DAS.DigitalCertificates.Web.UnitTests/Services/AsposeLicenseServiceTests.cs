using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    public class AsposeLicenseServiceTests
    {
        [Test]
        public async Task GetAsposeLicense_WhenBlobThrows_LogsError()
        {           
            var blobMock = new Mock<IBlobService>();
            var config = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                OneLoginSettingsUrl = "http://settings.com",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                FrameworkTemplateBlobName = "framework-template",
                MasterPassword = "master-password",
                LicenseBlobName = "license.xml",
                StorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "test-container",
                AsposeLicenseContainerName = "aspose-license-container"
            };

            var loggerMock = new Mock<ILogger<AsposeLicenseService>>();

            var exception = new InvalidOperationException("blob failed");
            blobMock
                .Setup(b => b.OpenBlobReadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(exception);
            var licenseWrapperMock = new Mock<IAsposeLicenseWrapper>();

            var service = new AsposeLicenseService(blobMock.Object, config, loggerMock.Object, licenseWrapperMock.Object);
                  
            Func<Task> act = async () => await service.GetAsposeLicense();
        
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("An error occurred while retrieving the Aspose license.");

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while retrieving the Aspose license.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GetAsposeLicense_WhenBlobReturnsStream_SetsLicense()
        {            
            var blobMock = new Mock<IBlobService>();
            var config = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                OneLoginSettingsUrl = "http://settings.com",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                FrameworkTemplateBlobName = "framework-template",
                MasterPassword = "master-password",
                LicenseBlobName = "license.xml",
                StorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "test-container",
                AsposeLicenseContainerName = "aspose-license-container"
            };
            var loggerMock = new Mock<ILogger<AsposeLicenseService>>();

            var licenseWrapperMock = new Mock<IAsposeLicenseWrapper>();

            var sampleStream = new MemoryStream(new byte[] { 1, 2, 3 });
            blobMock
                .Setup(b => b.OpenBlobReadAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(sampleStream);

            var service = new AsposeLicenseService(blobMock.Object, config, loggerMock.Object, licenseWrapperMock.Object);
            
            await service.GetAsposeLicense();

            blobMock.Verify(b => b.OpenBlobReadAsync("aspose-license-container", "license.xml"), Times.Once);

            licenseWrapperMock.Verify(x => x.SetLicense(sampleStream), Times.Once);            
        }
    }
}
