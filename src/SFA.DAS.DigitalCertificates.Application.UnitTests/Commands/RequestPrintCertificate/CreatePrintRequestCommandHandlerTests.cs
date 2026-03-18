using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.RequestPrintCertificate
{
    [TestFixture]
    public class CreatePrintRequestCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private CreatePrintRequestCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreatePrintRequestCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_CallsOuterApi_WithSuppliedParameters()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var request = new CreatePrintRequest
            {
                Address = new PrintAddressDto { ContactName = "Name", ContactPostCode = "PC1" },
                Email = new PrintEmailDto { EmailAddress = "email@ex.com", UserName = "User", LinkDomain = "http://localhost", TemplateId = "template-id" }
            };

            var command = new CreatePrintRequestCommand
            {
                CertificateId = certificateId,
                Request = request
            };

            _outerApiMock.Setup(x => x.CreatePrintRequest(It.IsAny<Guid>(), It.IsAny<CreatePrintRequest>())).Returns(Task.CompletedTask);

            // Act
            await _sut.Handle(command, CancellationToken.None);

            // Assert
            _outerApiMock.Verify(x => x.CreatePrintRequest(certificateId, request), Times.Once);
        }
    }
}
