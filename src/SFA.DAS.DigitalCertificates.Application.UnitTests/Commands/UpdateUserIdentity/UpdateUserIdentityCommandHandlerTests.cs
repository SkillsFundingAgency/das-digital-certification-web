using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.UpdateUserIdentity;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.UpdateUserIdentity
{
    [TestFixture]
    public class UpdateUserIdentityCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock = null!;
        private UpdateUserIdentityCommandHandler _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();

            _sut = new UpdateUserIdentityCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Request_And_Returns_Unit()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var command = new UpdateUserIdentityCommand
            {
                UserId = userId,
                DateOfBirth = new DateTime(1990, 1, 2, 0, 0, 0, DateTimeKind.Unspecified),
                Names = new List<Name>
                {
                    new()
                    {
                        FamilyName = "Smith",
                        GivenNames = "John",
                        ValidSince = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        ValidUntil = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
                    }
                }
            };

            _outerApiMock
                .Setup(x => x.UpdateUserIdentity(
                    It.IsAny<Guid>(),
                    It.IsAny<UpdateUserIdentityRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            _outerApiMock.Verify(x => x.UpdateUserIdentity(
                userId,
                It.Is<UpdateUserIdentityRequest>(r =>
                    r.DateOfBirth == command.DateOfBirth &&
                    r.Names != null &&
                    r.Names.Count == 1 &&
                    r.Names[0].FamilyName == "Smith" &&
                    r.Names[0].GivenNames == "John" &&
                    r.Names[0].ValidSince == command.Names[0].ValidSince &&
                    r.Names[0].ValidUntil == command.Names[0].ValidUntil)),
                Times.Once);
        }

        [Test]
        public async Task Handle_Allows_Null_Names()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var command = new UpdateUserIdentityCommand
            {
                UserId = userId,
                DateOfBirth = new DateTime(1985, 5, 10, 0, 0, 0, DateTimeKind.Unspecified),
                Names = null
            };

            _outerApiMock
                .Setup(x => x.UpdateUserIdentity(
                    It.IsAny<Guid>(),
                    It.IsAny<UpdateUserIdentityRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            _outerApiMock.Verify(x => x.UpdateUserIdentity(
                userId,
                It.Is<UpdateUserIdentityRequest>(r =>
                    r.DateOfBirth == command.DateOfBirth &&
                    r.Names == null)),
                Times.Once);
        }

        [Test]
        public async Task Handle_Allows_Empty_Names_List()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var command = new UpdateUserIdentityCommand
            {
                UserId = userId,
                DateOfBirth = new DateTime(1970, 3, 15, 0, 0, 0, DateTimeKind.Unspecified),
                Names = new List<Name>()
            };

            _outerApiMock
                .Setup(x => x.UpdateUserIdentity(
                    It.IsAny<Guid>(),
                    It.IsAny<UpdateUserIdentityRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);

            _outerApiMock.Verify(x => x.UpdateUserIdentity(
                userId,
                It.Is<UpdateUserIdentityRequest>(r =>
                    r.DateOfBirth == command.DateOfBirth &&
                    r.Names != null &&
                    r.Names.Count == 0)),
                Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var command = new UpdateUserIdentityCommand
            {
                UserId = Guid.NewGuid(),
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                Names = new List<Name>
                {
                    new()
                    {
                        FamilyName = "Smith",
                        GivenNames = "John"
                    }
                }
            };

            _outerApiMock
                .Setup(x => x.UpdateUserIdentity(
                    It.IsAny<Guid>(),
                    It.IsAny<UpdateUserIdentityRequest>()))
                .ThrowsAsync(new Exception("Outer API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>()
                .WithMessage("Outer API failure");
        }
    }
}