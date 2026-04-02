using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class BlobServiceTests
    {
        private Mock<ILogger<BlobService>> _logger;
        private DigitalCertificatesWebConfiguration _config;
        private BlobService _sut;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<BlobService>>();
            _config = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                MasterPassword = "master-password",
                LicenseBlobName = "license.xml",
                BlobStorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "container"
            };
            _sut = new BlobService(_config, _logger.Object);
        }        
       
        private BlobService CreateServiceAndReplaceContainer(Mock<BlobContainerClient> mockContainer)
        {           
            var field = typeof(BlobService).GetField("_blobContainerClient", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "_blobContainerClient field not found via reflection");
            field.SetValue(_sut, mockContainer.Object);

            return _sut;
        }

        [Test]
        public async Task GetBlobBytesAsync_ReturnsBytes_WhenBlobExists()
        {
            // Arrange
            var blobName = "test.txt";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes("hello world");
            var stream = new MemoryStream(contentBytes);

            var mockBlobClient = new Mock<BlobClient>();
            mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            var mockDownloadResult = Mock.Of<BlobDownloadStreamingResult>(d => d.Content == stream);
            mockBlobClient
                .Setup(b => b.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobDownloadStreamingResult>>(r => r.Value == mockDownloadResult));

            var mockContainer = new Mock<BlobContainerClient>();
            mockContainer.Setup(c => c.GetBlobClient(blobName)).Returns(mockBlobClient.Object);

            var _sut = CreateServiceAndReplaceContainer(mockContainer);

            // Act
            var result = await _sut.GetBlobBytesAsync(blobName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(contentBytes, result);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void OpenBlobReadAsync_ThrowsArgumentException_WhenBlobNameIsNullOrWhitespace(string blobName)
        {
            var mockContainer = new Mock<BlobContainerClient>();
            var _sut = CreateServiceAndReplaceContainer(mockContainer);

            Assert.ThrowsAsync<ArgumentException>(async () => await _sut.OpenBlobReadAsync(blobName));
        }

        [Test]
        public void OpenBlobReadAsync_ThrowsFileNotFoundException_WhenBlobDoesNotExist()
        {
            // Arrange
            var blobName = "missing.txt";
            var mockBlobClient = new Mock<BlobClient>();
            mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == false));

            var mockContainer = new Mock<BlobContainerClient>();
            mockContainer.Setup(c => c.GetBlobClient(blobName)).Returns(mockBlobClient.Object);

            var _sut = CreateServiceAndReplaceContainer(mockContainer);

            // Act & Assert
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _sut.OpenBlobReadAsync(blobName));
        }

        [Test]
        public async Task OpenBlobReadAsync_ReturnsStream_WhenBlobExists()
        {
            // Arrange
            var blobName = "file.bin";
            var contentBytes = new byte[] { 1, 2, 3, 4 };
            var stream = new MemoryStream(contentBytes);

            var mockBlobClient = new Mock<BlobClient>();
            mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            var mockDownloadResult = Mock.Of<BlobDownloadStreamingResult>(d => d.Content == stream);
            mockBlobClient
                .Setup(b => b.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobDownloadStreamingResult>>(r => r.Value == mockDownloadResult));

            var mockContainer = new Mock<BlobContainerClient>();
            mockContainer.Setup(c => c.GetBlobClient(blobName)).Returns(mockBlobClient.Object);

            var _sut = CreateServiceAndReplaceContainer(mockContainer);

            // Act
            using var resultStream = await _sut.OpenBlobReadAsync(blobName);

            // Assert
            Assert.That(resultStream, Is.Not.Null);
            using var ms = new MemoryStream();
            await resultStream.CopyToAsync(ms);
            Assert.AreEqual(contentBytes, ms.ToArray());
        }

        [Test]
        public void OpenBlobReadAsync_ThrowsRequestFailedException_WhenDownloadStreamingThrows()
        {
            // Arrange
            var blobName = "file.bin";
            var mockBlobClient = new Mock<BlobClient>();
            mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            mockBlobClient
                .Setup(b => b.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("download failed"));

            var mockContainer = new Mock<BlobContainerClient>();
            mockContainer.Setup(c => c.GetBlobClient(blobName)).Returns(mockBlobClient.Object);

            var _sut = CreateServiceAndReplaceContainer(mockContainer);

            // Act & Assert
            Assert.ThrowsAsync<RequestFailedException>(async () => await _sut.OpenBlobReadAsync(blobName));
           
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get blob from azure storage.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}