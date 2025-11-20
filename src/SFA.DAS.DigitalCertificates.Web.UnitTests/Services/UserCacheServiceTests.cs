using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class UserCacheServiceTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private Mock<ICacheStorageService> _cacheStorageMock;
        private UserCacheService _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _cacheStorageMock = new Mock<ICacheStorageService>();
            _sut = new UserCacheService(_outerApiMock.Object, _cacheStorageMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task CacheUserForGovUkIdentifier_Calls_CacheStorage_With_Correct_Key_And_Returns_User()
        {
            // Arrange
            var govUkIdentifier = "gov-123";
            var expectedUser = new User { Id = Guid.NewGuid(), GovUkIdentifier = govUkIdentifier };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    $"User:{govUkIdentifier}",
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _sut.CacheUserForGovUkIdentifier(govUkIdentifier);

            // Assert
            result.Should().Be(expectedUser);

            _cacheStorageMock.Verify(x =>
                x.GetOrCreateAsync(
                    $"User:{govUkIdentifier}",
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _outerApiMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task CacheUserForGovUkIdentifier_Delegate_Calls_OuterApi_And_Sets_60s_Expiry()
        {
            // Arrange
            var govUkIdentifier = "gov-456";
            var expectedUser = new User { Id = Guid.NewGuid(), GovUkIdentifier = govUkIdentifier };
            Func<DistributedCacheEntryOptions, Task<User>> capturedDelegate = null;

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    $"User:{govUkIdentifier}",
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<User>>, CancellationToken>(
                    (key, func, token) => capturedDelegate = func)
                .ReturnsAsync(expectedUser);

            _outerApiMock
                .Setup(x => x.GetUser(govUkIdentifier))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _sut.CacheUserForGovUkIdentifier(govUkIdentifier);

            // Assert
            result.Should().Be(expectedUser);
            capturedDelegate.Should().NotBeNull();

            // Simulate invoking the delegate (cache miss)
            var options = new DistributedCacheEntryOptions();
            var userFromDelegate = await capturedDelegate(options);

            userFromDelegate.Should().Be(expectedUser);
            options.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(60));

            _outerApiMock.Verify(x => x.GetUser(govUkIdentifier), Times.Once);
        }

        [Test]
        public void CacheUserForGovUkIdentifier_Throws_If_CacheStorage_Fails()
        {
            // Arrange
            var govUkIdentifier = "gov-error";

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Cache failure"));

            // Act
            Func<Task> act = async () => await _sut.CacheUserForGovUkIdentifier(govUkIdentifier);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("Cache failure");
        }
    }
}
