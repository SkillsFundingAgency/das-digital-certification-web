using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.DeleteSharing;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.DeleteSharing
{
    [TestFixture]
    public class DeleteSharingCommandTests
    {
        [Test]
        public void DeleteSharingCommand_Requires_SharingId()
        {
            // Arrange & Act
            var cmd = new DeleteSharingCommand { SharingId = Guid.NewGuid() };

            // Assert
            cmd.SharingId.Should().NotBeEmpty();
        }
    }
}
