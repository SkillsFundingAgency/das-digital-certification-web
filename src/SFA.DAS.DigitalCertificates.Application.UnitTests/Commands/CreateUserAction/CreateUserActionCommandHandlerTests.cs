using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateUserAction
{
    [TestFixture]
    public class CreateUserActionCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private CreateUserActionCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreateUserActionCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Args_And_Returns_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var command = new CreateUserActionCommand
            {
                UserId = userId,
                ActionType = Domain.Models.ActionType.Help,
                FamilyName = "F",
                GivenNames = "G",
                CertificateId = Guid.NewGuid(),
                CertificateType = Domain.Models.CertificateType.Standard,
                CourseName = "Course"
            };

            var expectedResponse = new CreateUserActionResponse { ActionCode = "REF-1" };

            _outerApiMock.Setup(x => x.CreateUserAction(userId, It.IsAny<CreateUserActionRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.ActionCode.Should().Be("REF-1");

            _outerApiMock.Verify(x => x.CreateUserAction(userId, It.Is<CreateUserActionRequest>(r => r.ActionType == command.ActionType.ToString() && r.FamilyName == "F" && r.GivenNames == "G" && r.CertificateId == command.CertificateId && r.CertificateType == "Standard" && r.CourseName == "Course")), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var command = new CreateUserActionCommand { UserId = userId, ActionType = Domain.Models.ActionType.Contact, FamilyName = "F", GivenNames = "G" };

            _outerApiMock.Setup(x => x.CreateUserAction(userId, It.IsAny<CreateUserActionRequest>()))
                .ReturnsAsync((CreateUserActionResponse)null!);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
