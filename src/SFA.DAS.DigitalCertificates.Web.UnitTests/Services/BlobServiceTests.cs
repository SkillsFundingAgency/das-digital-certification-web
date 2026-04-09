using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class BlobServiceTests
    {
        private Mock<BlobServiceClient> _blobServiceClient;
        private Mock<BlobContainerClient> _blobContainerClient;
        private Mock<BlobClient> _blobClient;
        private Mock<ILogger<BlobService>> _logger;
        private BlobService _sut;

        [SetUp]
        public void SetUp()
        {
            _blobServiceClient = new Mock<BlobServiceClient>();
            _blobContainerClient = new Mock<BlobContainerClient>();
            _blobClient = new Mock<BlobClient>();
            _logger = new Mock<ILogger<BlobService>>();

            _blobServiceClient
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_blobContainerClient.Object);

            _blobContainerClient
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClient.Object);

            _sut = new BlobService(_blobServiceClient.Object, _logger.Object);
        }

        [Test]
        public async Task GetBlobBytesAsync_ReturnsBytes_WhenBlobExists()
        {
            // Arrange
            var blobContainerName = "aspose-license";
            var blobName = "test.txt";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes("hello world");
            var stream = new MemoryStream(contentBytes);

            _blobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            var mockDownloadResult = Mock.Of<BlobDownloadStreamingResult>(d => d.Content == stream);

            _blobClient
                .Setup(x => x.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobDownloadStreamingResult>>(r => r.Value == mockDownloadResult));

            // Act
            var result = await _sut.GetBlobBytesAsync(blobContainerName, blobName);

            // Assert
            result.Should().NotBeNull();
            result.Should().Equal(contentBytes);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public async Task OpenBlobReadAsync_ThrowsArgumentException_WhenBlobNameIsNullOrWhitespace(string blobName)
        {
            // Act
            Func<Task> act = () => _sut.OpenBlobReadAsync("aspose-license", blobName);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task OpenBlobReadAsync_ThrowsInvalidOperationException_WhenBlobDoesNotExist()
        {
            // Arrange
            var blobContainerName = "aspose-license";
            var blobName = "missing.txt";

            _blobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == false));

            // Act
            Func<Task> act = () => _sut.OpenBlobReadAsync(blobContainerName, blobName);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task OpenBlobReadAsync_ReturnsStream_WhenBlobExists()
        {
            // Arrange
            var containerName = "aspose-license";
            var blobName = "file.bin";
            var contentBytes = new byte[] { 1, 2, 3, 4 };
            var stream = new MemoryStream(contentBytes);

            _blobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            var mockDownloadResult = Mock.Of<BlobDownloadStreamingResult>(d => d.Content == stream);

            _blobClient
                .Setup(x => x.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobDownloadStreamingResult>>(r => r.Value == mockDownloadResult));

            // Act
            await using var resultStream = await _sut.OpenBlobReadAsync(containerName, blobName);

            // Assert
            resultStream.Should().NotBeNull();

            using var ms = new MemoryStream();
            await resultStream.CopyToAsync(ms);

            ms.ToArray().Should().Equal(contentBytes);
        }

        [Test]
        public async Task OpenBlobReadAsync_ThrowsRequestFailedException_WhenDownloadStreamingThrows()
        {
            // Arrange
            var blobContainerName = "aspose-license";
            var blobName = "file.bin";

            _blobClient
                .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<bool>>(r => r.Value == true));

            _blobClient
                .Setup(x => x.DownloadStreamingAsync(It.IsAny<BlobDownloadOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("download failed"));

            // Act
            Func<Task> act = () => _sut.OpenBlobReadAsync(blobContainerName, blobName);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

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