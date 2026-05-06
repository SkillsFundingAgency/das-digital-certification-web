using FluentAssertions;
using Moq;
using NUnit.Framework;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.AuthoriseUser;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.UnitTests.Application.Commands.AuthoriseUser
{
    [TestFixture]
    public class AuthoriseUserCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private AuthoriseUserCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new AuthoriseUserCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Request_And_Returns_Unit()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AuthoriseUserCommand
            {
                UserId = userId,
                Uln = 11122233344L
            };

            _outerApiMock
                .Setup(x => x.AuthoriseUser(userId, It.IsAny<AuthoriseUserRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _outerApiMock.Verify(x => x.AuthoriseUser(userId, It.Is<AuthoriseUserRequest>(r => r.Uln == command.Uln)), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AuthoriseUserCommand
            {
                UserId = userId,
                Uln = 55566677788L
            };

            _outerApiMock
                .Setup(x => x.AuthoriseUser(userId, It.IsAny<AuthoriseUserRequest>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
