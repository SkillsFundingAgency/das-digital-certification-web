using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.DeleteSharing;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.DeleteSharing
{
    [TestFixture]
    public class DeleteSharingCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private DeleteSharingCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new DeleteSharingCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_DeleteSharing_With_Correct_Id()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var command = new DeleteSharingCommand { SharingId = sharingId };

            _outerApiMock.Setup(x => x.DeleteSharing(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _outerApiMock.Verify(x => x.DeleteSharing(It.Is<Guid>(g => g == sharingId)), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var command = new DeleteSharingCommand { SharingId = sharingId };

            _outerApiMock.Setup(x => x.DeleteSharing(It.IsAny<Guid>())).ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
