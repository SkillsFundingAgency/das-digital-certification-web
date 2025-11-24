using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateOrUpdateUser;
using SFA.DAS.DigitalCertificates.Web.Models.Home;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class HomeOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private HomeOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _sut = new HomeOrchestrator(_mediatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task CreateOrUpdateUser_Sends_Command_With_Correct_Values_And_Returns_UserId()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();

            var model = new CreateOrUpdateUserModel
            {
                GovUkIdentifier = "gov-123",
                EmailAddress = "user@example.com",
                PhoneNumber = "07123456789",
                Names = new List<NameModel>
                {
                    new NameModel
                    {
                        ValidSince = new DateTime(1990, 5, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        ValidUntil = null,
                        FamilyName = "Smith",
                        GivenNames = "John"
                    }
                },
                DateOfBirth = new DateTime(1990, 5, 1, 0, 0, 0, DateTimeKind.Unspecified)
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateOrUpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _sut.CreateOrUpdateUser(model);

            // Assert
            result.Should().Be(expectedUserId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateOrUpdateUserCommand>(c =>
                    c.GovUkIdentifier == model.GovUkIdentifier &&
                    c.EmailAddress == model.EmailAddress &&
                    c.PhoneNumber == model.PhoneNumber &&
                    c.DateOfBirth == model.DateOfBirth &&
                    c.Names != null &&
                    c.Names.Count == 1 &&
                    c.Names[0].FamilyName == model.Names[0].FamilyName &&
                    c.Names[0].GivenNames == model.Names[0].GivenNames &&
                    c.Names[0].ValidSince == model.Names[0].ValidSince &&
                    c.Names[0].ValidUntil == model.Names[0].ValidUntil),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateOrUpdateUser_Allows_Empty_Names_List()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();

            var model = new CreateOrUpdateUserModel
            {
                GovUkIdentifier = "gov-456",
                EmailAddress = "no.names@example.com",
                PhoneNumber = "07000000000",
                Names = new List<NameModel>(), // empty
                DateOfBirth = new DateTime(1985, 10, 10, 0, 0, 0, DateTimeKind.Unspecified)
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateOrUpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _sut.CreateOrUpdateUser(model);

            // Assert
            result.Should().Be(expectedUserId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateOrUpdateUserCommand>(c =>
                    c.Names != null && !c.Names.Any() &&
                    c.GovUkIdentifier == model.GovUkIdentifier &&
                    c.EmailAddress == model.EmailAddress &&
                    c.DateOfBirth == model.DateOfBirth),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateOrUpdateUser_Handles_Null_Names_List()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();

            var model = new CreateOrUpdateUserModel
            {
                GovUkIdentifier = "gov-789",
                EmailAddress = "null.names@example.com",
                PhoneNumber = "07999999999",
                Names = null, // null
                DateOfBirth = new DateTime(1975, 2, 15, 0, 0, 0, DateTimeKind.Unspecified)
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateOrUpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _sut.CreateOrUpdateUser(model);

            // Assert
            result.Should().Be(expectedUserId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateOrUpdateUserCommand>(c =>
                    c.Names == null &&
                    c.GovUkIdentifier == model.GovUkIdentifier &&
                    c.EmailAddress == model.EmailAddress &&
                    c.DateOfBirth == model.DateOfBirth),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateOrUpdateUser_Handles_Null_DateOfBirth()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();

            var model = new CreateOrUpdateUserModel
            {
                GovUkIdentifier = "gov-null-dob",
                EmailAddress = "null.dob@example.com",
                PhoneNumber = "07012345678",
                Names = new List<NameModel>
                {
                    new NameModel
                    {
                        ValidSince = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        ValidUntil = null,
                        FamilyName = "Doe",
                        GivenNames = "Jane"
                    }
                },
                DateOfBirth = null
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateOrUpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUserId);

            // Act
            var result = await _sut.CreateOrUpdateUser(model);

            // Assert
            result.Should().Be(expectedUserId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateOrUpdateUserCommand>(c =>
                    c.GovUkIdentifier == model.GovUkIdentifier &&
                    c.EmailAddress == model.EmailAddress &&
                    c.PhoneNumber == model.PhoneNumber &&
                    c.Names!.Count == 1 &&
                    c.Names[0].FamilyName == "Doe" &&
                    c.Names[0].GivenNames == "Jane" &&
                    c.DateOfBirth == null),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
