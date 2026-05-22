using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetUserActions
{
    [TestFixture]
    public class GetUserActionsQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetUserActionsQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetUserActionsQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Mapped_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var response = new GetUserActionsResponse
            {
                UserActions =
                {
                    new UserActionResponse
                    {
                        Id = 1,
                        UserId = userId,
                        ActionType = ActionType.NotFound.ToString(),
                        ActionTime = DateTime.UtcNow,
                        ActionStatus = UserActionStatus.New.ToString(),
                        FamilyName = "Smith",
                        GivenNames = "John",
                        CertificateId = null,
                        CertificateType = CertificateType.Standard.ToString(),
                        CourseName = "Course A",
                        ActionCode = "REF123",
                        AdminActions = new List<AdminActionResponse>
                        {
                            new AdminActionResponse
                            {
                                Username = "admin",
                                ActionTime = DateTime.UtcNow,
                                Action = AdminActionType.Viewed.ToString()
                            }
                        }
                    }
                }
            };

            _outerApiMock
                .Setup(x => x.GetUserActions(userId))
                .ReturnsAsync(response);

            var request = new GetUserActionsQuery { UserId = userId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserActions.Should().HaveCount(1);
            var ua = result.UserActions![0];
            ua.Id.Should().Be(response.UserActions[0].Id);
            ua.UserId.Should().Be(response.UserActions[0].UserId);
            ua.ActionType.Should().Be(ActionType.NotFound);
            ua.ActionStatus.Should().Be(UserActionStatus.New);
            ua.FamilyName.Should().Be(response.UserActions[0].FamilyName);
            ua.GivenNames.Should().Be(response.UserActions[0].GivenNames);
            ua.CertificateType.Should().Be(CertificateType.Standard);
            ua.CourseName.Should().Be(response.UserActions[0].CourseName);
            ua.ActionCode.Should().Be(response.UserActions[0].ActionCode);
            ua.AdminActions.Should().NotBeNull();

            _outerApiMock.Verify(x => x.GetUserActions(userId), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetUserActions(userId))
                .ReturnsAsync((GetUserActionsResponse)null!);

            var request = new GetUserActionsQuery { UserId = userId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _outerApiMock.Verify(x => x.GetUserActions(userId), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetUserActions(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var request = new GetUserActionsQuery { UserId = userId };

            // Act
            Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
