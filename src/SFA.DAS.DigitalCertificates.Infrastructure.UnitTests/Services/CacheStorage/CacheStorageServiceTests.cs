using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Infrastructure.UnitTests.Services.CacheStorage
{
    [TestFixture]
    public class CacheStorageServiceTests
    {
        private Mock<IDistributedCache> _distributedCacheMock;
        private CacheStorageService _cacheStorageService;

        [SetUp]
        public void Setup()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _cacheStorageService = new CacheStorageService(_distributedCacheMock.Object);
        }

        [Test]
        public async Task RemoveAsync_ShouldRemoveItemFromCache()
        {
            // Arrange
            var key = "testKey";

            // Act
            await _cacheStorageService.RemoveAsync(key);

            // Assert
            _distributedCacheMock.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}