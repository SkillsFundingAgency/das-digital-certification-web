using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Authentication
{
    public class UlnAuthorisedAuthorizationHandlerTests
    {
        private Mock<ISessionStorageService> _sessionMock;
        private Mock<IUserService> _userServiceMock;
        private UlnAuthorisedAuthorizationHandler _sut;

        private AuthorizationHandlerContext _authContext;
        private UlnAuthorisedRequirement _requirement;

        [SetUp]
        public void SetUp()
        {
            _sessionMock = new Mock<ISessionStorageService>();
            _userServiceMock = new Mock<IUserService>();

            _sut = new UlnAuthorisedAuthorizationHandler(
                _sessionMock.Object,
                _userServiceMock.Object);

            _requirement = new UlnAuthorisedRequirement();
            _authContext = new AuthorizationHandlerContext(
                new[] { _requirement },
                new ClaimsPrincipal(),
                null);
        }

        [Test]
        public async Task When_UlnIsAuthorised_Then_Succeeds()
        {
            // Arrange
            var govId = "gov-123";
            _userServiceMock.Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionMock.Setup(x => x.GetUlnAuthorisationAsync(govId))
                .ReturnsAsync(new UlnAuthorisation { AuthorisationId = Guid.NewGuid(), AuthorisedAt = DateTime.Now, Uln = "123456789" });

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasSucceeded.Should().BeTrue();
            _authContext.HasFailed.Should().BeFalse();
        }

        [Test]
        public async Task When_UlnIsNotAuthorised_Then_Fails()
        {
            // Arrange
            var govId = "gov-123";
            _userServiceMock.Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionMock.Setup(x => x.GetUlnAuthorisationAsync(govId))
                .ReturnsAsync((UlnAuthorisation)null);

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasFailed.Should().BeTrue();
            _authContext.FailureReasons
                .Should()
                .ContainSingle(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotUlnAuthorized);
        }
    }
}
