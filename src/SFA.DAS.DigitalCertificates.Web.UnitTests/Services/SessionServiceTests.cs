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
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    [TestFixture]
    public class SessionServiceTests
    {
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMediator> _mediatorMock = null!;
        private SessionService _sut = null!;

        private const string UsernameKey = "DigitalCertificates:Username";
        private const string ShareEmailKey = "DigitalCertificates:ShareEmail";
        private const string OwnedCertificatesKeyPrefix = "DigitalCertificates:OwnedCertificates:";
        private const string UlnAuthorisationKeyPrefix = "DigitalCertificates:UlnAuthorisation:";
        private const string RecordedSharingAccessKey = "DigitalCertificates:RecordedSharingAccessCodes";

        [SetUp]
        public void SetUp()
        {
            _sessionStorageMock = new Mock<ISessionStorageService>();
            _mediatorMock = new Mock<IMediator>();

            _sut = new SessionService(_sessionStorageMock.Object, _mediatorMock.Object);
        }

        [Test]
        public async Task SetUsernameAsync_Calls_Storage_With_Correct_Key()
        {
            await _sut.SetUsernameAsync("bob");

            _sessionStorageMock.Verify(s => s.SetAsync(UsernameKey, "bob"), Times.Once);
        }

        [Test]
        public async Task GetUserNameAsync_Returns_Value_From_Storage()
        {
            _sessionStorageMock.Setup(s => s.GetAsync(UsernameKey)).ReturnsAsync("alice");

            var result = await _sut.GetUserNameAsync();

            result.Should().Be("alice");
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
        public async Task ClearSessionDataAsync_Clears_Username_And_ShareEmail_Only_When_Id_Empty()
        {
            var govId = string.Empty;

            await _sut.ClearSessionDataAsync(govId);

            _sessionStorageMock.Verify(s => s.ClearAsync(ShareEmailKey), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(UsernameKey), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(It.Is<string>(k => k.StartsWith(OwnedCertificatesKeyPrefix))), Times.Never);
            _sessionStorageMock.Verify(s => s.ClearAsync(It.Is<string>(k => k.StartsWith(UlnAuthorisationKeyPrefix))), Times.Never);
        }

        [Test]
        public async Task ClearSessionDataAsync_Clears_Namespaced_Keys_When_Id_Provided()
        {
            var govId = "gov-123";

            await _sut.ClearSessionDataAsync(govId);

            _sessionStorageMock.Verify(s => s.ClearAsync(ShareEmailKey), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(UsernameKey), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(OwnedCertificatesKeyPrefix + govId), Times.Once);
            _sessionStorageMock.Verify(s => s.ClearAsync(UlnAuthorisationKeyPrefix + govId), Times.Once);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_Returns_From_Session_If_Present()
        {
            var govId = "gov-1";
            var expected = new List<Certificate> { new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "C", CourseLevel = "1" } };
            var json = JsonSerializer.Serialize(expected);

            _sessionStorageMock.Setup(s => s.GetAsync(OwnedCertificatesKeyPrefix + govId)).ReturnsAsync(json);

            var result = await _sut.GetOwnedCertificatesAsync(govId);

            result.Should().BeEquivalentTo(expected);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_Returns_Null_If_User_Not_Found()
        {
            var govId = "gov-2";

            _sessionStorageMock.Setup(s => s.GetAsync(OwnedCertificatesKeyPrefix + govId)).ReturnsAsync((string?)null);
            _mediatorMock.Setup(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var result = await _sut.GetOwnedCertificatesAsync(govId);

            result.Should().BeNull();
            _mediatorMock.Verify(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetOwnedCertificatesAsync_Calls_Api_And_Stores_In_Session_When_Missing()
        {
            var govId = "gov-3";
            var user = new User { Id = Guid.NewGuid(), GovUkIdentifier = govId, EmailAddress = "user@test.com" };
            var expectedCertificates = new List<Certificate> { new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "Course", CourseLevel = "1" } };

            _sessionStorageMock.Setup(s => s.GetAsync(OwnedCertificatesKeyPrefix + govId)).ReturnsAsync((string?)null);
            _mediatorMock.Setup(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var response = new GetCertificatesQueryResult { Certificates = expectedCertificates };
            _mediatorMock.Setup(m => m.Send(It.Is<GetCertificatesQuery>(q => q.UserId == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            string? storedJson = null;
            _sessionStorageMock.Setup(s => s.SetAsync(OwnedCertificatesKeyPrefix + govId, It.IsAny<string>()))
                .Callback<string, string>((k, v) => storedJson = v)
                .Returns(Task.CompletedTask);

            var result = await _sut.GetOwnedCertificatesAsync(govId);

            result.Should().BeEquivalentTo(expectedCertificates);
            storedJson.Should().NotBeNullOrEmpty();
            var des = JsonSerializer.Deserialize<List<Certificate>>(storedJson!);
            des.Should().BeEquivalentTo(expectedCertificates);
        }

        [Test]
        public async Task GetUlnAuthorisationAsync_Returns_From_Session_If_Present()
        {
            var govId = "gov-u-1";
            var expected = new UlnAuthorisation { AuthorisationId = Guid.NewGuid(), Uln = "123", AuthorisedAt = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(expected);

            _sessionStorageMock.Setup(s => s.GetAsync(UlnAuthorisationKeyPrefix + govId)).ReturnsAsync(json);

            var result = await _sut.GetUlnAuthorisationAsync(govId);

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

            _sessionStorageMock.Setup(s => s.GetAsync(UlnAuthorisationKeyPrefix + govId)).ReturnsAsync((string?)null);
            _mediatorMock.Setup(m => m.Send(It.Is<GetUserQuery>(q => q.GovUkIdentifier == govId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var response = new GetCertificatesQueryResult { Authorisation = expectedAuth };
            _mediatorMock.Setup(m => m.Send(It.Is<GetCertificatesQuery>(q => q.UserId == user.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            string? storedJson = null;
            _sessionStorageMock.Setup(s => s.SetAsync(UlnAuthorisationKeyPrefix + govId, It.IsAny<string>()))
                .Callback<string, string>((k, v) => storedJson = v)
                .Returns(Task.CompletedTask);

            var result = await _sut.GetUlnAuthorisationAsync(govId);

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
            string? storedJson = null;

            _sessionStorageMock.Setup(s => s.GetAsync(RecordedSharingAccessKey)).ReturnsAsync((string?)null);
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

            _sessionStorageMock.Setup(s => s.GetAsync(RecordedSharingAccessKey)).ReturnsAsync((string?)null);

            // Act
            var result = await _sut.IsSharingAccessCodeRecordedAsync(code);

            // Assert
            result.Should().BeFalse();
        }
    }
}
