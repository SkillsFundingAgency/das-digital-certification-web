using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
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
using SFA.DAS.GovUK.Auth.Controllers.Routes;

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

            _digitalCertificatesWebConfig =
                new DigitalCertificatesWebConfiguration
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

            _contextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(_httpContext);

            _sut = new HomeController(
                _contextAccessorMock.Object,
                _orchestratorMock.Object,
                _loggerMock.Object,
                _digitalCertificatesWebConfig);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public void Index_ShouldRedirectToExternalStartPage_WhenConfigured()
        {
            // Arrange
            _digitalCertificatesWebConfig.ExternalStartPage =
                "https://external-start-page.com";

            // Act
            var result = _sut.Index();

            // Assert
            var redirectResult = result.Should()
                .BeOfType<RedirectResult>()
                .Subject;

            redirectResult.Url.Should()
                .Be("https://external-start-page.com");
        }

        [Test]
        public void Index_ShouldReturnView_WhenExternalStartPageNotConfigured()
        {
            // Act
            var result = _sut.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void Check_WithLocalReturnUrl_ShouldReturnViewWithReturnUrl()
        {
            // Arrange
            const string returnUrl = "/certificates";

            SetUrlIsLocal(returnUrl, true);

            // Act
            var result = _sut.Check(returnUrl);

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            viewResult.Model.Should().Be(returnUrl);
        }

        [Test]
        public void Check_WithExternalReturnUrl_ShouldReturnViewWithRootReturnUrl()
        {
            // Arrange
            const string returnUrl = "https://malicious.example.com";

            SetUrlIsLocal(returnUrl, false);

            // Act
            var result = _sut.Check(returnUrl);

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            viewResult.Model.Should().Be("/");
        }

        [Test]
        public void Check_WithDefaultReturnUrl_ShouldReturnViewWithRootReturnUrl()
        {
            // Arrange
            SetUrlIsLocal("/", true);

            // Act
            var result = _sut.Check();

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            viewResult.Model.Should().Be("/");
        }

        [Test]
        public void CheckContinue_WithLocalReturnUrl_ShouldRedirectToVerifyIdentity()
        {
            // Arrange
            const string returnUrl = "/certificates";

            SetUrlIsLocal(returnUrl, true);

            var expectedUrl =
                $"{ServiceRoutes.Paths.VerifyIdentity.ServiceControllerPath()}" +
                $"?returnUrl={Uri.EscapeDataString(returnUrl)}";

            // Act
            var result = _sut.CheckContinue(returnUrl);

            // Assert
            var redirectResult = result.Should()
                .BeOfType<RedirectResult>()
                .Subject;

            redirectResult.Url.Should().Be(expectedUrl);
        }

        [Test]
        public void CheckContinue_WithExternalReturnUrl_ShouldUseRootReturnUrl()
        {
            // Arrange
            const string returnUrl = "https://malicious.example.com";

            SetUrlIsLocal(returnUrl, false);

            var expectedUrl =
                $"{ServiceRoutes.Paths.VerifyIdentity.ServiceControllerPath()}" +
                $"?returnUrl={Uri.EscapeDataString("/")}";

            // Act
            var result = _sut.CheckContinue(returnUrl);

            // Assert
            var redirectResult = result.Should()
                .BeOfType<RedirectResult>()
                .Subject;

            redirectResult.Url.Should().Be(expectedUrl);
        }

        [Test]
        public void CheckContinue_WithQueryString_ShouldEncodeReturnUrl()
        {
            // Arrange
            const string returnUrl =
                "/certificates?page=2&status=active";

            SetUrlIsLocal(returnUrl, true);

            var expectedUrl =
                $"{ServiceRoutes.Paths.VerifyIdentity.ServiceControllerPath()}" +
                $"?returnUrl={Uri.EscapeDataString(returnUrl)}";

            // Act
            var result = _sut.CheckContinue(returnUrl);

            // Assert
            var redirectResult = result.Should()
                .BeOfType<RedirectResult>()
                .Subject;

            redirectResult.Url.Should().Be(expectedUrl);
        }

        [Test]
        public async Task Verified_ShouldRedirectToCertificatesList()
        {
            // Act
            var result = await _sut.Verified();

            // Assert
            var redirectResult = result.Should()
                .BeOfType<RedirectToRouteResult>()
                .Subject;

            redirectResult.RouteName.Should()
                .Be(CertificatesController.CertificatesListRouteGet);
        }

        [Test]
        public void Locked_ShouldReturnView()
        {
            // Act
            var result = _sut.Locked();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsTrue_ReturnsExpectedModel()
        {
            // Arrange
            var controller = CreateControllerWithCookies(
                new Dictionary<string, string>
                {
                    { CookieKeys.AnalyticsConsent, "true" }
                });

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeTrue();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsFalse_ReturnsExpectedModel()
        {
            // Arrange
            var controller = CreateControllerWithCookies(
                new Dictionary<string, string>
                {
                    { CookieKeys.AnalyticsConsent, "false" }
                });

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeFalse();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsMissing_ReturnsExpectedModel()
        {
            // Arrange
            var controller = CreateControllerWithCookies();

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeFalse();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void Cookies_WhenAnalyticsConsentCookieIsInvalid_ReturnsExpectedModel()
        {
            // Arrange
            var controller = CreateControllerWithCookies(
                new Dictionary<string, string>
                {
                    { CookieKeys.AnalyticsConsent, "not-a-valid-bool" }
                });

            // Act
            var result = controller.Cookies();

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.ConsentAnalyticsCookie.Should().BeFalse();
            model.BackUrl.Should().BeEmpty();
        }

        [Test]
        public void CookieDetails_ShouldReturnView()
        {
            // Act
            var result = _sut.CookieDetails();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void AccessDenied_ShouldReturnView()
        {
            // Act
            var result = _sut.AccessDenied();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public void Error_ShouldLogErrorAndReturnView()
        {
            // Arrange
            const string errorMessage = "Test error message";

            var httpContext = new DefaultHttpContext
            {
                TraceIdentifier = "TestTraceIdentifier"
            };

            _contextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            // Act
            var result = _sut.Error(errorMessage);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (value, _) =>
                            value.ToString().Contains(errorMessage)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            var model = viewResult.Model.Should()
                .BeOfType<ErrorViewModel>()
                .Subject;

            model.RequestId.Should().Be("TestTraceIdentifier");
            model.ErrorMessage.Should().Be(errorMessage);
        }

        [Test]
        public void AccessibilityStatement_ShouldReturnViewWithPageViewModel()
        {
            // Arrange
            const string returnUrl = "/previous-page";

            SetUrlIsLocal(returnUrl, true);

            // Act
            var result = _sut.AccessibilityStatement(returnUrl);

            // Assert
            var viewResult = result.Should()
                .BeOfType<ViewResult>()
                .Subject;

            viewResult.Model.Should()
                .BeOfType<PageViewModel>();
        }

        private void SetUrlIsLocal(string returnUrl, bool isLocal)
        {
            var urlHelperMock = new Mock<IUrlHelper>();

            urlHelperMock
                .Setup(x => x.IsLocalUrl(returnUrl))
                .Returns(isLocal);

            _sut.Url = urlHelperMock.Object;
        }

        private HomeController CreateControllerWithCookies(
            Dictionary<string, string> cookies = null)
        {
            var requestCookieCollectionMock =
                new Mock<IRequestCookieCollection>();

            if (cookies is not null)
            {
                foreach (var cookie in cookies)
                {
                    requestCookieCollectionMock
                        .Setup(x => x[cookie.Key])
                        .Returns(cookie.Value);
                }
            }

            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Cookies = requestCookieCollectionMock.Object
                }
            };

            _contextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            return new HomeController(
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
        }
    }
}