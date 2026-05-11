using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetUserActions
{
    [TestFixture]
    public class GetUserActionsQueryResultTests
    {
        [Test]
        public void Implicit_Operator_Maps_Response_To_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new GetUserActionsResponse
            {
                UserActions =
                {
                    new UserActionResponse
                    {
                        Id = 10,
                        UserId = userId,
                        ActionType = ActionType.NotMatched.ToString(),
                        ActionTime = DateTime.UtcNow,
                        ActionStatus = UserActionStatus.New.ToString(),
                        FamilyName = "Jones",
                        GivenNames = "Anna",
                        CertificateId = Guid.NewGuid(),
                        CertificateType = CertificateType.Framework.ToString(),
                        CourseName = "Framework Course",
                        ActionCode = "ABC123",
                        AdminActions = new System.Collections.Generic.List<AdminActionResponse>
                        {
                            new AdminActionResponse
                            {
                                Username = "op",
                                ActionTime = DateTime.UtcNow,
                                Action = AdminActionType.Unlocked.ToString()
                            }
                        }
                    }
                }
            };

            // Act
            GetUserActionsQueryResult? result = response;

            // Assert
            result.Should().NotBeNull();
            result!.UserActions.Should().HaveCount(1);
            var ua = result.UserActions![0];
            ua.Id.Should().Be(10);
            ua.UserId.Should().Be(userId);
            ua.ActionType.Should().Be(ActionType.NotMatched);
            ua.ActionStatus.Should().Be(UserActionStatus.New);
            ua.FamilyName.Should().Be("Jones");
            ua.GivenNames.Should().Be("Anna");
            ua.CertificateId.Should().NotBeNull();
            ua.CertificateType.Should().Be(CertificateType.Framework);
            ua.CourseName.Should().Be("Framework Course");
            ua.ActionCode.Should().Be("ABC123");
            ua.AdminActions.Should().NotBeNull();
            ua.AdminActions.Should().HaveCount(1);
            ua.AdminActions.Should().ContainSingle(a => a.Username == "op" && a.Action == AdminActionType.Unlocked);
        }
    }
}
