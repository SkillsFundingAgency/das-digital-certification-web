using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class AuthoriseOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IValidator<KnowYourUlnViewModel>> _knowUlnValidatorMock;
        private AuthoriseOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _userServiceMock = new Mock<IUserService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _knowUlnValidatorMock = new Mock<IValidator<KnowYourUlnViewModel>>();

            _sut = new AuthoriseOrchestrator(_mediatorMock.Object, _userServiceMock.Object, _cacheServiceMock.Object, _knowUlnValidatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
            _knowUlnValidatorMock = null;
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_GovUkIdentifier_Is_Null_Does_Not_Call_CacheService()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns((string)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetOrCreateMatchesAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_UserId_Is_Null_Does_Not_Call_CacheService()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns("gov-123");
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetOrCreateMatchesAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_Identifiers_Present_Calls_CacheService()
        {
            // Arrange
            var govUkId = "gov-123";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync((Domain.Models.MatchesAndMasks)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetOrCreateMatchesAsync(govUkId, userId), Times.Once);
        }
    }
}
