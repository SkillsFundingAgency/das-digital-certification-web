using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;

namespace SFA.DAS.DigitalCertificates.Infrastructure.UnitTests.Services
{
    [TestFixture]
    public class CacheStorageServiceTests
    {
        private Mock<IDistributedCache> _distributedCacheMock;
        private CacheStorageService _sut;

        [SetUp]
        public void SetUp()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _sut = new CacheStorageService(_distributedCacheMock.Object);
        }

        [Test]
        public async Task GetOrCreateAsync_Returns_Deserialised_Value_When_Cache_Hit()
        {
            // Arrange
            var key = "test-key";
            var cachedObj = new TestObject { Id = 1, Name = "Cached" };
            var json = JsonConvert.SerializeObject(cachedObj);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            _distributedCacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _sut.GetOrCreateAsync(key, _ => Task.FromResult(new TestObject { Id = 2, Name = "Factory" }));

            // Assert
            result.Should().BeEquivalentTo(cachedObj);
            _distributedCacheMock.Verify(c => c.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetOrCreateAsync_Calls_Factory_And_Caches_Result_When_Cache_Miss()
        {
            // Arrange
            var key = "test-key";
            var factoryObj = new TestObject { Id = 2, Name = "Factory" };
            byte[] storedBytes = null;
            DistributedCacheEntryOptions storedOptions = null;

            _distributedCacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            _distributedCacheMock
                .Setup(c => c.SetAsync(
                    key,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, b, o, __) =>
                {
                    storedBytes = b;
                    storedOptions = o;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetOrCreateAsync(key, _ => Task.FromResult(factoryObj));

            // Assert
            result.Should().BeEquivalentTo(factoryObj);
            var json = System.Text.Encoding.UTF8.GetString(storedBytes);
            json.Should().Contain("Factory");
            storedOptions.Should().NotBeNull();
        }

        [Test]
        public async Task GetOrCreateAsync_Does_Not_Cache_Default_Value()
        {
            // Arrange
            var key = "empty";
            _distributedCacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            // Act
            var result = await _sut.GetOrCreateAsync<TestObject>(key, _ => Task.FromResult(default(TestObject)));

            // Assert
            result.Should().BeNull();
            _distributedCacheMock.Verify(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetAsync_Returns_Deserialised_Object_When_Found()
        {
            // Arrange
            var key = "get-key";
            var expected = new TestObject { Id = 11, Name = "Existing" };
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expected));

            _distributedCacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _sut.GetAsync<TestObject>(key);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetAsync_Returns_Default_When_Not_Found()
        {
            // Arrange
            var key = "missing-key";
            _distributedCacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            // Act
            var result = await _sut.GetAsync<TestObject>(key);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_Calls_DistributedCache_RemoveAsync()
        {
            // Arrange
            var key = "remove-key";
            _distributedCacheMock
                .Setup(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RemoveAsync(key);

            // Assert
            _distributedCacheMock.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
