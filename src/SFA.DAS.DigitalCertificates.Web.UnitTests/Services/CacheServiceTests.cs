using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetMatches;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services;
using Match = SFA.DAS.DigitalCertificates.Domain.Models.Match;

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

            var configuration = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = string.Empty,
                RedisConnectionString = string.Empty,
                DataProtectionKeysDatabase = string.Empty
            };
            _sut = new CacheService(_cacheStorageMock.Object, _mediatorMock.Object, configuration);
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

        [Test]
        public async Task GetOrCreateMatchesAsync_Calls_CacheStorage_With_Correct_Key_And_Returns_Result()
        {
            // Arrange
            var gov = "gov-111";
            var userId = Guid.NewGuid();

            var expected = new MatchesAndMasks
            {
                Matches = { new Match { Uln = "111" } },
                Masks = { new Mask { CourseCode = "C1" } }
            };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    CacheService.GetScopedKey("Matches", gov),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut.GetOrCreateMatchesAsync(gov, userId);

            // Assert
            result.Should().Be(expected);
            _cacheStorageMock.Verify(x => x.GetOrCreateAsync(
                CacheService.GetScopedKey("Matches", gov),
                It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetOrCreateMatchesAsync_Delegate_Calls_Mediator_And_Sets_Expiry()
        {
            // Arrange
            var configuration = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = string.Empty,
                RedisConnectionString = string.Empty,
                DataProtectionKeysDatabase = string.Empty,
                MatchesCacheExpiryDays = 7
            };

            var localSut = new CacheService(_cacheStorageMock.Object, _mediatorMock.Object, configuration);

            var gov = "gov-222";
            var userId = Guid.NewGuid();

            var expected = new MatchesAndMasks
            {
                Matches = { new Match { Uln = "222" } }
            };

            Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>> capturedDelegate = null!;

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    CacheService.GetScopedKey("Matches", gov),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>, CancellationToken>((k, func, t) => capturedDelegate = func)
                .ReturnsAsync(expected);

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetMatchesQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var result = await localSut.GetOrCreateMatchesAsync(gov, userId);

            // Assert
            result.Should().Be(expected);
            capturedDelegate.Should().NotBeNull();

            var options = new DistributedCacheEntryOptions();
            var fromDelegate = await capturedDelegate(options);
            fromDelegate.Should().Be(expected);
            options.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromDays(configuration.MatchesCacheExpiryDays.Value));
        }

        [Test]
        public async Task ClearMatches_Removes_Matches_And_FailCount_Keys()
        {
            // Arrange
            var gov = "gov-333";

            // Act
            await _sut.ClearMatches(gov);

            // Assert
            _cacheStorageMock.Verify(x => x.RemoveAsync(CacheService.GetScopedKey("Matches", gov)), Times.Once);
            _cacheStorageMock.Verify(x => x.RemoveAsync(CacheService.GetScopedKey("MatchFailCount", gov)), Times.Once);
        }

        [Test]
        public async Task IncrementMatchFailCountAsync_Increments_And_Sets_Expiry()
        {
            // Arrange
            var configuration = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = string.Empty,
                RedisConnectionString = string.Empty,
                DataProtectionKeysDatabase = string.Empty,
                MatchesCacheExpiryDays = 5
            };

            var localSut = new CacheService(_cacheStorageMock.Object, _mediatorMock.Object, configuration);

            var gov = "gov-444";
            var key = CacheService.GetScopedKey("MatchFailCount", gov);

            _cacheStorageMock
                .Setup(x => x.GetAsync<int?>(key))
                .ReturnsAsync((int?)null);

            Func<DistributedCacheEntryOptions, Task<int>> capturedDelegate = null!;

            _cacheStorageMock
                .Setup(x => x.SetAsync<int>(key, It.IsAny<Func<DistributedCacheEntryOptions, Task<int>>>(), It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<int>>, CancellationToken>((k, func, t) => capturedDelegate = func)
                .ReturnsAsync(1);

            // Act
            var updated = await localSut.IncrementMatchFailCountAsync(gov);

            // Assert
            updated.Should().Be(1);
            capturedDelegate.Should().NotBeNull();

            var options = new DistributedCacheEntryOptions();
            var setValue = await capturedDelegate(options);
            setValue.Should().Be(1);
            options.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromDays(configuration.MatchesCacheExpiryDays.Value));
        }
    }
}
