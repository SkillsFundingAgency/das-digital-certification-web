using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.TestHelper.Extensions;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Models;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IHomeOrchestrator> _orchestratorMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IGovUkAuthenticationService> _govUkAuthenticationServiceMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            _orchestratorMock = new Mock<IHomeOrchestrator>();
            _configMock = new Mock<IConfiguration>();
            _govUkAuthenticationServiceMock = new Mock<IGovUkAuthenticationService>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            
            _sut = new HomeController(
                _orchestratorMock.Object,
                _configMock.Object,
                _govUkAuthenticationServiceMock.Object,
                _contextAccessorMock.Object,
                _loggerMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public void Index_ShouldReturnView_WhenRunningLocally()
        {
            // Arrange
            _configMock
                .Setup(p => p["EnvironmentName"])
                .Returns("LOCAL");

            // Act
            var result = _sut.Index() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Index_ShouldReturnView_WhenRunningInDev()
        {
            // Arrange
            _configMock
                .Setup(p => p["EnvironmentName"])
                .Returns("DEV");

            // Act
            var result = _sut.Index() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void AccessDenied_ShouldReturnView()
        {
            // Act
            var result = _sut.AccessDenied() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Error_ShouldLogErrorAndReturnView()
        {
            // Arrange
            var errorMessage = "Test error message";
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "TestTraceIdentifier";
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);

            // Act
            var result = _sut.Error(errorMessage) as ViewResult;

            // Assert
            _loggerMock.VerifyLogError(errorMessage, Times.Once);

            result.Should().NotBeNull();
            var model = result.Model as ErrorViewModel;
            model.Should().NotBeNull();
            model.RequestId.Should().Be("TestTraceIdentifier");
            model.ErrorMessage.Should().Be(errorMessage);
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
        public void UserSignedOut_ShouldDeleteAuthCookie()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var responseCookiesMock = new Mock<IResponseCookies>();

            httpContext.Setup(c => c.Response).Returns(responseMock.Object);
            responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);

            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext.Object);

            // Act
            _sut.UserSignedOut();

            // Assert
            responseCookiesMock.Verify(c => c.Delete("SFA.DAS.DigitalCertificates.Web.Auth"), Times.Once);
        }
    }
}