using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Services;
using System.Net.Http;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    public class HomeControllerBackUrlTests
    {
        private Mock<IHomeOrchestrator> _orchestratorMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IGovUkAuthenticationService> _govUkAuthServiceMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _sut;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _orchestratorMock = new Mock<IHomeOrchestrator>();
            _configMock = new Mock<IConfiguration>();
            _govUkAuthServiceMock = new Mock<IGovUkAuthenticationService>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<HomeController>>();

            _httpContext = new DefaultHttpContext();
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);

            _sut = new HomeController(
                _orchestratorMock.Object,
                _configMock.Object,
                _govUkAuthServiceMock.Object,
                _contextAccessorMock.Object,
                _loggerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();

        [Test]
        public void Cookies_WhenReturnUrlIsProvided_SetsBackUrlOnModel()
        {
            // Arrange
            const string returnUrl = "/previous-page";

            // Act
            var result = _sut.Cookies(returnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.BackUrl.Should().Be(returnUrl);
        }

        [Test]
        public void Cookies_WhenReturnUrlIsNotProvided_SetsBackUrlToNull()
        {
            // Act
            var result = _sut.Cookies();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<CookiesViewModel>()
                .Subject;

            model.BackUrl.Should().BeNull();
        }

        [Test]
        public void Help_WhenReturnUrlIsProvided_SetsBackUrlOnModel()
        {
            // Arrange
            const string returnUrl = "/previous-page";

            // Act
            var result = _sut.Help(returnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<PageViewModel>()
                .Subject;

            model.BackUrl.Should().Be(returnUrl);
        }

        [Test]
        public void Help_WhenReturnUrlIsNotProvided_SetsBackUrlToNull()
        {
            // Act
            var result = _sut.Help();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<PageViewModel>()
                .Subject;

            model.BackUrl.Should().BeNull();
        }

        [Test]
        public void AccessibilityStatement_WhenReturnUrlIsProvided_SetsBackUrlOnModel()
        {
            // Arrange
            const string returnUrl = "/previous-page";

            // Act
            var result = _sut.AccessibilityStatement(returnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<PageViewModel>()
                .Subject;

            model.BackUrl.Should().Be(returnUrl);
        }

        [Test]
        public void AccessibilityStatement_WhenReturnUrlIsNotProvided_SetsBackUrlToNull()
        {
            // Act
            var result = _sut.AccessibilityStatement();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            var model = viewResult.Model.Should()
                .BeOfType<PageViewModel>()
                .Subject;

            model.BackUrl.Should().BeNull();
        }

    }
}
