using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharingEmail
{
    [TestFixture]
    public class CreateSharingEmailCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private CreateSharingEmailCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreateSharingEmailCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Request_And_Returns_Result()
        {
            // Arrange
            var sharingId = Guid.NewGuid();

            var command = new CreateSharingEmailCommand
            {
                SharingId = sharingId,
                EmailAddress = "user@example.com",
                UserName = "User Name",
                LinkDomain = "https://example.com",
                MessageText = "Here is a message",
                TemplateId = "template-123"
            };

            var expectedResponse = new CreateSharingEmailResponse
            {
                Id = Guid.NewGuid(),
                EmailLinkCode = Guid.NewGuid()
            };

            _outerApiMock
                .Setup(x => x.CreateSharingEmail(It.IsAny<Guid>(), It.IsAny<CreateSharingEmailRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedResponse.Id);
            result.EmailLinkCode.Should().Be(expectedResponse.EmailLinkCode);

            _outerApiMock.Verify(x => x.CreateSharingEmail(
                It.Is<Guid>(g => g == sharingId),
                It.Is<CreateSharingEmailRequest>(r =>
                    r.EmailAddress == command.EmailAddress &&
                    r.UserName == command.UserName &&
                    r.LinkDomain == command.LinkDomain &&
                    r.MessageText == command.MessageText &&
                    r.TemplateId == command.TemplateId
                )), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var command = new CreateSharingEmailCommand
            {
                SharingId = Guid.NewGuid(),
                EmailAddress = "null@example.com",
                UserName = "Null User",
                LinkDomain = "https://null.example",
                MessageText = "",
                TemplateId = "tmpl"
            };

            _outerApiMock
                .Setup(x => x.CreateSharingEmail(It.IsAny<Guid>(), It.IsAny<CreateSharingEmailRequest>()))
                .ReturnsAsync((CreateSharingEmailResponse)null!);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var command = new CreateSharingEmailCommand
            {
                SharingId = Guid.NewGuid(),
                EmailAddress = "fail@example.com",
                UserName = "Fail User",
                LinkDomain = "https://fail.example",
                MessageText = "",
                TemplateId = "tmpl"
            };

            _outerApiMock
                .Setup(x => x.CreateSharingEmail(It.IsAny<Guid>(), It.IsAny<CreateSharingEmailRequest>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
