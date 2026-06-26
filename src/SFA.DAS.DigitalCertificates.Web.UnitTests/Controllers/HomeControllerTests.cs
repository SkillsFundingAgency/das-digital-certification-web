using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Infrastructure;
using SFA.DAS.DigitalCertificates.Web.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IHomeOrchestrator> _orchestratorMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private DigitalCertificatesWebConfiguration _digitalCertificatesWebConfig;
        private HomeController _sut;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _orchestratorMock = new Mock<IHomeOrchestrator>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _digitalCertificatesWebConfig = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                OneLoginSettingsUrl = "http://settings.com",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                FrameworkTemplateBlobName = "framework-template",
                MasterPassword = "master-password",
                StorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "test-container",
                AsposeLicenseContainerName = "aspose-license-container",
                LicenseBlobName = "license-blob"               
            };

            _httpContext = new DefaultHttpContext();
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);

            _sut = new HomeController(
                _contextAccessorMock.Object,
                _orchestratorMock.Object,
                _loggerMock.Object,
                _digitalCertificatesWebConfig);
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();        

        [Test]
        public void Index_ShouldRedirect_To_ExternalStartPage_WhenConfigured()
        {
            _digitalCertificatesWebConfig.ExternalStartPage = "https://external-start-page.com";

            // Act
            var result = _sut.Index();

            // Assert
            var redirect = result as RedirectResult;
            redirect.Should().NotBeNull();
            redirect.Url.Should().Be("https://external-start-page.com");
        }

        [Test]
        public void Index_ShouldRedirect_To_View_When_ExternalStartPage_NotConfigured()
        {
            // Act
            var result = _sut.Index();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();            
        }

        [Test]
        public void Check_ShouldReturnView()
        {
            var result = _sut.Check() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void Locked_ShouldReturnView()
        {
            var result = _sut.Locked() as ViewResult;
            result.Should().NotBeNull();
        }
       
        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsTrue_ReturnsViewWithConsentAnalyticsCookieTrue()
        {
            // Arrange
            var controller = CreateControllerWithCookies(new Dictionary<string, string>
        {
            { CookieKeys.AnalyticsConsent, "true" }
        });

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeTrue();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsFalse_ReturnsViewWithConsentAnalyticsCookieFalse()
        {
            // Arrange
            var controller = CreateControllerWithCookies(new Dictionary<string, string>
            {
                { CookieKeys.AnalyticsConsent, "false" }
            });

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeFalse();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsMissing_ReturnsViewWithConsentAnalyticsCookieFalse()
        {
            // Arrange
            var controller = CreateControllerWithCookies();

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeFalse();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsInvalid_ReturnsViewWithConsentAnalyticsCookieFalse()
        {
            // Arrange
            var controller = CreateControllerWithCookies(new Dictionary<string, string>
            {
                { CookieKeys.AnalyticsConsent, "not-a-valid-bool" }
            });

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeFalse();
            model.BackUrl.Should().BeEmpty();
        }
       
        [Test]
        public void CookieDetails_ShouldReturnView()
        {
            var result = _sut.CookieDetails() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Verified_Should_Redirect_To_CertificatesList()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(
                    new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(), "Cookies")));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            _contextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = await _sut.Verified();

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect!.RouteName.Should().Be(CertificatesController.CertificatesListRouteGet);
        }

        [Test]
        public void AccessDenied_ShouldReturnView()
        {
            var result = _sut.AccessDenied() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void Error_ShouldLogError_And_ReturnView()
        {
            // Arrange
            var errorMessage = "Test error message";
            var httpContext = new DefaultHttpContext { TraceIdentifier = "TestTraceIdentifier" };
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);

            // Act
            var result = _sut.Error(errorMessage) as ViewResult;

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(errorMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            result.Should().NotBeNull();
            var model = result.Model as ErrorViewModel;
            model.Should().NotBeNull();
            model!.RequestId.Should().Be("TestTraceIdentifier");
            model.ErrorMessage.Should().Be(errorMessage);
        }

        [Test]
        public void AccessibilityStatement_ShouldReturnView_WithPageViewModel()
        {
            // Arrange
            var returnUrl = "/previous-page";
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _sut.Url = urlHelperMock.Object;

            // Act
            var result = _sut.AccessibilityStatement(returnUrl) as ViewResult;

            // Assert
            result.Should().NotBeNull();

            var model = result!.Model as PageViewModel;
            model.Should().NotBeNull();           
        }

        private HomeController CreateControllerWithCookies(
             Dictionary<string, string> cookies = null)
        {
            var requestCookieCollectionMock = new Mock<IRequestCookieCollection>();

            if (cookies is not null)
            {
                foreach (var cookie in cookies)
                {
                    requestCookieCollectionMock
                        .Setup(x => x[cookie.Key])
                        .Returns(cookie.Value);
                }
            }

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Cookies = requestCookieCollectionMock.Object;

            _contextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            var controller = new HomeController(
                _contextAccessorMock.Object,
                _orchestratorMock.Object,
                _loggerMock.Object, 
                _digitalCertificatesWebConfig)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            return controller;
        }
    }
}
