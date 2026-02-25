using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    public class CertificatesOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<ISessionService> _sessionMock;
        private Mock<IUserService> _userServiceMock;

        private CertificatesOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _sessionMock = new Mock<ISessionService>();
            _userServiceMock = new Mock<IUserService>();
            _sut = new CertificatesOrchestrator(
                _mediatorMock.Object,
                _sessionMock.Object,
                _userServiceMock.Object);
        }

        [Test]
        public async Task When_CertificatesExist_Then_ReturnsViewModelWithCertificates()
        {
            // Arrange
            var govId = "gov-123";
            var certs = new List<Certificate>
            {
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "Bricklayer", CourseLevel = "1", DateAwarded = DateTime.Now }
            };

            _userServiceMock
                .Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionMock
                .Setup(x => x.GetOwnedCertificatesAsync(govId))
                .ReturnsAsync(certs);

            // Act
            var result = await _sut.GetCertificatesListViewModel();

            // Assert
            result.Should().NotBeNull();
            result.Certificates.Should().BeEquivalentTo(certs);

            _sessionMock.Verify(x => x.GetOwnedCertificatesAsync(govId), Times.Once);
        }

        [Test]
        public async Task When_NoCertificates_Then_ReturnsViewModelWithNullCertificates()
        {
            // Arrange
            var govId = "gov-123";

            _userServiceMock
                .Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionMock
                .Setup(x => x.GetOwnedCertificatesAsync(govId))
                .ReturnsAsync((List<Certificate>)null);

            // Act
            var result = await _sut.GetCertificatesListViewModel();

            // Assert
            result.Should().NotBeNull();
            result.Certificates.Should().BeNull();

            _sessionMock.Verify(x => x.GetOwnedCertificatesAsync(govId), Times.Once);
        }

        [Test]
        public async Task Mediator_IsNotUsed()
        {
            // Arrange
            var govId = "gov-123";

            _userServiceMock
                .Setup(x => x.GetGovUkIdentifier())
                .Returns(govId);

            _sessionMock
                .Setup(x => x.GetOwnedCertificatesAsync(govId))
                .ReturnsAsync(new List<Certificate>());

            // Act
            await _sut.GetCertificatesListViewModel();

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<object>(), default), Times.Never);
        }

        [Test]
        public async Task GetCertificateStandardViewModel_ReturnsNull_When_MediatorReturnsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetStandardCertificateQueryResult)null);

            // Act
            var result = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetCertificateStandardViewModel_MapsFields_And_ShowBackLinkTrue_When_OwnedMoreThanOne()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var govId = "gov-456";

            var mediatorResult = new GetStandardCertificateQueryResult
            {
                FamilyName = "Smith",
                GivenNames = "John",
                Uln = 123456,
                CertificateType = "Standard",
                CertificateReference = "ABC123",
                CourseCode = "C1",
                CourseName = "Bricklayer",
                CourseOption = "Opt",
                CourseLevel = 2,
                DateAwarded = DateTime.UtcNow.Date,
                OverallGrade = "Pass",
                ProviderName = "Provider",
                Ukprn = "10000000",
                EmployerName = "Employer",
                AssessorName = "Assessor",
                StartDate = DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = null,
                PrintRequestedBy = null
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _userServiceMock
                .Setup(u => u.GetGovUkIdentifier())
                .Returns(govId);

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "A", CourseLevel = "1", DateAwarded = DateTime.UtcNow },
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Framework, CourseName = "B", CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock
                .Setup(s => s.GetOwnedCertificatesAsync(govId))
                .ReturnsAsync(owned);

            // Act
            var result = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(mediatorResult.FamilyName);
            result.GivenNames.Should().Be(mediatorResult.GivenNames);
            result.Uln.Should().Be(mediatorResult.Uln);
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.CertificateReference.Should().Be(mediatorResult.CertificateReference);
            result.CourseCode.Should().Be(mediatorResult.CourseCode);
            result.CourseName.Should().Be(mediatorResult.CourseName);
            result.CourseOption.Should().Be(mediatorResult.CourseOption);
            result.CourseLevel.Should().Be(mediatorResult.CourseLevel);
            result.DateAwarded.Should().Be(mediatorResult.DateAwarded);
            result.OverallGrade.Should().Be(mediatorResult.OverallGrade);
            result.ProviderName.Should().Be(mediatorResult.ProviderName);
            result.Ukprn.Should().Be(mediatorResult.Ukprn);
            result.EmployerName.Should().Be(mediatorResult.EmployerName);
            result.AssessorName.Should().Be(mediatorResult.AssessorName);
            result.StartDate.Should().Be(mediatorResult.StartDate);

            result.ShowBackLink.Should().BeTrue();
        }

        [Test]
        public async Task GetCertificateStandardViewModel_Sets_ShowBackLinkFalse_When_OwnedSingleOrNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var govId = "gov-789";

            var mediatorResult = new GetStandardCertificateQueryResult
            {
                FamilyName = "Jones",
                GivenNames = "Amy",
                Uln = 654321,
                CertificateType = "Standard",
                CourseName = "Plumber",
                CourseLevel = 3
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetStandardCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _userServiceMock
                .Setup(u => u.GetGovUkIdentifier())
                .Returns(govId);

            var single = new List<Certificate>
            {
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "Only", CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock
                .SetupSequence(s => s.GetOwnedCertificatesAsync(govId))
                .ReturnsAsync(single)
                .ReturnsAsync((List<Certificate>)null);

            // Act
            var resultSingle = await _sut.GetCertificateStandardViewModel(certificateId);
            var resultNull = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            resultSingle.Should().NotBeNull();
            resultSingle!.ShowBackLink.Should().BeFalse();

            resultNull.Should().NotBeNull();
            resultNull!.ShowBackLink.Should().BeFalse();
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_ReturnsNull_When_MediatorReturnsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetFrameworkCertificateQueryResult)null);

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_MapsFields_And_ShowBackLinkTrue_When_OwnedMoreThanOne()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var govId = "gov-456";

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "Smith",
                GivenNames = "John",
                Uln = 123456,
                CertificateType = "Framework",
                FrameworkCertificateNumber = "FW-1",
                CourseName = "Bricklayer",
                CourseOption = "Opt",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow.Date,
                ProviderName = "Provider",
                Ukprn = 10000000,
                EmployerName = "Employer",
                StartDate = DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = null,
                PrintRequestedBy = null,
                QualificationsAndAwardingBodies = new System.Collections.Generic.List<string> { "Q1, A1" },
                DeliveryInformation = new System.Collections.Generic.List<string> { "D1" }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _userServiceMock
                .Setup(u => u.GetGovUkIdentifier())
                .Returns(govId);

            var owned = new System.Collections.Generic.List<Certificate>
            {
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Standard, CourseName = "A", CourseLevel = "1", DateAwarded = DateTime.UtcNow },
                new Certificate { CertificateId = Guid.NewGuid(), CertificateType = CertificateType.Framework, CourseName = "B", CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock
                .Setup(s => s.GetOwnedCertificatesAsync(govId))
                .ReturnsAsync(owned);

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(mediatorResult.FamilyName);
            result.GivenNames.Should().Be(mediatorResult.GivenNames);
            result.Uln.Should().Be(mediatorResult.Uln);
            result.CertificateType.Should().Be(CertificateType.Framework);
            result.FrameworkCertificateNumber.Should().Be(mediatorResult.FrameworkCertificateNumber);
            result.CourseName.Should().Be(mediatorResult.CourseName);
            result.CourseOption.Should().Be(mediatorResult.CourseOption);
            result.CourseLevel.Should().Be(mediatorResult.CourseLevel);
            result.DateAwarded.Should().Be(mediatorResult.DateAwarded);
            result.ProviderName.Should().Be(mediatorResult.ProviderName);
            result.Ukprn.Should().Be(mediatorResult.Ukprn);
            result.EmployerName.Should().Be(mediatorResult.EmployerName);
            result.StartDate.Should().Be(mediatorResult.StartDate);
            result.QualificationsAndAwardingBodies.Should().BeEquivalentTo(mediatorResult.QualificationsAndAwardingBodies);
            result.DeliveryInformation.Should().BeEquivalentTo(mediatorResult.DeliveryInformation);

            result.ShowBackLink.Should().BeTrue();
        }

        [Test]
        public async Task CreateOrReuseUserActionForNonSpecific_ReturnsReference_When_UserPresent()
        {
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);
            _sessionMock.Setup(s => s.GetUserDetailsAsync()).ReturnsAsync(new UserDetails { FamilyName = "Fam", GivenNames = "Given" });

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateUserActionCommandResult { ActionCode = "REF-NON" });

            var result = await _sut.CreateOrReuseUserActionForNonSpecific();

            result.Should().Be("REF-NON");

            _mediatorMock.Verify(m => m.Send(It.Is<CreateUserActionCommand>(c => c.UserId == userId && c.ActionType == ActionType.Contact && c.FamilyName == "Fam" && c.GivenNames == "Given"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CreateOrReuseUserActionForCertificate__ReturnsReference_When_UserPresent()
        {
            var userId = Guid.NewGuid();
            var govId = "gov-1";
            var certificateId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);

            _sessionMock.Setup(s => s.GetUserDetailsAsync()).ReturnsAsync(new Domain.Models.UserDetails { FamilyName = "F", GivenNames = "G" });

            var owned = new List<Certificate> { new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Framework, CourseName = "CourseX", CourseLevel = "1" } };
            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync(govId)).ReturnsAsync(owned);

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new CreateUserActionCommandResult { ActionCode = "REF-CERT" });

            var result = await _sut.CreateOrReuseUserActionForCertificate(certificateId);

            result.Should().Be("REF-CERT");

            _mediatorMock.Verify(m => m.Send(It.Is<CreateUserActionCommand>(c => c.UserId == userId && c.ActionType == ActionType.Help && c.CertificateId == certificateId && c.CertificateType == CertificateType.Framework && c.CourseName == "CourseX"), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetContactUsViewModel_ReturnsNull_When_ReferenceEmpty()
        {
            var result = await _sut.GetContactUsViewModel(string.Empty, null);
            result.Should().BeNull();
        }

        [Test]
        public async Task GetContactUsViewModel_SetsCertificateType_When_CertificateProvided()
        {
            var govId = "gov-2";
            var certificateId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);

            var owned = new List<Certificate> { new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = "Unknown", CourseLevel = "1" } };
            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync(govId)).ReturnsAsync(owned);

            var model = await _sut.GetContactUsViewModel("REF-1", certificateId);

            model.Should().NotBeNull();
            model!.ReferenceNumber.Should().Be("REF-1");
            model.CertificateType.Should().Be(CertificateType.Standard);
        }
    }
}
