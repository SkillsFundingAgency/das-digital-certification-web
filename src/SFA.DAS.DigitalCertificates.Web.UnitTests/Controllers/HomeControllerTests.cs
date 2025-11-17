using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.TestHelper.Extensions;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Models;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Services;

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
    }
}