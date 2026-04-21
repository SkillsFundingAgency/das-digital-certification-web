using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    public class AuthoriseControllerTests
    {
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IAuthoriseOrchestrator> _orchestratorMock;
        [SetUp]
        public void SetUp()
        {
            _sessionServiceMock = new Mock<ISessionService>();
            _orchestratorMock = new Mock<IAuthoriseOrchestrator>();
        }

        [Test]
        public async Task NeedMoreInformation_When_Authorised_Redirects_To_CertificatesList()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetIsUlnAuthorisedAsync())
                .ReturnsAsync(true);

            // Act
            var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
            var sut = new AuthoriseController(httpContextAccessor, _sessionServiceMock.Object, _orchestratorMock.Object);
            var result = await sut.NeedMoreInformation();

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CertificatesListRouteGet);
        }

        [Test]
        public async Task NeedMoreInformation_When_NotAuthorised_Prepares_And_Returns_View()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetIsUlnAuthorisedAsync())
                .ReturnsAsync(false);

            // Act
            var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
            var sut = new AuthoriseController(httpContextAccessor, _sessionServiceMock.Object, _orchestratorMock.Object);
            var result = await sut.NeedMoreInformation();

            // Assert
            _orchestratorMock.Verify(o => o.PrepareNeedMoreInformationAsync(), Times.Once);
            result.Should().BeOfType<ViewResult>();
        }

        
    }
}
