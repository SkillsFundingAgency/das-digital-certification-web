using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingEmail
{
    [TestFixture]
    public class CreateSharingEmailCommandTests
    {
        [Test]
        public void Should_Convert_To_CreateSharingEmailRequest_Successfully()
        {
            // Arrange
            var command = new CreateSharingEmailCommand
            {
                SharingId = Guid.NewGuid(),
                EmailAddress = "user@example.com",
                UserName = "User Name",
                LinkDomain = "https://example.com",
                MessageText = "Hello",
                TemplateId = "template-1"
            };

            // Act
            CreateSharingEmailRequest request = command;

            // Assert
            request.Should().NotBeNull();
            request.EmailAddress.Should().Be(command.EmailAddress);
            request.UserName.Should().Be(command.UserName);
            request.LinkDomain.Should().Be(command.LinkDomain);
            request.MessageText.Should().Be(command.MessageText);
            request.TemplateId.Should().Be(command.TemplateId);
        }
    }
}
