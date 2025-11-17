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

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class ServiceControllerTests
    {
        private Mock<IConfiguration> _configMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private ServiceController _sut;

        [SetUp]
        public void Setup()
        {
            
            _configMock = new Mock<IConfiguration>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            
            _sut = new ServiceController(
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
        public async Task SigningOut_ReturnsSignOutResult_WithOidcHint_AndSchemes()
        {
            // Arrange
            var idToken = "some_id_token";
            var http = new DefaultHttpContext();

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
            auth.Setup(a => a.AuthenticateAsync(http, CookieAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(AuthenticateResult.Success(cookieTicket));
            auth.Setup(a => a.AuthenticateAsync(http, OpenIdConnectDefaults.AuthenticationScheme))
                .ReturnsAsync(AuthenticateResult.Success(oidcTicket));

            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(auth.Object);
            http.RequestServices = sp.Object;

            _contextAccessorMock.Setup(a => a.HttpContext).Returns(http);
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

            auth.Verify(a => a.AuthenticateAsync(http, OpenIdConnectDefaults.AuthenticationScheme), Times.AtLeastOnce);
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