using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Authentication
{
    public class UlnAuthorisedAuthorizationHandlerTests
    {
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IUserService> _userServiceMock;
        private UlnAuthorisedAuthorizationHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _sessionServiceMock = new Mock<ISessionService>();
            _userServiceMock = new Mock<IUserService>();
            _sut = new UlnAuthorisedAuthorizationHandler(_sessionServiceMock.Object, _userServiceMock.Object);
        }

        [Test]
        public async Task When_UlnAuthorised_Returns_Succeeds()
        {
            // Arrange
            var govId = "gov-123";
            _userServiceMock.Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionServiceMock.Setup(x => x.GetUlnAuthorisationAsync(govId))
                .ReturnsAsync(new UlnAuthorisation { Uln = "123" });

            var requirement = new UlnAuthorisedRequirement();
            var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

            // Act
            await _sut.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Test]
        public async Task When_Not_UlnAuthorised_Returns_Fails()
        {
            // Arrange
            var govId = "gov-123";
            _userServiceMock.Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionServiceMock.Setup(x => x.GetUlnAuthorisationAsync(govId))
                .ReturnsAsync((UlnAuthorisation)null);

            var requirement = new UlnAuthorisedRequirement();
            var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

            // Act
            await _sut.HandleAsync(context);

            // Assert
            context.HasFailed.Should().BeTrue();
        }
    }
}
