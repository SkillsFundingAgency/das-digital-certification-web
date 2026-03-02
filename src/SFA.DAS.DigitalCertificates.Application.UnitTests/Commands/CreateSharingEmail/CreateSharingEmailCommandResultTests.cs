using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingEmail
{
    [TestFixture]
    public class CreateSharingEmailCommandResultTests
    {
        [Test]
        public void Should_Convert_From_CreateSharingEmailResponse_Successfully()
        {
            // Arrange
            var response = new CreateSharingEmailResponse
            {
                Id = Guid.NewGuid(),
                EmailLinkCode = Guid.NewGuid()
            };

            // Act
            CreateSharingEmailCommandResult? result = response;

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(response.Id);
            result.EmailLinkCode.Should().Be(response.EmailLinkCode);
        }
    }
}
