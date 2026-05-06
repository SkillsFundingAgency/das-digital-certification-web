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
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMediator> _mediatorMock = null!;
        private Mock<IUserService> _userServiceMock = null!;
        private SessionService _sut = null!;

        private const string ShareEmailKey = "DigitalCertificates:ShareEmail";
        private const string OwnedCertificatesKeyPrefix = "DigitalCertificates:OwnedCertificates:";
        private const string UlnAuthorisationKeyPrefix = "DigitalCertificates:UlnAuthorisation:";
        private const string RecordedSharingAccessKey = "DigitalCertificates:RecordedSharingAccessCodes";
        private const string DeliveryAddressKeyPrefix = "DigitalCertificates:DeliveryAddress:";
        private const string AuthorisationAnswersKeyPrefix = "DigitalCertificates:AuthorisationAnswers:";

        [SetUp]
        public void SetUp()
        {
            _sessionStorageMock = new Mock<ISessionStorageService>();
            _mediatorMock = new Mock<IMediator>();
            _userServiceMock = new Mock<IUserService>();

            _sut = new SessionService(_sessionStorageMock.Object, _mediatorMock.Object, _userServiceMock.Object);
        }

        [Test]
        public async Task SetShareEmailAsync_Calls_Storage_With_Correct_Key()
        {
            await _sut.SetShareEmailAsync("a@b.com");

            _sessionStorageMock.Verify(s => s.SetAsync(ShareEmailKey, "a@b.com"), Times.Once);
        }

        [Test]
        public async Task GetShareEmailAsync_Returns_Value_From_Storage()
        {
            _sessionStorageMock.Setup(s => s.GetAsync(ShareEmailKey)).ReturnsAsync("x@y.com");

            var result = await _sut.GetShareEmailAsync();

            result.Should().Be("x@y.com");
        }

        [Test]
        public async Task ClearSessionDataAsync_Clears_Namespaced_Keys_When_Id_Provided()
        {
            await _sut.ClearSessionDataAsync();

            _sessionStorageMock.Verify(s => s.ClearAsync(ShareEmailKey), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(OwnedCertificatesKeyPrefix), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(UlnAuthorisationKeyPrefix), Times.Once);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_Returns_From_Session_If_Present()
        {
            var expected = new List<Certificate> { new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "C", CourseLevel = "1" } };
            var json = JsonSerializer.Serialize(expected);

            _sessionStorageMock.Setup(s => s.GetAsync(OwnedCertificatesKeyPrefix)).ReturnsAsync(json);

            var result = await _sut.GetOwnedCertificatesAsync();

            result.Should().BeEquivalentTo(expected);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_Returns_Null_If_User_Not_Found()
        {
            var govId = "gov-2";

            _sessionStorageMock.Setup(s => s.GetAsync(OwnedCertificatesKeyPrefix)).ReturnsAsync((string)null);
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);
            _mediatorMock.Setup(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            var result = await _sut.GetOwnedCertificatesAsync();

            result.Should().BeNull();
            _mediatorMock.Verify(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_Calls_Api_And_Stores_In_Session_When_Missing()
        {
            var govId = "gov-3";
            var user = new User { Id = Guid.NewGuid(), GovUkIdentifier = govId, EmailAddress = "user@test.com" };
            var expectedCertificates = new List<Certificate> { new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "Course", CourseLevel = "1" } };

            _sessionStorageMock.Setup(s => s.GetAsync(OwnedCertificatesKeyPrefix)).ReturnsAsync((string)null);
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);
            _mediatorMock.Setup(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var response = new GetCertificatesQueryResult { Certificates = expectedCertificates };
            _mediatorMock.Setup(m => m.Send(It.Is<GetCertificatesQuery>(q => q.UserId == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            string storedJson = null!;
            _sessionStorageMock.Setup(s => s.SetAsync(OwnedCertificatesKeyPrefix, It.IsAny<string>()))
                .Callback<string, string>((k, v) => storedJson = v)
                .Returns(Task.CompletedTask);

            var result = await _sut.GetOwnedCertificatesAsync();

            result.Should().BeEquivalentTo(expectedCertificates);
            storedJson.Should().NotBeNullOrEmpty();
            var des = JsonSerializer.Deserialize<List<Certificate>>(storedJson!);
            des.Should().BeEquivalentTo(expectedCertificates);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_Returns_From_Session_If_Present()
        {
            var expected = new UlnAuthorisation { AuthorisationId = Guid.NewGuid(), Uln = "123", AuthorisedAt = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(expected);


            _sessionStorageMock.Setup(s => s.GetAsync(UlnAuthorisationKeyPrefix)).ReturnsAsync(json);

            var result = await _sut.GetUlnAuthorisationAsync();

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expected);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_Calls_Api_And_Stores_In_Session_When_Missing()
        {
            var govId = "gov-u-2";
            var user = new User { Id = Guid.NewGuid(), GovUkIdentifier = govId, EmailAddress = "user@test.com" };
            var expectedAuth = new UlnAuthorisation { AuthorisationId = Guid.NewGuid(), Uln = "999", AuthorisedAt = DateTime.UtcNow };

            _sessionStorageMock.Setup(s => s.GetAsync(UlnAuthorisationKeyPrefix)).ReturnsAsync((string)null);
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);
            _mediatorMock.Setup(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var response = new GetCertificatesQueryResult { Authorisation = expectedAuth };
            _mediatorMock.Setup(m => m.Send(It.Is<GetCertificatesQuery>(q => q.UserId == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            string storedJson = null!;
            _sessionStorageMock.Setup(s => s.SetAsync(UlnAuthorisationKeyPrefix, It.IsAny<string>()))
                .Callback<string, string>((k, v) => storedJson = v)
                .Returns(Task.CompletedTask);

            var result = await _sut.GetUlnAuthorisationAsync();

            result.Should().BeEquivalentTo(expectedAuth);
            storedJson.Should().NotBeNullOrEmpty();
            var des = JsonSerializer.Deserialize<UlnAuthorisation>(storedJson!);
            des.Should().BeEquivalentTo(expectedAuth);
        }

        [Test]
        public async Task AddRecordedSharingAccessCodeAsync_Adds_Code_When_Not_Present()
        {
            // Arrange
            var code = Guid.NewGuid();
            string storedJson = null!;

            _sessionStorageMock.Setup(s => s.GetAsync(RecordedSharingAccessKey)).ReturnsAsync((string)null);
            _sessionStorageMock.Setup(s => s.SetAsync(RecordedSharingAccessKey, It.IsAny<string>()))
                .Callback<string, string>((k, v) => storedJson = v)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.AddRecordedSharingAccessCodeAsync(code);

            // Assert
            storedJson.Should().NotBeNullOrEmpty();
            var list = JsonSerializer.Deserialize<List<string>>(storedJson!);
            list.Should().Contain(code.ToString());
            _sessionStorageMock.Verify(s => s.SetAsync(RecordedSharingAccessKey, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task AddRecordedSharingAccessCodeAsync_Does_Not_Add_When_Already_Present()
        {
            // Arrange
            var code = Guid.NewGuid();
            var existing = new List<string> { code.ToString() };
            var json = JsonSerializer.Serialize(existing);

            _sessionStorageMock.Setup(s => s.GetAsync(RecordedSharingAccessKey)).ReturnsAsync(json);

            // Act
            await _sut.AddRecordedSharingAccessCodeAsync(code);

            // Assert
            _sessionStorageMock.Verify(s => s.SetAsync(RecordedSharingAccessKey, It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task IsSharingAccessCodeRecordedAsync_Returns_True_When_Present()
        {
            // Arrange
            var code = Guid.NewGuid();
            var existing = new List<string> { code.ToString() };
            var json = JsonSerializer.Serialize(existing);

            _sessionStorageMock.Setup(s => s.GetAsync(RecordedSharingAccessKey)).ReturnsAsync(json);

            // Act
            var result = await _sut.IsSharingAccessCodeRecordedAsync(code);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsSharingAccessCodeRecordedAsync_Returns_False_When_Not_Present()
        {
            // Arrange
            var code = Guid.NewGuid();

            _sessionStorageMock.Setup(s => s.GetAsync(RecordedSharingAccessKey)).ReturnsAsync((string)null);

            // Act
            var result = await _sut.IsSharingAccessCodeRecordedAsync(code);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task SetDeliveryAddressAsync_Calls_Storage_With_Correct_Key()
        {
            var addr = new CheckAndSubmitViewModel
            {
                CertificateId = Guid.NewGuid(),
                Organisation = "Org",
                AddressLine1 = "L1",
                Postcode = "PC1"
            };

            await _sut.SetDeliveryAddressAsync(addr);

            _sessionStorageMock.Verify(s => s.SetAsync(DeliveryAddressKeyPrefix, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetDeliveryAddressAsync_Returns_Value_From_Storage()
        {
            var addr = new CheckAndSubmitViewModel
            {
                CertificateId = Guid.NewGuid(),
                Organisation = "Org",
                AddressLine1 = "L1",
                Postcode = "PC1"
            };
            var json = JsonSerializer.Serialize(addr);

            _sessionStorageMock.Setup(s => s.GetAsync(DeliveryAddressKeyPrefix)).ReturnsAsync(json);

            var result = await _sut.GetDeliveryAddressAsync();

            result.Should().NotBeNull();
            result!.Organisation.Should().Be("Org");
            result.AddressLine1.Should().Be("L1");
            result.Postcode.Should().Be("PC1");
        }

        [Test]
        public async Task ClearDeliveryAddressAsync_Calls_ClearAsync_With_Correct_Key()
        {
            await _sut.ClearDeliveryAddressAsync();

            _sessionStorageMock.Verify(s => s.ClearAsync(DeliveryAddressKeyPrefix), Times.Once);
        }

        [Test]
        public async Task SetAuthorisationAnswersAsync_Calls_Storage_With_Correct_Key()
        {
            var answers = new AuthorisationAnswers { KnowUln = true, Uln = 1234567890L };

            await _sut.SetAuthorisationAnswersAsync(answers);

            _sessionStorageMock.Verify(s => s.SetAsync(AuthorisationAnswersKeyPrefix, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetAuthorisationAnswersAsync_Returns_Value_From_Storage()
        {
            var answers = new AuthorisationAnswers { KnowUln = false, Uln = (long?)null };
            var json = JsonSerializer.Serialize(answers);

            _sessionStorageMock.Setup(s => s.GetAsync(AuthorisationAnswersKeyPrefix)).ReturnsAsync(json);

            var result = await _sut.GetAuthorisationAnswersAsync();

            result.Should().NotBeNull();
            result!.KnowUln.Should().BeFalse();
            result.Uln.Should().BeNull();
        }

        [Test]
        public async Task ClearAuthorisationAnswersAsync_Calls_ClearAsync_With_Correct_Key()
        {
            await _sut.ClearAuthorisationAnswersAsync();

            _sessionStorageMock.Verify(s => s.ClearAsync(AuthorisationAnswersKeyPrefix), Times.Once);
        }
    }
}
