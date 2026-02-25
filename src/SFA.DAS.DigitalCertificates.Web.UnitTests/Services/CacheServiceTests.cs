using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class CacheServiceTests
    {
        private Mock<ICacheStorageService> _cacheStorageMock;
        private Mock<IMediator> _mediatorMock;
        private CacheService _sut;

        [SetUp]
        public void Setup()
        {
            _cacheStorageMock = new Mock<ICacheStorageService>();
            _mediatorMock = new Mock<IMediator>();

            _sut = new CacheService(_cacheStorageMock.Object, _mediatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task GetUserAsync_Calls_CacheStorage_With_Correct_Key_And_Returns_User()
        {
            var expectedUser = new User { Id = Guid.NewGuid(), GovUkIdentifier = "gov-123", EmailAddress = "name@domain.com" };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    CacheService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            var result = await _sut.GetUserAsync(expectedUser.GovUkIdentifier);

            result.Should().Be(expectedUser);

            _cacheStorageMock.Verify(x =>
                x.GetOrCreateAsync(
                    CacheService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetUserAsync_Delegate_Calls_Mediator_And_Sets_60s_Expiry()
        {
            var expectedUser = new User { Id = Guid.NewGuid(), GovUkIdentifier = "gov-456", EmailAddress = "name@domain.com" };
            Func<DistributedCacheEntryOptions, Task<User>> capturedDelegate = null;

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    CacheService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<User>>, CancellationToken>(
                    (key, func, token) => capturedDelegate = func)
                .ReturnsAsync(expectedUser);

            _mediatorMock
                .Setup(x => x.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == expectedUser.GovUkIdentifier), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            var result = await _sut.GetUserAsync(expectedUser.GovUkIdentifier);

            result.Should().Be(expectedUser);
            capturedDelegate.Should().NotBeNull();

            var options = new DistributedCacheEntryOptions();
            var userFromDelegate = await capturedDelegate(options);

            userFromDelegate.Should().Be(expectedUser);
            options.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(60));
        }

        [Test]
        public void GetUserAsync_Throws_If_CacheStorage_Fails()
        {
            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Cache failure"));

            Func<Task> act = async () => await _sut.GetUserAsync("gov-error");

            act.Should().ThrowAsync<Exception>().WithMessage("Cache failure");
        }

        [Test]
        public async Task Clear_Removes_User_Cache_Entry()
        {
            var govUkIdentifier = "gov-999";

            var expectedUserKey = CacheService.GetScopedKey(nameof(User), govUkIdentifier);

            // Act
            await _sut.Clear(govUkIdentifier);

            // Assert
            _cacheStorageMock.Verify(x => x.RemoveAsync(expectedUserKey), Times.Once);
        }

        [Test]
        public void GetScopedKey_Returns_Correct_Format()
        {
            var key = CacheService.GetScopedKey("User", "gov-123");
            key.Should().Be("DigitalCertificates:User:gov-123");
        }
    }
}
