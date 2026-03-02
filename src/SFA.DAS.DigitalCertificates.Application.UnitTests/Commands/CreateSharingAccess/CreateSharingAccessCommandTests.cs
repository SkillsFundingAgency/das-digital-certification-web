using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingAccess;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingAccess
{
    [TestFixture]
    public class CreateSharingAccessCommandTests
    {
        [Test]
        public void Should_Convert_To_CreateSharingAccessRequest_Successfully()
        {
            // Arrange
            var sharingId = Guid.NewGuid();

            var command = new CreateSharingAccessCommand
            {
                SharingId = sharingId
            };

            // Act
            CreateSharingAccessRequest request = command;

            // Assert
            request.Should().NotBeNull();
            request.SharingId.Should().Be(sharingId);
        }
    }
}
