using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingAccess;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingAccess
{
    [TestFixture]
    public class CreateSharingAccessCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock = null!;
        private CreateSharingAccessCommandHandler _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreateSharingAccessCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_With_SharingId_Calls_CreateSharingAccess()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var command = new CreateSharingAccessCommand { SharingId = sharingId };

            _outerApiMock.Setup(x => x.CreateSharingAccess(It.IsAny<CreateSharingAccessRequest>())).Returns(Task.CompletedTask);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _outerApiMock.Verify(x => x.CreateSharingAccess(
                It.Is<CreateSharingAccessRequest>(r => r.SharingId == sharingId)), Times.Once);

            _outerApiMock.Verify(x => x.CreateSharingEmailAccess(It.IsAny<CreateSharingEmailAccessRequest>()), Times.Never);
        }

        [Test]
        public async Task Handle_With_SharingEmailId_Calls_CreateSharingEmailAccess()
        {
            // Arrange
            var sharingEmailId = Guid.NewGuid();
            var command = new CreateSharingAccessCommand { SharingEmailId = sharingEmailId };

            _outerApiMock.Setup(x => x.CreateSharingEmailAccess(It.IsAny<CreateSharingEmailAccessRequest>())).Returns(Task.CompletedTask);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _outerApiMock.Verify(x => x.CreateSharingEmailAccess(
                It.Is<CreateSharingEmailAccessRequest>(r => r.SharingEmailId == sharingEmailId)), Times.Once);

            _outerApiMock.Verify(x => x.CreateSharingAccess(It.IsAny<CreateSharingAccessRequest>()), Times.Never);
        }

        [Test]
        public async Task Handle_With_Neither_Id_Does_Not_Call_Api()
        {
            // Arrange
            var command = new CreateSharingAccessCommand();

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _outerApiMock.Verify(x => x.CreateSharingAccess(It.IsAny<CreateSharingAccessRequest>()), Times.Never);
            _outerApiMock.Verify(x => x.CreateSharingEmailAccess(It.IsAny<CreateSharingEmailAccessRequest>()), Times.Never);
        }

        [Test]
        public void Handle_When_Api_Throws_Propagates_Exception()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var command = new CreateSharingAccessCommand { SharingId = sharingId };

            _outerApiMock.Setup(x => x.CreateSharingAccess(It.IsAny<CreateSharingAccessRequest>())).ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
