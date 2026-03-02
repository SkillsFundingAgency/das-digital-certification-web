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
        public async Task Handle_Calls_CreateSharingAccess()
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
        }

        [Test]
        public async Task Handle_When_Api_Throws_Propagates_Exception()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var command = new CreateSharingAccessCommand { SharingId = sharingId };

            _outerApiMock.Setup(x => x.CreateSharingAccess(It.IsAny<CreateSharingAccessRequest>())).ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
