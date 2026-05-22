using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateUserAction
{
    [TestFixture]
    public class CreateUserActionCommandTests
    {
        [Test]
        public void ImplicitConversion_To_Request_Maps_Fields()
        {
            // Arrange
            var command = new CreateUserActionCommand
            {
                UserId = Guid.NewGuid(),
                ActionType = ActionType.Contact,
                FamilyName = "Fam",
                GivenNames = "Given",
                CertificateId = Guid.NewGuid(),
                CertificateType = CertificateType.Framework,
                CourseName = "Course X"
            };

            // Act
            var request = (CreateUserActionRequest)command;

            // Assert
            request.ActionType.Should().Be("Contact");
            request.FamilyName.Should().Be("Fam");
            request.GivenNames.Should().Be("Given");
            request.CertificateId.Should().Be(command.CertificateId);
            request.CertificateType.Should().Be("Framework");
            request.CourseName.Should().Be("Course X");
        }
    }
}
