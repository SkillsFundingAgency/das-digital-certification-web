using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Authorization
{
    [TestFixture]
    public class DigitalCertificateCustomClaimsTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<ICacheService> _cacheServiceMock;
        private DigitalCertificateCustomClaims _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _cacheServiceMock = new Mock<ICacheService>();
            _sut = new DigitalCertificateCustomClaims(_mediatorMock.Object, _cacheServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task GetClaims_WithPrincipal_Adds_UserId_And_Allowed_When_User_Not_Locked()
        {
            // Arrange
            var govUkIdentifier = "gov-123";
            var user = new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = "gov-123",
                EmailAddress = "name@domain.com",
                IsLocked = false
            };

            _cacheServiceMock
                .Setup(x => x.GetUserAsync(govUkIdentifier))
                .ReturnsAsync(user);

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, govUkIdentifier),
                new Claim(ClaimTypes.Email, user.EmailAddress)
            }));

            // Act
            var result = await _sut.GetClaims(principal);

            // Assert
            result.Should().Contain(c => c.Type == DigitalCertificateClaimsTypes.UserId && c.Value == user.Id.ToString());
            result.Should().Contain(c => c.Type == ClaimTypes.AuthorizationDecision && c.Value == AuthorizationDecisions.Allowed);
        }

        [Test]
        public async Task GetClaims_WithPrincipal_Adds_Suspended_When_User_Is_Locked()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = "gov-123",
                EmailAddress = "name@domain.com",
                IsLocked = true
            };

            _cacheServiceMock
                .Setup(x => x.GetUserAsync(user.GovUkIdentifier))
                .ReturnsAsync(user);

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.GovUkIdentifier),
                new Claim(ClaimTypes.Email, user.EmailAddress)
            }));

            // Act
            var result = await _sut.GetClaims(principal);

            // Assert
            result.Should().Contain(c => c.Type == DigitalCertificateClaimsTypes.UserId && c.Value == user.Id.ToString());
            result.Should().Contain(c => c.Type == ClaimTypes.AuthorizationDecision && c.Value == AuthorizationDecisions.Suspended);
        }

        [Test]
        public async Task GetClaims_WithPrincipal_ReturnsEmpty_When_User_Not_Found()
        {
            // Arrange
            var govUkIdentifier = "gov-123";
            _cacheServiceMock
                .Setup(x => x.GetUserAsync(govUkIdentifier))
                .ReturnsAsync((User)null);

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, govUkIdentifier)
            }));

            // Act
            var result = await _sut.GetClaims(principal);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetClaims_WithNullPrincipal_ReturnsEmpty()
        {
            // Act
            var result = await _sut.GetClaims((ClaimsPrincipal)null);

            // Assert
            result.Should().BeEmpty();
            _cacheServiceMock.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetClaims_WithTokenValidatedContext_Delegates_To_Principal()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                GovUkIdentifier = "gov-123",
                EmailAddress = "name@domain.com",
                IsLocked = false
            };

            _cacheServiceMock
                .Setup(x => x.GetUserAsync(user.GovUkIdentifier))
                .ReturnsAsync(user);

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.GovUkIdentifier),
                new Claim(ClaimTypes.Email, user.EmailAddress)
            }));

            var httpContext = new DefaultHttpContext();
            var authScheme = new AuthenticationScheme("oidc", "oidc", typeof(OpenIdConnectHandler));
            var authOptions = new OpenIdConnectOptions();
            var authProperties = new AuthenticationProperties();

            var tokenContext = new TokenValidatedContext(httpContext, authScheme, authOptions, principal, authProperties);

            // Act
            var result = await _sut.GetClaims(tokenContext);

            // Assert
            result.Should().Contain(c => c.Type == DigitalCertificateClaimsTypes.UserId && c.Value == user.Id.ToString());
            result.Should().Contain(c => c.Type == ClaimTypes.AuthorizationDecision && c.Value == AuthorizationDecisions.Allowed);
        }


        [Test]
        public async Task GetClaims_WithNullContext_ReturnsEmpty()
        {
            // Act
            var result = await _sut.GetClaims((TokenValidatedContext)null);

            // Assert
            result.Should().BeEmpty();
            _cacheServiceMock.Verify(x => x.GetUserAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
