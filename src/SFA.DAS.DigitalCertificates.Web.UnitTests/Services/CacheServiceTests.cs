using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.UpdateUserIdentity;
using SFA.DAS.DigitalCertificates.Application.Queries.GetMatches;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Models;
using Match = SFA.DAS.DigitalCertificates.Domain.Models.Match;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class CacheServiceTests
    {
        private Mock<ICacheStorageService> _cacheStorageMock = null!;
        private Mock<IMediator> _mediatorMock = null!;

        private CacheService _sut = null!;

        [SetUp]
        public void Setup()
        {
            _cacheStorageMock = new Mock<ICacheStorageService>();
            _mediatorMock = new Mock<IMediator>();

            _sut = new CacheService(
                _cacheStorageMock.Object,
                _mediatorMock.Object,
                CreateConfiguration());
        }

        [Test]
        public async Task GetUserAsync_Calls_CacheStorage_With_Correct_Key_And_Returns_User()
        {
            // Arrange
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = "gov-123",
                EmailAddress = "name@domain.com"
            };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    CacheService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _sut.GetUserAsync(expectedUser.GovUkIdentifier);

            // Assert
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
            // Arrange
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = "gov-456",
                EmailAddress = "name@domain.com"
            };

            Func<DistributedCacheEntryOptions, Task<User>> capturedDelegate = null!;

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    CacheService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<User>>, CancellationToken>(
                    (_, func, _) => capturedDelegate = func)
                .ReturnsAsync(expectedUser);

            _mediatorMock
                .Setup(x => x.Send(
                    It.Is<GetUserQuery>(q => q.GovUkIdentifier == expectedUser.GovUkIdentifier),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _sut.GetUserAsync(expectedUser.GovUkIdentifier);

            // Assert
            result.Should().Be(expectedUser);
            capturedDelegate.Should().NotBeNull();

            var options = new DistributedCacheEntryOptions();
            var userFromDelegate = await capturedDelegate(options);

            userFromDelegate.Should().Be(expectedUser);
            options.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(60));
        }

        [Test]
        public async Task GetUserAsync_Throws_If_CacheStorage_Fails()
        {
            // Arrange
            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Cache failure"));

            // Act
            Func<Task> act = async () => await _sut.GetUserAsync("gov-error");

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Cache failure");
        }

        [Test]
        public async Task ClearUser_Removes_User_Cache_Entry()
        {
            // Arrange
            var govUkIdentifier = "gov-999";
            var expectedUserKey = CacheService.GetScopedKey(nameof(User), govUkIdentifier);

            // Act
            await _sut.ClearUser(govUkIdentifier);

            // Assert
            _cacheStorageMock.Verify(x => x.RemoveAsync(expectedUserKey), Times.Once);
        }

        [Test]
        public void GetScopedKey_Returns_Correct_Format()
        {
            // Arrange
            var identifier = "gov-123";

            // Act
            var key = CacheService.GetScopedKey("User", identifier);

            // Assert
            key.Should().Be("DigitalCertificates:User:gov-123");
        }

        [Test]
        public async Task GetMatchesAsync_WhenCachedMatchesExist_ReturnsCachedResult()
        {
            // Arrange
            var gov = "gov-111";
            var userIdentityId = Guid.NewGuid();
            var key = CacheService.GetScopedKey("Matches", gov);

            var expected = new MatchesAndMasks
            {
                Matches =
                {
                    new Match
                    {
                        Uln = 111L,
                        UserIdentityId = userIdentityId
                    }
                },
                Masks =
                {
                    new Mask
                    {
                        CourseCode = "C1"
                    }
                }
            };

            _cacheStorageMock
                .Setup(x => x.GetAsync<MatchesAndMasks>(key))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut.GetMatchesAsync(gov);

            // Assert
            result.Should().Be(expected);

            _cacheStorageMock.Verify(x => x.GetAsync<MatchesAndMasks>(key), Times.Once);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<UpdateUserIdentityCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<GetMatchesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _cacheStorageMock.Verify(x =>
                x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _cacheStorageMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetMatchesAsync_WhenNoCachedMatches_ReturnsNull()
        {
            // Arrange
            var gov = "gov-no-cache";
            var key = CacheService.GetScopedKey("Matches", gov);

            _cacheStorageMock
                .Setup(x => x.GetAsync<MatchesAndMasks>(key))
                .ReturnsAsync((MatchesAndMasks)null);

            // Act
            var result = await _sut.GetMatchesAsync(gov);

            // Assert
            result.Should().BeNull();

            _cacheStorageMock.Verify(x => x.GetAsync<MatchesAndMasks>(key), Times.Once);
            _cacheStorageMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<UpdateUserIdentityCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<GetMatchesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetMatchesAsync_WhenCachedValueHasNoMatches_RemovesCachedValue_AndReturnsNull()
        {
            // Arrange
            var gov = "gov-empty-cache";
            var key = CacheService.GetScopedKey("Matches", gov);

            var cached = new MatchesAndMasks
            {
                Masks =
                {
                    new Mask
                    {
                        CourseCode = "MASK1"
                    }
                }
            };

            _cacheStorageMock
                .Setup(x => x.GetAsync<MatchesAndMasks>(key))
                .ReturnsAsync(cached);

            // Act
            var result = await _sut.GetMatchesAsync(gov);

            // Assert
            result.Should().BeNull();

            _cacheStorageMock.Verify(x => x.RemoveAsync(key), Times.Once);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<UpdateUserIdentityCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<GetMatchesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetMatchesAsync_WhenCachedMatchesHaveNullUserIdentityId_RemovesCachedValue_AndReturnsNull()
        {
            // Arrange
            var gov = "gov-cache-with-old-matches";
            var key = CacheService.GetScopedKey("Matches", gov);

            var cached = new MatchesAndMasks
            {
                Matches =
                {
                    new Match
                    {
                        Uln = 111L,
                        UserIdentityId = null
                    }
                }
            };

            _cacheStorageMock
                .Setup(x => x.GetAsync<MatchesAndMasks>(key))
                .ReturnsAsync(cached);

            // Act
            var result = await _sut.GetMatchesAsync(gov);

            // Assert
            result.Should().BeNull();

            _cacheStorageMock.Verify(x => x.RemoveAsync(key), Times.Once);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<UpdateUserIdentityCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mediatorMock.Verify(x =>
                x.Send(It.IsAny<GetMatchesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CreateMatchesAsync_UpdatesUserIdentity_GetsMatches_AndSetsExpiry()
        {
            // Arrange
            var configuration = CreateConfiguration();
            configuration.MatchesCacheExpiryDays = 7;

            var localSut = new CacheService(
                _cacheStorageMock.Object,
                _mediatorMock.Object,
                configuration);

            var gov = "gov-222";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();
            var key = CacheService.GetScopedKey("Matches", gov);
            var govUkCredentialSubject = CreateGovUkCredentialSubject();

            var expected = new MatchesAndMasks
            {
                Matches =
                {
                    new Match
                    {
                        Uln = 222,
                        UserIdentityId = userIdentityId
                    }
                }
            };

            Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>> capturedDelegate = null!;

            _mediatorMock
                .Setup(x => x.Send(
                    It.IsAny<UpdateUserIdentityCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            _mediatorMock
                .Setup(x => x.Send(
                    It.Is<GetMatchesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            _cacheStorageMock
                .Setup(x => x.SetAsync(
                    key,
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>, CancellationToken>(
                    (_, func, _) => capturedDelegate = func)
                .ReturnsAsync(expected);

            // Act
            var result = await localSut.CreateMatchesAsync(gov, userId, govUkCredentialSubject);

            // Assert
            result.Should().Be(expected);

            VerifyUpdateUserIdentityCommandWasSent(userId);

            _mediatorMock.Verify(x => x.Send(
                It.Is<GetMatchesQuery>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _cacheStorageMock.Verify(x => x.SetAsync(
                key,
                It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            capturedDelegate.Should().NotBeNull();

            var options = new DistributedCacheEntryOptions();
            var fromDelegate = await capturedDelegate(options);

            fromDelegate.Should().Be(expected);
            options.AbsoluteExpirationRelativeToNow.Should()
                .Be(TimeSpan.FromDays(configuration.MatchesCacheExpiryDays.Value));
        }

        [Test]
        public async Task CreateMatchesAsync_WhenMediatorReturnsNoMatches_ReturnsNull_AndDoesNotCache()
        {
            // Arrange
            var gov = "gov-no-matches";
            var userId = Guid.NewGuid();
            var govUkCredentialSubject = CreateGovUkCredentialSubject();

            var emptyResult = new MatchesAndMasks
            {
                Masks =
                {
                    new Mask
                    {
                        CourseCode = "MASK1"
                    }
                }
            };

            _mediatorMock
                .Setup(x => x.Send(
                    It.IsAny<UpdateUserIdentityCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            _mediatorMock
                .Setup(x => x.Send(
                    It.Is<GetMatchesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResult);

            // Act
            var result = await _sut.CreateMatchesAsync(gov, userId, govUkCredentialSubject);

            // Assert
            result.Should().BeNull();

            VerifyUpdateUserIdentityCommandWasSent(userId);

            _cacheStorageMock.Verify(x =>
                x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task CreateMatchesAsync_WhenMediatorReturnsNull_ReturnsNull_AndDoesNotCache()
        {
            // Arrange
            var gov = "gov-null-matches";
            var userId = Guid.NewGuid();
            var govUkCredentialSubject = CreateGovUkCredentialSubject();

            _mediatorMock
                .Setup(x => x.Send(
                    It.IsAny<UpdateUserIdentityCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            _mediatorMock
                .Setup(x => x.Send(
                    It.Is<GetMatchesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((MatchesAndMasks)null);

            // Act
            var result = await _sut.CreateMatchesAsync(gov, userId, govUkCredentialSubject);

            // Assert
            result.Should().BeNull();

            VerifyUpdateUserIdentityCommandWasSent(userId);

            _cacheStorageMock.Verify(x =>
                x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<MatchesAndMasks>>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task ClearMatches_Removes_Matches_And_FailCount_Keys()
        {
            // Arrange
            var gov = "gov-333";

            // Act
            await _sut.ClearMatches(gov);

            // Assert
            _cacheStorageMock.Verify(x =>
                x.RemoveAsync(CacheService.GetScopedKey("Matches", gov)),
                Times.Once);

            _cacheStorageMock.Verify(x =>
                x.RemoveAsync(CacheService.GetScopedKey("MatchFailCount", gov)),
                Times.Once);
        }

        [Test]
        public async Task ClearMatchFailCountAsync_Removes_FailCount_Key()
        {
            // Arrange
            var gov = "gov-clear-fail-count";

            // Act
            await _sut.ClearMatchFailCountAsync(gov);

            // Assert
            _cacheStorageMock.Verify(x =>
                x.RemoveAsync(CacheService.GetScopedKey("MatchFailCount", gov)),
                Times.Once);
        }

        [Test]
        public async Task IncrementMatchFailCountAsync_Increments_And_Sets_Expiry()
        {
            // Arrange
            var configuration = CreateConfiguration();
            configuration.MatchesCacheExpiryDays = 5;

            var localSut = new CacheService(
                _cacheStorageMock.Object,
                _mediatorMock.Object,
                configuration);

            var gov = "gov-444";
            var key = CacheService.GetScopedKey("MatchFailCount", gov);

            _cacheStorageMock
                .Setup(x => x.GetAsync<int?>(key))
                .ReturnsAsync((int?)null);

            Func<DistributedCacheEntryOptions, Task<int>> capturedDelegate = null!;

            _cacheStorageMock
                .Setup(x => x.SetAsync(
                    key,
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<int>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<int>>, CancellationToken>(
                    (_, func, _) => capturedDelegate = func)
                .ReturnsAsync(1);

            // Act
            var updated = await localSut.IncrementMatchFailCountAsync(gov);

            // Assert
            updated.Should().Be(1);
            capturedDelegate.Should().NotBeNull();

            var options = new DistributedCacheEntryOptions();
            var setValue = await capturedDelegate(options);

            setValue.Should().Be(1);
            options.AbsoluteExpirationRelativeToNow.Should()
                .Be(TimeSpan.FromDays(configuration.MatchesCacheExpiryDays.Value));
        }

        [Test]
        public async Task GetMatchFailCountAsync_Returns_Current_Value_When_Present()
        {
            // Arrange
            var gov = "gov-555";
            var key = CacheService.GetScopedKey("MatchFailCount", gov);

            _cacheStorageMock
                .Setup(x => x.GetAsync<int?>(key))
                .ReturnsAsync(3);

            // Act
            var result = await _sut.GetMatchFailCountAsync(gov);

            // Assert
            result.Should().Be(3);
        }

        [Test]
        public async Task GetMatchFailCountAsync_Returns_Zero_When_Not_Present()
        {
            // Arrange
            var gov = "gov-666";
            var key = CacheService.GetScopedKey("MatchFailCount", gov);

            _cacheStorageMock
                .Setup(x => x.GetAsync<int?>(key))
                .ReturnsAsync((int?)null);

            // Act
            var result = await _sut.GetMatchFailCountAsync(gov);

            // Assert
            result.Should().Be(0);
        }

        private void VerifyUpdateUserIdentityCommandWasSent(Guid userId)
        {
            _mediatorMock.Verify(x => x.Send(
                It.Is<UpdateUserIdentityCommand>(c =>
                    c.UserId == userId &&
                    c.DateOfBirth == new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) &&
                    c.Names.Count == 1 &&
                    c.Names[0].FamilyName == "Smith" &&
                    c.Names[0].GivenNames == "John" &&
                    c.Names[0].ValidSince == new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) &&
                    c.Names[0].ValidUntil == new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static GovUkCredentialSubject CreateGovUkCredentialSubject()
        {
            return new GovUkCredentialSubject
            {
                BirthDates = new List<GovUkBirthDateEntry>
                {
                    new GovUkBirthDateEntry
                    {
                        Value = "1990-01-01",
                        ValidUntilRaw = "2025-01-01"
                    }
                },
                Names = new List<GovUkName>
                {
                    new GovUkName
                    {
                        ValidFromRaw = "2020-01-01",
                        ValidUntilRaw = "2022-01-01",
                        NameParts = new List<GovUkNamePart>
                        {
                            new GovUkNamePart
                            {
                                Type = "GivenName",
                                Value = "John"
                            },
                            new GovUkNamePart
                            {
                                Type = "FamilyName",
                                Value = "Smith"
                            }
                        }
                    }
                }
            };
        }

        private static DigitalCertificatesWebConfiguration CreateConfiguration()
        {
            return new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = string.Empty,
                OneLoginSettingsUrl = string.Empty,
                RedisConnectionString = string.Empty,
                DataProtectionKeysDatabase = string.Empty,
                ContainerName = string.Empty,
                AsposeLicenseContainerName = string.Empty,
                StandardTemplateBlobName = string.Empty,
                GreenStandardTemplateBlobName = string.Empty,
                FrameworkTemplateBlobName = string.Empty,
                LicenseBlobName = string.Empty,
                MasterPassword = string.Empty,
                StorageConnectionString = string.Empty
            };
        }
    }
}