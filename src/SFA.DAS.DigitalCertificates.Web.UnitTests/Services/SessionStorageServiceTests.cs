using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<ICacheStorageService> _cacheStorageMock;
        private Mock<IMediator> _mediatorMock;
        private SessionStorageService _sut;

        [SetUp]
        public void Setup()
        {
            _cacheStorageMock = new Mock<ICacheStorageService>();
            _mediatorMock = new Mock<IMediator>();

            _sut = new SessionStorageService(_cacheStorageMock.Object, _mediatorMock.Object);
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
                    SessionStorageService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUser);

            var result = await _sut.GetUserAsync(expectedUser.GovUkIdentifier);

            result.Should().Be(expectedUser);

            _cacheStorageMock.Verify(x =>
                x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
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
                    SessionStorageService.GetScopedKey(nameof(User), expectedUser.GovUkIdentifier),
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
        public async Task GetOwnedCertificatesAsync_Returns_Certificates_From_GetCertificatesAsync()
        {
            var govUkIdentifier = "gov-789";

            var expectedCertificates = new List<Certificate>
            {
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "Bricklayer", CourseLevel = "1" }
            };

            var response = new GetCertificatesQueryResult
            {
                Certificates = expectedCertificates
            };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(CertificatesResponse), govUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _sut.GetOwnedCertificatesAsync(govUkIdentifier);

            result.Should().BeEquivalentTo(expectedCertificates);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_Returns_Authorisation_From_GetCertificatesAsync()
        {
            var govUkIdentifier = "gov-777";

            var expectedAuthorisation = new UlnAuthorisation
            {
                Uln = "1234567890",
                AuthorisationId = Guid.NewGuid(),
                AuthorisedAt = DateTime.Now
            };

            var response = new GetCertificatesQueryResult
            {
                Authorisation = expectedAuthorisation
            };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(CertificatesResponse), govUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _sut.GetUlnAuthorisationAsync(govUkIdentifier);

            result.Should().Be(expectedAuthorisation);
        }

        [Test]
        public async Task GetCertificatesAsync_When_User_Exists_Calls_Mediator()
        {
            var govUkIdentifier = "gov-111";
            var user = new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = govUkIdentifier,
                EmailAddress = "name@domain.com"
            };

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(User), govUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>> capturedDelegate = null;

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(CertificatesResponse), govUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>>, CancellationToken>(
                    (key, fn, token) => capturedDelegate = fn)
                .Returns((string key,
                          Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>> fn,
                          CancellationToken _) =>
                {
                    // simulate cache miss by explicitly invoking the delegate
                    var options = new DistributedCacheEntryOptions();
                    return fn(options);
                });

            var expectedResponse = new GetCertificatesQueryResult();

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            await _sut.GetOwnedCertificatesAsync(govUkIdentifier);

            // Assert
            _mediatorMock.Verify(
                x => x.Send(It.Is<GetCertificatesQuery>(q => q.UserId == user.Id),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetCertificatesAsync_When_User_Does_Not_Exist_Returns_Null()
        {
            var govUkIdentifier = "gov-222";

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(User), govUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<User>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            _cacheStorageMock
                .Setup(x => x.GetOrCreateAsync(
                    SessionStorageService.GetScopedKey(nameof(CertificatesResponse), govUkIdentifier),
                    It.IsAny<Func<DistributedCacheEntryOptions, Task<GetCertificatesQueryResult>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCertificatesQueryResult)null);

            var result = await _sut.GetOwnedCertificatesAsync(govUkIdentifier);

            result.Should().BeNull();

            _mediatorMock.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public void GetScopedKey_Returns_Correct_Format()
        {
            var key = SessionStorageService.GetScopedKey("User", "gov-123");
            key.Should().Be("DigitalCertificates:User:gov-123");
        }
    }
}
