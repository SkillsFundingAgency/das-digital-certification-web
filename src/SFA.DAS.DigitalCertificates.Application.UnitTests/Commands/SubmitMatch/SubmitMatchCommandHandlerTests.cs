using FluentAssertions;
using Moq;
using NUnit.Framework;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.SubmitMatch;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.UnitTests.Application.Commands.SubmitMatch
{
    [TestFixture]
    public class SubmitMatchCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private SubmitMatchCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new SubmitMatchCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Request_And_Returns_Unit()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new SubmitMatchCommand
            {
                UserId = userId,
                Uln = 11122233344L,
                FamilyName = "Smith",
                DateOfBirth = new DateTime(1990,1,1),
                CourseCode = "C1",
                CourseName = "Course One",
                Ukprn = 12345,
                IsMatched = true
            };

            _outerApiMock
                .Setup(x => x.SubmitMatch(userId, It.IsAny<SubmitMatchRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _outerApiMock.Verify(x => x.SubmitMatch(userId, It.Is<SubmitMatchRequest>(r => r.FamilyName == command.FamilyName && r.Uln == command.Uln && r.IsMatched == command.IsMatched)), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new SubmitMatchCommand
            {
                UserId = userId,
                FamilyName = "Smith",
                DateOfBirth = new DateTime(1980,1,1)
            };

            _outerApiMock
                .Setup(x => x.SubmitMatch(userId, It.IsAny<SubmitMatchRequest>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
