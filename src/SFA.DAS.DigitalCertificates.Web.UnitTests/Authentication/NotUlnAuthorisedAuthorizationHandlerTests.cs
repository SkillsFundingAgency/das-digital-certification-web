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
    public class NotUlnAuthorisedAuthorizationHandlerTests
    {
        private Mock<ISessionService> _sessionServiceMock;
        private NotUlnAuthorisedAuthorizationHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _sessionServiceMock = new Mock<ISessionService>();
            _sut = new NotUlnAuthorisedAuthorizationHandler(_sessionServiceMock.Object);
        }

        [Test]
        public async Task When_Not_UlnAuthorised_Returns_Succeeds()
        {
            _sessionServiceMock.Setup(x => x.GetUlnAuthorisationAsync()).ReturnsAsync((UlnAuthorisation)null);

            var requirement = new NotUlnAuthorisedRequirement();
            var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

            await _sut.HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();
        }

        [Test]
        public async Task When_UlnAuthorised_Returns_Fails()
        {
            _sessionServiceMock.Setup(x => x.GetUlnAuthorisationAsync()).ReturnsAsync(new UlnAuthorisation { Uln = "123" });

            var requirement = new NotUlnAuthorisedRequirement();
            var context = new AuthorizationHandlerContext(new[] { requirement }, null, null);

            await _sut.HandleAsync(context);

            context.HasFailed.Should().BeTrue();
        }
    }
}
