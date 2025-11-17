using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Authorization
{
    [TestFixture]
    public class DigitalCertificatesClaimsTransformerTests
    {
        private Mock<IUserCacheService> _userCacheServiceMock;
        private DigitalCertificatesClaimsTransformer _sut;

        [SetUp]
        public void SetUp()
        {
            _userCacheServiceMock = new Mock<IUserCacheService>();
            _sut = new DigitalCertificatesClaimsTransformer(_userCacheServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task TransformAsync_ShouldReturnSamePrincipal_WhenNotAuthenticated()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _sut.TransformAsync(principal);

            // Assert
            result.Should().BeSameAs(principal);
            _userCacheServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TransformAsync_ShouldReturnSamePrincipal_WhenUserIdClaimMissing()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "gov-123")
            }, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _sut.TransformAsync(principal);

            // Assert
            result.Should().BeSameAs(principal);
            _userCacheServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TransformAsync_ShouldReturnSamePrincipal_WhenNameIdentifierMissing()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(DigitalCertificateClaimsTypes.UserId, Guid.NewGuid().ToString())
            }, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _sut.TransformAsync(principal);

            // Assert
            result.Should().BeSameAs(principal);
            _userCacheServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task TransformAsync_ShouldNotChangeAuthorizationDecision_WhenUserNotLocked()
        {
            // Arrange
            var govUkIdentifier = "gov-123";
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(DigitalCertificateClaimsTypes.UserId, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, govUkIdentifier),
                new Claim(ClaimTypes.AuthorizationDecision, AuthorizationDecisions.Allowed)
            }, "TestAuth");

            var principal = new ClaimsPrincipal(identity);

            _userCacheServiceMock
                .Setup(s => s.CacheUserForGovUkIdentifier(govUkIdentifier))
                .ReturnsAsync(new User { LockedAt = null });

            // Act
            var result = await _sut.TransformAsync(principal);

            // Assert
            result.FindFirst(ClaimTypes.AuthorizationDecision)!.Value
                .Should().Be(AuthorizationDecisions.Allowed);

            _userCacheServiceMock.Verify(s => s.CacheUserForGovUkIdentifier(govUkIdentifier), Times.Once);
        }

        [Test]
        public async Task TransformAsync_ShouldUpdateAuthorizationDecision_WhenUserLocked()
        {
            // Arrange
            var govUkIdentifier = "gov-123";
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(DigitalCertificateClaimsTypes.UserId, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, govUkIdentifier),
                new Claim(ClaimTypes.AuthorizationDecision, AuthorizationDecisions.Allowed)
            }, "TestAuth");

            var principal = new ClaimsPrincipal(identity);

            _userCacheServiceMock
                .Setup(s => s.CacheUserForGovUkIdentifier(govUkIdentifier))
                .ReturnsAsync(new User { LockedAt = DateTime.UtcNow });

            // Act
            var result = await _sut.TransformAsync(principal);

            // Assert
            result.FindFirst(ClaimTypes.AuthorizationDecision)!.Value
                .Should().Be(AuthorizationDecisions.Suspended);

            _userCacheServiceMock.Verify(s => s.CacheUserForGovUkIdentifier(govUkIdentifier), Times.Once);
        }

        [Test]
        public async Task TransformAsync_ShouldNotChangePrincipal_WhenAuthorizationDecisionClaimMissing()
        {
            // Arrange
            var govUkIdentifier = "gov-123";
            var identity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(DigitalCertificateClaimsTypes.UserId, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, govUkIdentifier)
            }, "TestAuth");

            var principal = new ClaimsPrincipal(identity);

            _userCacheServiceMock
                .Setup(s => s.CacheUserForGovUkIdentifier(govUkIdentifier))
                .ReturnsAsync(new User { LockedAt = null });

            // Act
            var result = await _sut.TransformAsync(principal);

            // Assert
            result.Should().BeSameAs(principal);
            result.Claims.Should().NotContain(c => c.Type == ClaimTypes.AuthorizationDecision);
        }
    }
}
