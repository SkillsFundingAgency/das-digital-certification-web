using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Constants;
using SFA.DAS.DigitalCertificates.Web.Enums;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators.CertificatesOrchestratorTests
{
    public class CertificatesOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ISessionService> _sessionMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IBlobService> _blobServiceMock;
        private Mock<IAsposeLicenseService> _asposeLicenseServiceMock;
        private Mock<IDownloadCertificateService> _downloadCertificateServiceMock;
        private DigitalCertificatesWebConfiguration _digitalCertificatesWebConfig;

        private CertificatesOrchestrator _sut;
        private Mock<IValidator<SelectAddressViewModel>> _selectAddressValidatorMock;
        private Mock<IValidator<AddAddressManualViewModel>> _addAddressValidatorMock;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionMock = new Mock<ISessionService>();
            _userServiceMock = new Mock<IUserService>();
            _blobServiceMock = new Mock<IBlobService>();
            _asposeLicenseServiceMock = new Mock<IAsposeLicenseService>();
            _downloadCertificateServiceMock = new Mock<IDownloadCertificateService>();
            _digitalCertificatesWebConfig = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                OneLoginSettingsUrl = "http://settings.com",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "0",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                FrameworkTemplateBlobName = "framework-template",
                MasterPassword = "master-password",
                StorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "test-container",
                AsposeLicenseContainerName = "test-aspose-container",
                LicenseBlobName = "license-blob",
                NotificationTemplates = new List<NotificationTemplate>
                {
                    new NotificationTemplate { TemplateName = NotificationTemplateNames.PrintRequest, TemplateId = "template-id" }
                }
            };           
            _selectAddressValidatorMock = new Mock<IValidator<SelectAddressViewModel>>();
            _addAddressValidatorMock = new Mock<IValidator<AddAddressManualViewModel>>();

            _httpContext = new DefaultHttpContext();
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);

            _sut = new CertificatesOrchestrator(
                _mediatorMock.Object,
                _contextAccessorMock.Object,
                _sessionMock.Object,
                _userServiceMock.Object,
                _selectAddressValidatorMock.Object,
                _addAddressValidatorMock.Object,
                _blobServiceMock.Object,
                _asposeLicenseServiceMock.Object,
                _digitalCertificatesWebConfig,
                _downloadCertificateServiceMock.Object);
        }

        private void SetupUserClaims(
            string givenNames = "Given",
            string surname = "Fam",
            string email = "user@ex.com",
            string displayName = "Test User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, givenNames),
                new Claim(ClaimTypes.Surname, surname),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, displayName)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            _httpContext.User = new ClaimsPrincipal(identity);
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
                .Setup(x => x.GetOwnedCertificatesAsync())
                .ReturnsAsync(certs);

            // Act
            var result = await _sut.GetCertificatesListViewModel();

            // Assert
            result.Should().NotBeNull();
            result.Certificates.Should().BeEquivalentTo(certs);

            _sessionMock.Verify(x => x.GetOwnedCertificatesAsync(), Times.Once);
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
                .Setup(x => x.GetOwnedCertificatesAsync())
                .ReturnsAsync((List<Certificate>)null);

            // Act
            var result = await _sut.GetCertificatesListViewModel();

            // Assert
            result.Should().NotBeNull();
            result.Certificates.Should().BeNull();

            _sessionMock.Verify(x => x.GetOwnedCertificatesAsync(), Times.Once);
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
                .Setup(x => x.GetOwnedCertificatesAsync())
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
                .Setup(s => s.GetOwnedCertificatesAsync())
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
                .SetupSequence(s => s.GetOwnedCertificatesAsync())
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
                EmployerName = "Employer",
                StartDate = DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = null,
                PrintRequestedBy = null,
                QualificationsAndAwardingBodies = new List<string> { "Q1, A1" },
                DeliveryInformation = new List<DeliveryInformationResponse> { new DeliveryInformationResponse { Id = "D1", Action = "Requested", Status = DeliveryInformationStatuses.Submitted, EventTime = DateTime.UtcNow } }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
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
                .Setup(s => s.GetOwnedCertificatesAsync())
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
            result.EmployerName.Should().Be(mediatorResult.EmployerName);
            result.StartDate.Should().Be(mediatorResult.StartDate);
            result.QualificationsAndAwardingBodies.Should().BeEquivalentTo(mediatorResult.QualificationsAndAwardingBodies);
            result.DeliveryInformation.Should().BeEquivalentTo(mediatorResult.DeliveryInformation);

            result.ShowBackLink.Should().BeTrue();
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_Sets_ShowPrintHeaderTrue_When_StatusDelivered()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "Delivered",
                GivenNames = "D",
                Uln = 2,
                CertificateType = "Framework",
                CourseName = "Course",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow.Date,
                PrintRequestedAt = null,
                PrintRequestedBy = null,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "E1", Action = "Requested", Status = DeliveryInformationStatuses.Delivered, EventTime = DateTime.UtcNow }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.ShowPrintHeader.Should().BeTrue();
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_Sets_ShowPrintHeaderFalse_When_StatusSubmitted()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "Submitted",
                GivenNames = "S",
                Uln = 3,
                CertificateType = "Framework",
                CourseName = "Course",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow.Date,
                PrintRequestedAt = null,
                PrintRequestedBy = null,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "E1", Action = "Requested", Status = DeliveryInformationStatuses.Submitted, EventTime = DateTime.UtcNow }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.ShowPrintHeader.Should().BeFalse();
        }

        [Test]
        public async Task CreateUserActionForNonSpecific_ReturnsReference_When_UserPresent()
        {
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);
            SetupUserClaims(givenNames: "Given", surname: "Fam");

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateUserActionCommandResult { ActionCode = "REF-NON" });

            var result = await _sut.CreateUserActionForNonSpecific();

            result.Should().Be("REF-NON");

            _mediatorMock.Verify(m => m.Send(It.Is<CreateUserActionCommand>(c =>
                    c.UserId == userId &&
                    c.ActionType == ActionType.Contact &&
                    c.FamilyName == "Fam" &&
                    c.GivenNames == "Given"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CreateUserActionForCertificate_ReturnsReference_And_CertificateType_When_UserPresent()
        {
            var userId = Guid.NewGuid();
            var govId = "gov-1";
            var certificateId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);
            SetupUserClaims(givenNames: "G", surname: "F");

            var owned = new List<Certificate>
            {
                new Certificate
                {
                    CertificateId = certificateId,
                    CertificateType = CertificateType.Framework,
                    CourseName = "CourseX",
                    CourseLevel = "1"
                }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateUserActionCommandResult { ActionCode = "REF-CERT" });

            var result = await _sut.CreateUserActionForCertificate(certificateId, ActionType.Help);

            result.ReferenceNumber.Should().Be("REF-CERT");
            result.CertificateType.Should().Be(CertificateType.Framework);

            _mediatorMock.Verify(m => m.Send(It.Is<CreateUserActionCommand>(c =>
                    c.UserId == userId &&
                    c.ActionType == ActionType.Help &&
                    c.CertificateId == certificateId &&
                    c.CertificateType == CertificateType.Framework &&
                    c.CourseName == "CourseX" &&
                    c.FamilyName == "F" &&
                    c.GivenNames == "G"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CreateUserActionForCertificate_ReturnsEmpty_When_CertificateNotOwned()
        {
            var userId = Guid.NewGuid();
            var govId = "gov-1";
            var certificateId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            var result = await _sut.CreateUserActionForCertificate(certificateId, ActionType.Help);

            result.ReferenceNumber.Should().BeNull();
            result.CertificateType.Should().Be(CertificateType.Unknown);

            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetContactUsViewModel_ReturnsNull_When_ReferenceEmpty()
        {
            var result = await _sut.GetContactUsViewModel(string.Empty, null);

            result.Should().BeNull();
        }

        [Test]
        public async Task CreatePrintRequest_UsesDeliveryAddress_When_DeliveryAddressPresent_AndMediatorCalled()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var deliveryAddress = new CheckAndSubmitViewModel
            {
                Organisation = "DelOrg",
                AddressLine1 = "Del1",
                AddressLine2 = "Del2",
                TownOrCity = "DelTown",
                County = "DelCounty",
                Postcode = "DEL123"
            };

            _sessionMock.Setup(s => s.GetDeliveryAddressAsync()).ReturnsAsync(deliveryAddress);
            SetupUserClaims(email: "user@ex.com", displayName: "Test User");

            CreatePrintRequestCommand sentCommand = null!;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreatePrintRequestCommand>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((c, _) => sentCommand = (CreatePrintRequestCommand)c)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.CreatePrintRequest(certificateId);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreatePrintRequestCommand>(), It.IsAny<CancellationToken>()), Times.Once);

            sentCommand.Should().NotBeNull();
            sentCommand!.Request.Address.ContactOrganisation.Should().Be(deliveryAddress.Organisation);
            sentCommand.Request.Address.ContactAddLine1.Should().Be(deliveryAddress.AddressLine1);
            sentCommand.Request.Address.ContactAddLine2.Should().Be(deliveryAddress.AddressLine2);
            sentCommand.Request.Address.ContactAddLine3.Should().Be(deliveryAddress.TownOrCity);
            sentCommand.Request.Address.ContactAddLine4.Should().Be(deliveryAddress.County);
            sentCommand.Request.Address.ContactPostCode.Should().Be(deliveryAddress.Postcode);
            sentCommand.Request.Email.EmailAddress.Should().Be("user@ex.com");
            sentCommand.Request.Email.UserName.Should().Be("Test User");
        }

        [Test]
        public async Task GetContactUsViewModel_SetsCertificateType_When_CertificateProvided()
        {
            var govId = "gov-2";
            var certificateId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govId);

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = "Unknown", CourseLevel = "1" }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);

            var model = await _sut.GetContactUsViewModel("REF-1", certificateId);

            model.Should().NotBeNull();
            model!.ReferenceNumber.Should().Be("REF-1");
            model.CertificateType.Should().Be(CertificateType.Standard);
        }

        [Test]
        public async Task GetPrintRequestConfirmationViewModel_SetsCourseName_When_CertificateFound()
        {
            var certificateId = Guid.NewGuid();
            var courseName = "Confirmed Course";

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = courseName, CourseLevel = "1" }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);

            var result = await _sut.GetPrintRequestConfirmationViewModel(certificateId);

            result.Should().NotBeNull();
            result.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be(courseName);
        }

        [Test]
        public async Task GetSelectAddressViewModel_ReturnsNull_When_CertificateMissing()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _sessionMock
                .Setup(s => s.GetOwnedCertificatesAsync())
                .ReturnsAsync((List<Certificate>)null);

            // Act
            var result = await _sut.GetSelectAddressViewModel(certificateId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetSelectAddressViewModel_PopulatesFields_When_CertificateFoundAndUserClaimsPresent()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var courseName = "Course X";

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = courseName, CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);
            SetupUserClaims(givenNames: "UserGiven", surname: "UserFamily");

            // Act
            var result = await _sut.GetSelectAddressViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be(courseName);
            result.GivenNames.Should().Be("UserGiven");
            result.FamilyName.Should().Be("UserFamily");
            result.SearchTerm.Should().BeNullOrEmpty();
        }

        [Test]
        public async Task GetAddAddressViewModel_ReturnsNull_When_CertificateMissing()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _sessionMock
                .Setup(s => s.GetOwnedCertificatesAsync())
                .ReturnsAsync((List<Certificate>)null);

            // Act
            var result = await _sut.GetAddAddressViewModel(certificateId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetAddAddressViewModel_PopulatesFields_When_CertificateFoundAndUserClaimsPresent()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var courseName = "Course Y";

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = courseName, CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);
            SetupUserClaims(givenNames: "UserGiven", surname: "UserFamily");

            // Act
            var result = await _sut.GetAddAddressViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be(courseName);
            result.GivenNames.Should().Be("UserGiven");
            result.FamilyName.Should().Be("UserFamily");
        }

        [Test]
        public async Task GetCheckAndSubmitViewModel_PopulatesFields_When_CertificateFoundAndSessionAddressPresent()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var courseName = "Course Z";

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = courseName, CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);
            SetupUserClaims(givenNames: "UserGiven", surname: "UserFamily");

            var addr = new CheckAndSubmitViewModel
            {
                BackRoute = "addAddress",
                Organisation = "Org",
                AddressLine1 = "L1",
                AddressLine2 = "L2",
                TownOrCity = "Town",
                County = "County",
                Postcode = "PC1"
            };

            _sessionMock.Setup(s => s.GetDeliveryAddressAsync()).ReturnsAsync(addr);

            // Act
            var result = await _sut.GetCheckAndSubmitViewModel(certificateId, "CertificateStandardRouteGet");

            // Assert
            result.Should().NotBeNull();
            result!.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be(courseName);
            result.GivenNames.Should().Be("UserGiven");
            result.FamilyName.Should().Be("UserFamily");
            result.BackRoute.Should().Be(addr.BackRoute);
            result.Organisation.Should().Be(addr.Organisation);
            result.AddressLine1.Should().Be(addr.AddressLine1);
            result.TownOrCity.Should().Be(addr.TownOrCity);
            result.Postcode.Should().Be(addr.Postcode);
        }

        [Test]
        public async Task GetCheckAndSubmitViewModel_UsesDefaultBackRoute_When_NoSessionAddress()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var courseName = "Course Default";

            var owned = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = courseName, CourseLevel = "1", DateAwarded = DateTime.UtcNow }
            };

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(owned);
            SetupUserClaims(givenNames: "UserGiven", surname: "UserFamily");

            _sessionMock.Setup(s => s.GetDeliveryAddressAsync()).ReturnsAsync((CheckAndSubmitViewModel)null);

            var defaultBack = "defaultRoute";

            // Act
            var result = await _sut.GetCheckAndSubmitViewModel(certificateId, defaultBack);

            // Assert
            result.Should().NotBeNull();
            result!.BackRoute.Should().Be(defaultBack);
        }

        [Test]
        public async Task StoreDeliveryAddressFromLocationAsync_SetsSessionAndReturnsTrue_When_MatchFound()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var selectedName = "Match Address";
            var backRoute = "select";

            var locationsResult = new GetLocationsQueryResult
            {
                Locations = new[]
                {
                    new LocationResult
                    {
                        Name = selectedName,
                        Organisation = "Org",
                        AddressLine1 = "L1",
                        AddressLine2 = "L2",
                        PostTown = "Town",
                        County = "County",
                        Postcode = "PC1"
                    }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetLocationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(locationsResult);

            // Act
            var result = await _sut.StoreDeliveryAddressFromLocationAsync(certificateId, selectedName, backRoute);

            // Assert
            result.Should().BeTrue();

            _sessionMock.Verify(s => s.SetDeliveryAddressAsync(It.Is<CheckAndSubmitViewModel>(c =>
                    c.Organisation == "Org" &&
                    c.AddressLine1 == "L1" &&
                    c.Postcode == "PC1" &&
                    c.BackRoute == backRoute)),
                Times.Once);
        }

        [Test]
        public async Task StoreDeliveryAddressFromLocationAsync_ReturnsFalse_When_NoMatchFound()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var selectedName = "No Match";
            var backRoute = "select";

            var locationsResult = new GetLocationsQueryResult
            {
                Locations = new[]
                {
                    new LocationResult { Name = "Other Address", Organisation = "Org" }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetLocationsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(locationsResult);

            // Act
            var result = await _sut.StoreDeliveryAddressFromLocationAsync(certificateId, selectedName, backRoute);

            // Assert
            result.Should().BeFalse();

            _sessionMock.Verify(s => s.SetDeliveryAddressAsync(It.IsAny<CheckAndSubmitViewModel>()), Times.Never);
        }

        [Test]
        public async Task CreateUserActionForCertificate_ReturnsEmpty_When_UserIdNull()
        {
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            var result = await _sut.CreateUserActionForCertificate(Guid.NewGuid(), ActionType.Help);

            result.ReferenceNumber.Should().BeNull();
            result.CertificateType.Should().Be(CertificateType.Unknown);

            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetCertificateStandardViewModel_Sets_PrintStatus_Printed_And_ShowPrintHeaderTrue()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var eventTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var mediatorResult = new GetStandardCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Standard",
                CourseName = "Course",
                CourseLevel = 1,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "1", Action = "Printed", Status = DeliveryInformationStatuses.Printed, EventTime = eventTime }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetStandardCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.PrintStatus.Should().Be(PrintStatus.Printed);
            result.PrintStatusDate.Should().Be(eventTime);
            result.PrintStatusMessage.Should().Contain("printed on");
            result.ShowPrintHeader.Should().BeTrue();
            result.PrintStatusDisplay.Should().Be(PrintStatus.Printed.ToString());
        }

        [Test]
        public async Task GetCertificateStandardViewModel_Sets_PrintStatus_Requested_And_DisplayIsPrintRequested()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var eventTime = new DateTime(2024, 5, 15, 0, 0, 0, DateTimeKind.Utc);

            var mediatorResult = new GetStandardCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Standard",
                CourseName = "Course",
                CourseLevel = 1,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "1", Action = "Requested", Status = DeliveryInformationStatuses.PrintRequested, EventTime = eventTime }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetStandardCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.PrintStatus.Should().Be(PrintStatus.Requested);
            result.PrintStatusDisplay.Should().Be("Print requested");
            result.ShowPrintHeader.Should().BeTrue();
            result.PrintStatusMessage.Should().Contain("requested on");
        }

        [Test]
        public async Task GetCertificateStandardViewModel_Sets_ShowRequestPrintTrue_When_StatusSubmittedAndNoPrintRequestedAt()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var mediatorResult = new GetStandardCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Standard",
                CourseName = "Course",
                CourseLevel = 1,
                PrintRequestedAt = null,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "1", Action = "Submitted", Status = DeliveryInformationStatuses.Submitted, EventTime = DateTime.UtcNow }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetStandardCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.ShowRequestPrint.Should().BeTrue();
        }

        [Test]
        public async Task GetCertificateStandardViewModel_Sets_PrintStatusNone_When_DeliveryInformationEmpty()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var mediatorResult = new GetStandardCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Standard",
                CourseName = "Course",
                CourseLevel = 1,
                DeliveryInformation = new List<DeliveryInformationResponse>()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetStandardCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateStandardViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.PrintStatus.Should().Be(PrintStatus.None);
            result.ShowPrintHeader.Should().BeFalse();
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_Sets_PrintStatusNone_When_AllEventsBeforeCutoverDate()
        {
            // Arrange
            _digitalCertificatesWebConfig.CutoverDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var certificateId = Guid.NewGuid();

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Framework",
                CourseName = "Course",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "1", Action = "Submitted", Status = DeliveryInformationStatuses.Submitted, EventTime = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc) }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFrameworkCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.PrintStatus.Should().Be(PrintStatus.None);
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_Sets_PrintStatus_Delivered_WithCorrectMessage()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var eventTime = new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc);

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Framework",
                CourseName = "Course",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "1", Action = "Delivered", Status = DeliveryInformationStatuses.Delivered, EventTime = eventTime }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFrameworkCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.PrintStatus.Should().Be(PrintStatus.Delivered);
            result.PrintStatusDate.Should().Be(eventTime);
            result.PrintStatusMessage.Should().Contain("delivered on");
            result.ShowPrintHeader.Should().BeTrue();
            result.PrintStatusCssClass.Should().Contain("delivered");
        }

        [Test]
        public async Task GetCertificateFrameworkViewModel_Sets_PrintStatus_Requested_For_SentToPrinter()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var eventTime = new DateTime(2024, 4, 10, 0, 0, 0, DateTimeKind.Utc);

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "A",
                GivenNames = "B",
                Uln = 1,
                CertificateType = "Framework",
                CourseName = "Course",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow,
                DeliveryInformation = new List<DeliveryInformationResponse>
                {
                    new DeliveryInformationResponse { Id = "1", Action = "Sent", Status = DeliveryInformationStatuses.SentToPrinter, EventTime = eventTime }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFrameworkCertificateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _sessionMock.Setup(s => s.GetOwnedCertificatesAsync()).ReturnsAsync(new List<Certificate>());

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.PrintStatus.Should().Be(PrintStatus.Requested);
            result.PrintStatusDisplay.Should().Be("Print requested");
            result.PrintStatusMessage.Should().Contain("requested on");
        }

        [Test]
        public async Task GetDownloadFrameworkCertificateViewModelAsync_ReturnsNull_When_MediatorReturnsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetFrameworkCertificateQueryResult)null);

            // Act
            var result = await _sut.GetDownloadFrameworkCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().BeNull();

            _downloadCertificateServiceMock.Verify(s => s.CreateDownloadCertificateViewModel(
                It.IsAny<DownloadCertificateRequestViewModel>()),
                Times.Never);
        }

        [Test]
        public async Task GetDownloadFrameworkCertificateViewModelAsync_ReturnsViewModel_When_ResultIsValid()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var awardedDate = DateTime.UtcNow.Date;

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "Smith",
                GivenNames = "John",
                CertificateType = "Framework",
                CourseName = "Cloud Developer",
                CourseOption = "Option A",
                CourseLevel = "3",
                DateAwarded = awardedDate,
                FrameworkCertificateNumber = "123456"
            };

            var expectedViewModel = new DownloadCertificateViewModel
            {
                FamilyName = "Smith",
                GivenNames = "John",
                CourseName = "Cloud Developer",
                CourseOption = "Option A",
                CourseLevel = "3",
                DateAwarded = awardedDate,
                CertificateNumber = "123456",
                CertificateType = CertificateType.Framework
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _downloadCertificateServiceMock
                .Setup(s => s.CreateDownloadCertificateViewModel(
                    It.Is<DownloadCertificateRequestViewModel>(r =>
                        r.CertificateId == certificateId
                        && r.CertificateType == CertificateType.Framework
                        && r.FamilyName == "Smith"
                        && r.GivenNames == "John"
                        && r.CourseName == "Cloud Developer"
                        && r.CourseOption == "Option A"
                        && r.CourseLevel == "3"
                        && r.DateAwarded == awardedDate
                        && r.CertificateNumber == "123456")))
                .Returns(expectedViewModel);

            // Act
            var result = await _sut.GetDownloadFrameworkCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().BeSameAs(expectedViewModel);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId),
                It.IsAny<CancellationToken>()), Times.Once);

            _downloadCertificateServiceMock.Verify(s => s.CreateDownloadCertificateViewModel(
                It.IsAny<DownloadCertificateRequestViewModel>()), Times.Once);
        }

        [Test]
        public async Task GetDownloadFrameworkCertificateViewModelAsync_ThrowsInvalidOperationException_When_DownloadServiceThrows()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var mediatorResult = new GetFrameworkCertificateQueryResult
            {
                FamilyName = null,
                GivenNames = "John",
                CertificateType = "Framework",
                CourseName = "Cloud Developer",
                CourseOption = "Option A",
                CourseLevel = "3",
                DateAwarded = DateTime.UtcNow.Date,
                FrameworkCertificateNumber = "9876543"
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            _downloadCertificateServiceMock
                .Setup(s => s.CreateDownloadCertificateViewModel(
                    It.IsAny<DownloadCertificateRequestViewModel>()))
                .Throws(new InvalidOperationException(
                    $"Certificate {certificateId} is missing required data."));

            // Act
            Func<Task> act = async () =>
                await _sut.GetDownloadFrameworkCertificateViewModelAsync(certificateId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage($"Certificate {certificateId} is missing required data.");

            _downloadCertificateServiceMock.Verify(s => s.CreateDownloadCertificateViewModel(
                It.Is<DownloadCertificateRequestViewModel>(r =>
                    r.CertificateId == certificateId
                    && r.CertificateType == CertificateType.Framework
                    && r.FamilyName == null
                    && r.GivenNames == "John"
                    && r.CourseName == "Cloud Developer"
                    && r.CourseOption == "Option A"
                    && r.CourseLevel == "3"
                    && r.CertificateNumber == "9876543")),
                Times.Once);
        }
    }
}
