using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmailAccess;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingEmailAccess
{
    [TestFixture]
    public class CreateSharingEmailAccessCommandTests
    {
        [Test]
        public void Should_Convert_To_CreateSharingEmailAccessRequest_Successfully()
        {
            // Arrange
            var sharingEmailId = Guid.NewGuid();

            var command = new CreateSharingEmailAccessCommand
            {
                SharingEmailId = sharingEmailId
            };

            // Act
            CreateSharingEmailAccessRequest request = command;

            // Assert
            request.Should().NotBeNull();
            request.SharingEmailId.Should().Be(sharingEmailId);
        }
    }
}
