using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.RequestPrintCertificate
{
    [TestFixture]
    public class CreatePrintRequestCommandTests
    {
        [Test]
        public void RequestProperty_Maps_Fields()
        {
            // Arrange
            var request = new CreatePrintRequest
            {
                Address = new PrintAddressDto { ContactName = "Name", ContactPostCode = "PC1" },
                Email = new PrintEmailDto { EmailAddress = "email@ex.com", UserName = "User" }
            };

            var command = new CreatePrintRequestCommand
            {
                CertificateId = Guid.NewGuid(),
                Request = request
            };

            // Act
            var req = command.Request;

            // Assert
            req.Should().BeSameAs(request);
            req.Address.ContactName.Should().Be("Name");
            req.Email.EmailAddress.Should().Be("email@ex.com");
        }
    }
}
