using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateUserAction
{
    [TestFixture]
    public class CreateUserActionCommandResultTests
    {
        [Test]
        public void ImplicitConversion_From_Response_Maps_ActionCode()
        {
            // Arrange
            var response = new CreateUserActionResponse { ActionCode = "ABC123" };

            // Act
            CreateUserActionCommandResult? result = response;

            // Assert
            result.Should().NotBeNull();
            result!.ActionCode.Should().Be("ABC123");
        }

        [Test]
        public void ImplicitConversion_From_Null_Response_Returns_Null()
        {
            // Arrange & Act
            CreateUserActionCommandResult? result = (CreateUserActionResponse?)null;

            // Assert
            result.Should().BeNull();
        }
    }
}
