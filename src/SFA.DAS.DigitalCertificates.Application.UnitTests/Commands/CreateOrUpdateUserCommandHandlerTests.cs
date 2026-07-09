using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateOrUpdateUser;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.UnitTests.Application.Commands
{
    [TestFixture]
    public class CreateOrUpdateUserCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private CreateOrUpdateUserCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreateOrUpdateUserCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Request_And_Returns_UserId()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();
            var command = new CreateOrUpdateUserCommand
            {
                GovUkIdentifier = "gov-123",
                EmailAddress = "test@example.com",
                PhoneNumber = "07000111222"
            };

            _outerApiMock
                .Setup(x => x.CreateOrUpdateUser(It.IsAny<CreateOrUpdateUserRequest>()))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedUserId);

            _outerApiMock.Verify(x => x.CreateOrUpdateUser(
                It.Is<CreateOrUpdateUserRequest>(r =>
                    r.GovUkIdentifier == command.GovUkIdentifier &&
                    r.EmailAddress == command.EmailAddress &&
                    r.PhoneNumber == command.PhoneNumber
                )), Times.Once);
        }

        [Test]
        public async Task Handle_Allows_Null_Optional_Fields()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();
            var command = new CreateOrUpdateUserCommand
            {
                GovUkIdentifier = "gov-456",
                EmailAddress = "null@example.com",
                PhoneNumber = null
            };

            _outerApiMock
                .Setup(x => x.CreateOrUpdateUser(It.IsAny<CreateOrUpdateUserRequest>()))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedUserId);

            _outerApiMock.Verify(x => x.CreateOrUpdateUser(
                It.Is<CreateOrUpdateUserRequest>(r =>
                    r.GovUkIdentifier == command.GovUkIdentifier &&
                    r.EmailAddress == command.EmailAddress &&
                    r.PhoneNumber == null
                )), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var command = new CreateOrUpdateUserCommand
            {
                GovUkIdentifier = "gov-error",
                EmailAddress = "fail@example.com"
            };

            _outerApiMock
                .Setup(x => x.CreateOrUpdateUser(It.IsAny<CreateOrUpdateUserRequest>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
