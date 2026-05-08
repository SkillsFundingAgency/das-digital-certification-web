using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.AuthoriseUser;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.UnitTests.Application.Commands.AuthoriseUser
{
    [TestFixture]
    public class AuthoriseUserCommandTests
    {
        [Test]
        public void Implicit_Conversion_To_AuthoriseUserRequest_Maps_Uln()
        {
            // Arrange
            var cmd = new AuthoriseUserCommand
            {
                UserId = Guid.NewGuid(),
                Uln = 1234567890L
            };

            // Act
            AuthoriseUserRequest req = cmd;

            // Assert
            req.Should().NotBeNull();
            req.Uln.Should().Be(cmd.Uln);
        }
    }
}
