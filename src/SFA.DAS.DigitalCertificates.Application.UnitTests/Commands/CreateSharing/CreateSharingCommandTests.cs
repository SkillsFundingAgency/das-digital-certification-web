using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharing
{
    [TestFixture]
    public class CreateSharingCommandTests
    {
        [Test]
        public void Should_Convert_To_CreateSharingRequest_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();

            var command = new CreateSharingCommand
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Software Developer"
            };

            // Act
            CreateSharingRequest request = command;

            // Assert
            request.Should().NotBeNull();
            request.Userid.Should().Be(userId);
            request.CertificateId.Should().Be(certificateId);
            request.CertificateType.Should().Be("Standard");
            request.CourseName.Should().Be("Software Developer");
        }
    }
}