using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;

namespace SFA.DAS.DigitalCertificates.Infrastructure.UnitTests.Services.SessionStorage
{
    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private SessionStorageService _sut = null!;
        private Mock<ISession> _sessionMock = null!;
        private Dictionary<string, byte[]> _storage = null!;

        [SetUp]
        public void SetUp()
        {
            _storage = new Dictionary<string, byte[]>();

            _sessionMock = new Mock<ISession>();

            _sessionMock.SetupGet(s => s.IsAvailable).Returns(true);
            _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k, v) => _storage[k] = v);
            _sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback<string>(k => _storage.Remove(k));
            _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    if (_storage.TryGetValue(key, out var bytes))
                    {
                        value = bytes;
                        return true;
                    }

                    value = null!;
                    return false;
                });

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.Session).Returns(_sessionMock.Object);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

            _sut = new SessionStorageService(_httpContextAccessorMock.Object);
        }

        [Test]
        public async Task SetAsync_Then_GetAsync_ReturnsValue()
        {
            // Arrange
            var key = "test:key";
            var value = "hello";

            // Act
            await _sut.SetAsync(key, value);
            var result = await _sut.GetAsync(key);

            // Assert
            result.Should().Be(value);
        }

        [Test]
        public async Task ClearAsync_RemovesValue()
        {
            // Arrange
            var key = "test:key2";
            var value = "world";

            await _sut.SetAsync(key, value);

            // Act
            await _sut.ClearAsync(key);
            var result = await _sut.GetAsync(key);

            // Assert
            result.Should().BeNull();
        }
    }
}
