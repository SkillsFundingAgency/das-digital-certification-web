using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class ServiceControllerTests
    {
        private Mock<IConfiguration> _configMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<ISessionService> _sessionServiceMock;
        private ServiceController _sut;

        [SetUp]
        public void Setup()
        {
            _configMock = new Mock<IConfiguration>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _userServiceMock = new Mock<IUserService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _sessionServiceMock = new Mock<ISessionService>();

            _sessionServiceMock.Setup(s => s.ClearSessionDataAsync()).Returns(Task.CompletedTask);

            _sut = new ServiceController(
                _userServiceMock.Object,
                _cacheServiceMock.Object,
                _sessionServiceMock.Object,
                _configMock.Object,
                _contextAccessorMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public async Task SigningOut_ReturnsSignOutResult_WithOidcHint_AndSchemes_AndClearsSession()
        {
            // Arrange
            var idToken = "some_id_token";
            var govUkIdentifier = "gov-123";

            _userServiceMock.Setup(x => x.GetGovUkIdentifier())
                .Returns(govUkIdentifier);

            var httpMock = new Mock<HttpContext>();

            var sessionMock = new Mock<ISession>();
            httpMock.SetupGet(h => h.Session).Returns(sessionMock.Object);

            var oidcProps = new AuthenticationProperties();
            oidcProps.StoreTokens(new[]
            {
                new AuthenticationToken { Name = OpenIdConnectParameterNames.IdToken, Value = idToken }
            });

            var oidcTicket = new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme)),
                oidcProps,
                OpenIdConnectDefaults.AuthenticationScheme);

            var cookieTicket = new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties(),
                CookieAuthenticationDefaults.AuthenticationScheme);

            var auth = new Mock<IAuthenticationService>();
            auth.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), CookieAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(AuthenticateResult.Success(cookieTicket));
            auth.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), OpenIdConnectDefaults.AuthenticationScheme))
                .ReturnsAsync(AuthenticateResult.Success(oidcTicket));

            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(auth.Object);
            httpMock.SetupGet(h => h.RequestServices).Returns(sp.Object);

            _contextAccessorMock.Setup(a => a.HttpContext).Returns(httpMock.Object);
            _configMock.Setup(c => c["StubAuth"]).Returns("false");

            // Act
            var actionResult = await _sut.SigningOut();

            // Assert
            var signOut = actionResult as SignOutResult;
            signOut.Should().NotBeNull();

            signOut!.AuthenticationSchemes.Should().Contain(new[]
            {
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            });

            signOut.Properties.Parameters.Should().ContainKey(OpenIdConnectParameterNames.IdTokenHint);
            signOut.Properties.Parameters[OpenIdConnectParameterNames.IdTokenHint].Should().Be(idToken);

            _cacheServiceMock.Verify(
                x => x.Clear(govUkIdentifier),
                Times.Once
            );

            _sessionServiceMock.Verify(s => s.ClearSessionDataAsync(), Times.Once);
        }

        [Test]
        public void SignedOut_ShouldDeleteAuthCookie()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var responseCookiesMock = new Mock<IResponseCookies>();

            httpContext.Setup(c => c.Response).Returns(responseMock.Object);
            responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);

            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext.Object);

            // Act
            _sut.SignedOut();

            // Assert
            responseCookiesMock.Verify(c => c.Delete("SFA.DAS.DigitalCertificates.Web.Auth"), Times.Once);
        }
    }
}