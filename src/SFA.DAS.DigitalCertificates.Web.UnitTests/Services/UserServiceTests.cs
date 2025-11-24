using System;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    public class UserServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private DefaultHttpContext _httpContext;
        private UserService _sut;

        [SetUp]
        public void SetUp()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

            _sut = new UserService(_httpContextAccessorMock.Object);
        }

        [Test]
        public void When_UserHasValidUserIdClaim_Then_ReturnsParsedGuid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetAuthenticatedUser(new Claim(DigitalCertificateClaimsTypes.UserId, userId.ToString()));

            // Act
            var result = _sut.GetUserId();

            // Assert
            result.Should().Be(userId);
        }

        [Test]
        public void When_UserIdClaimMissing_Then_ReturnsNull()
        {
            // Arrange
            SetAuthenticatedUser(); // no user id claim

            // Act
            var result = _sut.GetUserId();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_UserIdClaimIsNotAGuid_Then_ReturnsNull()
        {
            // Arrange
            SetAuthenticatedUser(new Claim(DigitalCertificateClaimsTypes.UserId, "not-a-guid"));

            // Act
            var result = _sut.GetUserId();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_UserHasNameIdentifierClaim_Then_ReturnsClaimValue()
        {
            // Arrange
            SetAuthenticatedUser(new Claim(ClaimTypes.NameIdentifier, "gov-12345"));

            // Act
            var result = _sut.GetGovUkIdentifier();

            // Assert
            result.Should().Be("gov-12345");
        }

        [Test]
        public void When_NameIdentifierMissing_Then_ReturnsEmptyString()
        {
            // Arrange
            SetAuthenticatedUser();

            // Act
            var result = _sut.GetGovUkIdentifier();

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void When_UserIsNotAuthenticated_Then_GetGovUkIdentifierReturnsEmptyString()
        {
            // Arrange
            SetUnauthenticatedUser();

            // Act
            var result = _sut.GetGovUkIdentifier();

            // Assert
            result.Should().BeEmpty();
        }

        private void SetAuthenticatedUser(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
            _httpContext.User = new ClaimsPrincipal(identity);
        }

        private void SetUnauthenticatedUser()
        {
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
