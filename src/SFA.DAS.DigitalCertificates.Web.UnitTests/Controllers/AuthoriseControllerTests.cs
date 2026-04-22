using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IAuthoriseOrchestrator> _orchestratorMock;
        private AuthoriseController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionServiceMock = new Mock<ISessionService>();
            _orchestratorMock = new Mock<IAuthoriseOrchestrator>();

            _sut = new AuthoriseController(_contextAccessorMock.Object, _sessionServiceMock.Object, _orchestratorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task NeedMoreInformation_When_NotAuthorised_Prepares_And_Returns_View()
        {
            // Act
            var result = await _sut.NeedMoreInformation();

            // Assert
            _orchestratorMock.Verify(o => o.PrepareNeedMoreInformationAsync(), Times.Once);
            result.Should().BeOfType<ViewResult>();
        }
    }
}
