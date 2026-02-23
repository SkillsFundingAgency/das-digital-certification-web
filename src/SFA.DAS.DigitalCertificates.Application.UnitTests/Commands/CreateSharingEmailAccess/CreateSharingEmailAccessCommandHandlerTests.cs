using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmailAccess;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingEmailAccess
{
    [TestFixture]
    public class CreateSharingEmailAccessCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock = null!;
        private CreateSharingEmailAccessCommandHandler _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreateSharingEmailAccessCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_CreateSharingEmailAccess()
        {
            // Arrange
            var sharingEmailId = Guid.NewGuid();
            var command = new CreateSharingEmailAccessCommand { SharingEmailId = sharingEmailId };

            _outerApiMock.Setup(x => x.CreateSharingEmailAccess(It.IsAny<CreateSharingEmailAccessRequest>())).Returns(Task.CompletedTask);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _outerApiMock.Verify(x => x.CreateSharingEmailAccess(
                It.Is<CreateSharingEmailAccessRequest>(r => r.SharingEmailId == sharingEmailId)), Times.Once);
        }

        [Test]
        public async Task Handle_When_Api_Throws_Propagates_Exception()
        {
            // Arrange
            var sharingEmailId = Guid.NewGuid();
            var command = new CreateSharingEmailAccessCommand { SharingEmailId = sharingEmailId };

            _outerApiMock.Setup(x => x.CreateSharingEmailAccess(It.IsAny<CreateSharingEmailAccessRequest>())).ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
