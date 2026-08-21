using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class SessionServiceTests
    {
        private const string ShareEmailKey = "DigitalCertificates:ShareEmail";
        private const string OwnedCertificatesKey = "DigitalCertificates:OwnedCertificates:";
        private const string UlnAuthorisationKey = "DigitalCertificates:UlnAuthorisation:";
        private const string AuthorisationAnswersKey = "DigitalCertificates:AuthorisationAnswers:";
        private const string RecordedSharingAccessKey = "DigitalCertificates:RecordedSharingAccessCodes";
        private const string DeliveryAddressKey = "DigitalCertificates:DeliveryAddress:";
        private const string ContactReferenceKey = "DigitalCertificates:ContactReference";

        private Mock<ISessionStorageService> _sessionStorageService = null!;
        private Mock<IMediator> _mediator = null!;
        private Mock<IUserService> _userService = null!;
        private SessionService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _sessionStorageService = new Mock<ISessionStorageService>();
            _mediator = new Mock<IMediator>();
            _userService = new Mock<IUserService>();

            _sut = new SessionService(
                _sessionStorageService.Object,
                _mediator.Object,
                _userService.Object);
        }

        [Test]
        public async Task SetShareEmailAsync_ShouldStoreEmail()
        {
            // Act
            await _sut.SetShareEmailAsync("a@b.com");

            // Assert
            _sessionStorageService.Verify(x => x.SetAsync(ShareEmailKey, "a@b.com"), Times.Once);
        }

        [Test]
        public async Task GetShareEmailAsync_ShouldReturnStoredEmail()
        {
            // Arrange
            _sessionStorageService
                .Setup(x => x.GetAsync(ShareEmailKey))
                .ReturnsAsync("x@y.com");

            // Act
            var result = await _sut.GetShareEmailAsync();

            // Assert
            result.Should().Be("x@y.com");
        }

        [Test]
        public async Task ClearShareEmailAsync_ShouldClearEmail()
        {
            // Act
            await _sut.ClearShareEmailAsync();

            // Assert
            _sessionStorageService.Verify(x => x.ClearAsync(ShareEmailKey), Times.Once);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_ShouldReturnCertificatesFromSession_WhenPresent()
        {
            // Arrange
            var expected = CreateCertificates();
            SetupSessionValue(OwnedCertificatesKey, expected);

            // Act
            var result = await _sut.GetOwnedCertificatesAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_ShouldUseKnownUserId_AndCacheCertificates()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expected = CreateCertificates();
            var response = new GetCertificatesQueryResult { Certificates = expected };
            string storedJson = null;

            _userService.Setup(x => x.GetUserId()).Returns(userId);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetCertificatesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            _sessionStorageService
                .Setup(x => x.SetAsync(OwnedCertificatesKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetOwnedCertificatesAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            JsonSerializer.Deserialize<List<Certificate>>(storedJson).Should().BeEquivalentTo(expected);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_ShouldResolveUserFromGovUkIdentifier_AndCacheCertificates()
        {
            // Arrange
            var govUkIdentifier = "gov-3";
            var user = CreateUser(govUkIdentifier);
            var expected = CreateCertificates();
            var response = new GetCertificatesQueryResult { Certificates = expected };
            string storedJson = null;

            SetupUserLookup(govUkIdentifier, user);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetCertificatesQuery>(q => q.UserId == user.Id),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            _sessionStorageService
                .Setup(x => x.SetAsync(OwnedCertificatesKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetOwnedCertificatesAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            JsonSerializer.Deserialize<List<Certificate>>(storedJson).Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_ShouldReturnNull_WhenGovUkIdentifierIsMissing()
        {
            // Arrange
            _userService.Setup(x => x.GetUserId()).Returns((Guid?)null);
            _userService.Setup(x => x.GetGovUkIdentifier()).Returns((string)null);

            // Act
            var result = await _sut.GetOwnedCertificatesAsync();

            // Assert
            result.Should().BeNull();
            _mediator.Verify(
                x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_ShouldReturnNull_WhenUserCannotBeResolved()
        {
            // Arrange
            const string govUkIdentifier = "gov-2";
            SetupUserLookup(govUkIdentifier, null);

            // Act
            var result = await _sut.GetOwnedCertificatesAsync();

            // Assert
            result.Should().BeNull();
            _mediator.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_ShouldNotCache_WhenQueryReturnsNoCertificates()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userService.Setup(x => x.GetUserId()).Returns(userId);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetCertificatesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCertificatesQueryResult { Certificates = null });

            // Act
            var result = await _sut.GetOwnedCertificatesAsync();

            // Assert
            result.Should().BeNull();
            _sessionStorageService.Verify(
                x => x.SetAsync(OwnedCertificatesKey, It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_ShouldReturnAuthorisationFromSession_WhenPresent()
        {
            // Arrange
            var expected = CreateAuthorisation();
            SetupSessionValue(UlnAuthorisationKey, expected);

            // Act
            var result = await _sut.GetUlnAuthorisationAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_ShouldUseKnownUserId_AndCacheAuthorisation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expected = CreateAuthorisation();
            var response = new GetCertificatesQueryResult { Authorisation = expected };
            string storedJson = null;

            _userService.Setup(x => x.GetUserId()).Returns(userId);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetCertificatesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            _sessionStorageService
                .Setup(x => x.SetAsync(UlnAuthorisationKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetUlnAuthorisationAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            JsonSerializer.Deserialize<UlnAuthorisation>(storedJson).Should().BeEquivalentTo(expected);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_ShouldResolveUserFromGovUkIdentifier_AndCacheAuthorisation()
        {
            // Arrange
            const string govUkIdentifier = "gov-u-2";
            var user = CreateUser(govUkIdentifier);
            var expected = CreateAuthorisation();
            var response = new GetCertificatesQueryResult { Authorisation = expected };
            string storedJson = null;

            SetupUserLookup(govUkIdentifier, user);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetCertificatesQuery>(q => q.UserId == user.Id),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            _sessionStorageService
                .Setup(x => x.SetAsync(UlnAuthorisationKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetUlnAuthorisationAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            JsonSerializer.Deserialize<UlnAuthorisation>(storedJson!).Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_ShouldReturnNull_WhenGovUkIdentifierIsMissing()
        {
            // Arrange
            _userService.Setup(x => x.GetUserId()).Returns((Guid?)null);
            _userService.Setup(x => x.GetGovUkIdentifier()).Returns(string.Empty);

            // Act
            var result = await _sut.GetUlnAuthorisationAsync();

            // Assert
            result.Should().BeNull();
            _mediator.Verify(
                x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _mediator.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_ShouldReturnNull_WhenUserCannotBeResolved()
        {
            // Arrange
            const string govUkIdentifier = "missing-user";
            SetupUserLookup(govUkIdentifier, null);

            // Act
            var result = await _sut.GetUlnAuthorisationAsync();

            // Assert
            result.Should().BeNull();
            _mediator.Verify(
                x => x.Send(It.IsAny<GetCertificatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_ShouldNotCache_WhenQueryReturnsNoAuthorisation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userService.Setup(x => x.GetUserId()).Returns(userId);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetCertificatesQuery>(q => q.UserId == userId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCertificatesQueryResult { Authorisation = null });

            // Act
            var result = await _sut.GetUlnAuthorisationAsync();

            // Assert
            result.Should().BeNull();
            _sessionStorageService.Verify(
                x => x.SetAsync(UlnAuthorisationKey, It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task SetAuthorisationAnswersAsync_ShouldStoreSerialisedAnswers()
        {
            // Arrange
            var expected = new AuthorisationAnswers { KnowUln = true, Uln = 1234567890L };
            string storedJson = null;
            _sessionStorageService
                .Setup(x => x.SetAsync(AuthorisationAnswersKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SetAuthorisationAnswersAsync(expected);

            // Assert
            JsonSerializer.Deserialize<AuthorisationAnswers>(storedJson).Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetAuthorisationAnswersAsync_ShouldReturnStoredAnswers()
        {
            // Arrange
            var expected = new AuthorisationAnswers { KnowUln = false, Uln = null };
            SetupSessionValue(AuthorisationAnswersKey, expected);

            // Act
            var result = await _sut.GetAuthorisationAnswersAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetAuthorisationAnswersAsync_ShouldReturnNull_WhenAnswersAreNotStored()
        {
            // Arrange
            _sessionStorageService
                .Setup(x => x.GetAsync(AuthorisationAnswersKey))
                .ReturnsAsync((string)null);

            // Act
            var result = await _sut.GetAuthorisationAnswersAsync();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ClearAuthorisationAnswersAsync_ShouldClearAnswers()
        {
            // Arrange

            // Act
            await _sut.ClearAuthorisationAnswersAsync();

            // Assert
            _sessionStorageService.Verify(x => x.ClearAsync(AuthorisationAnswersKey), Times.Once);
        }

        [Test]
        public async Task SetDeliveryAddressAsync_ShouldStoreSerialisedAddress()
        {
            // Arrange
            var expected = CreateAddress();
            string? storedJson = null;
            _sessionStorageService
                .Setup(x => x.SetAsync(DeliveryAddressKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SetDeliveryAddressAsync(expected);

            // Assert
            JsonSerializer.Deserialize<CheckAndSubmitViewModel>(storedJson!).Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task GetDeliveryAddressAsync_ShouldReturnStoredAddress()
        {
            // Arrange
            var expected = CreateAddress();
            SetupSessionValue(DeliveryAddressKey, expected);

            // Act
            var result = await _sut.GetDeliveryAddressAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task GetDeliveryAddressAsync_ShouldReturnNull_WhenAddressIsNotStored(string? storedValue)
        {
            // Arrange
            _sessionStorageService
                .Setup(x => x.GetAsync(DeliveryAddressKey))
                .ReturnsAsync(storedValue);

            // Act
            var result = await _sut.GetDeliveryAddressAsync();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ClearDeliveryAddressAsync_ShouldClearAddress()
        {
            // Arrange

            // Act
            await _sut.ClearDeliveryAddressAsync();

            // Assert
            _sessionStorageService.Verify(x => x.ClearAsync(DeliveryAddressKey), Times.Once);
        }

        [Test]
        public async Task SetContactReferenceAsync_ShouldStoreReference()
        {
            // Arrange

            // Act
            await _sut.SetContactReferenceAsync("reference-1");

            // Assert
            _sessionStorageService.Verify(
                x => x.SetAsync(ContactReferenceKey, "reference-1"),
                Times.Once);
        }

        [Test]
        public async Task GetContactReferenceAsync_ShouldReturnStoredReference()
        {
            // Arrange
            _sessionStorageService
                .Setup(x => x.GetAsync(ContactReferenceKey))
                .ReturnsAsync("reference-2");

            // Act
            var result = await _sut.GetContactReferenceAsync();

            // Assert
            result.Should().Be("reference-2");
        }

        [Test]
        public async Task ClearContactReferenceAsync_ShouldClearReference()
        {
            // Arrange

            // Act
            await _sut.ClearContactReferenceAsync();

            // Assert
            _sessionStorageService.Verify(x => x.ClearAsync(ContactReferenceKey), Times.Once);
        }

        [Test]
        public async Task AddRecordedSharingAccessCodeAsync_ShouldStoreCode_WhenNoCodesExist()
        {
            // Arrange
            var code = Guid.NewGuid();
            string storedJson = null;
            _sessionStorageService
                .Setup(x => x.SetAsync(RecordedSharingAccessKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.AddRecordedSharingAccessCodeAsync(code);

            // Assert
            JsonSerializer.Deserialize<List<string>>(storedJson).Should().Equal(code.ToString());
        }

        [Test]
        public async Task AddRecordedSharingAccessCodeAsync_ShouldAppendCode_AndPreserveExistingCodes()
        {
            // Arrange
            var existingCode = Guid.NewGuid();
            var newCode = Guid.NewGuid();
            string? storedJson = null;
            SetupSessionValue(RecordedSharingAccessKey, new List<string> { existingCode.ToString() });
            _sessionStorageService
                .Setup(x => x.SetAsync(RecordedSharingAccessKey, It.IsAny<string>()))
                .Callback<string, string>((_, value) => storedJson = value)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.AddRecordedSharingAccessCodeAsync(newCode);

            // Assert
            JsonSerializer.Deserialize<List<string>>(storedJson!).Should().Equal(
                existingCode.ToString(),
                newCode.ToString());
        }

        [Test]
        public async Task AddRecordedSharingAccessCodeAsync_ShouldNotStoreCode_WhenAlreadyPresent()
        {
            // Arrange
            var code = Guid.NewGuid();
            SetupSessionValue(RecordedSharingAccessKey, new List<string> { code.ToString() });

            // Act
            await _sut.AddRecordedSharingAccessCodeAsync(code);

            // Assert
            _sessionStorageService.Verify(
                x => x.SetAsync(RecordedSharingAccessKey, It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task IsSharingAccessCodeRecordedAsync_ShouldReturnTrue_WhenCodeIsPresent()
        {
            // Arrange
            var code = Guid.NewGuid();
            SetupSessionValue(RecordedSharingAccessKey, new List<string> { code.ToString() });

            // Act
            var result = await _sut.IsSharingAccessCodeRecordedAsync(code);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsSharingAccessCodeRecordedAsync_ShouldReturnFalse_WhenCodeIsNotPresent()
        {
            // Arrange
            SetupSessionValue(
                RecordedSharingAccessKey,
                new List<string> { Guid.NewGuid().ToString() });

            // Act
            var result = await _sut.IsSharingAccessCodeRecordedAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task IsSharingAccessCodeRecordedAsync_ShouldReturnFalse_WhenNoCodesAreStored()
        {
            // Arrange

            // Act
            var result = await _sut.IsSharingAccessCodeRecordedAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ClearSessionDataAsync_ShouldClearAllSessionValues()
        {
            // Arrange

            // Act
            await _sut.ClearSessionDataAsync();

            // Assert
            _sessionStorageService.Verify(x => x.ClearAsync(ShareEmailKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(OwnedCertificatesKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(UlnAuthorisationKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(AuthorisationAnswersKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(RecordedSharingAccessKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(DeliveryAddressKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(ContactReferenceKey), Times.Once);
            _sessionStorageService.Verify(x => x.ClearAsync(It.IsAny<string>()), Times.Exactly(7));
        }

        private void SetupUserLookup(string govUkIdentifier, User? user)
        {
            _userService.Setup(x => x.GetUserId()).Returns((Guid?)null);
            _userService.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetUserQuery>(q => q.GovUkIdentifier == govUkIdentifier),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
        }

        private void SetupSessionValue<T>(string key, T value)
        {
            _sessionStorageService
                .Setup(x => x.GetAsync(key))
                .ReturnsAsync(JsonSerializer.Serialize(value));
        }

        private static User CreateUser(string govUkIdentifier)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = govUkIdentifier,
                EmailAddress = "user@test.com"
            };
        }

        private static List<Certificate> CreateCertificates()
        {
            return new List<Certificate>
            {
                new Certificate
                {
                    CertificateId = Guid.NewGuid(),
                    CertificateType = CertificateType.Standard,
                    CourseName = "Course",
                    CourseLevel = "1"
                }
            };
        }

        private static UlnAuthorisation CreateAuthorisation()
        {
            return new UlnAuthorisation
            {
                AuthorisationId = Guid.NewGuid(),
                Uln = "1234567890",
                AuthorisedAt = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc)
            };
        }

        private static CheckAndSubmitViewModel CreateAddress()
        {
            return new CheckAndSubmitViewModel
            {
                CertificateId = Guid.NewGuid(),
                Organisation = "Organisation",
                AddressLine1 = "1 Test Street",
                Postcode = "AA1 1AA"
            };
        }
    }
}