using Aspose.Pdf;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    public class CertificatesOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<ISessionService> _sessionMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IBlobService> _blobServiceMock;
        private Mock<IAsposeLicenseService> _asposeLicenseServiceMock;
        private DigitalCertificatesWebConfiguration _digitalCertificatesWebConfig;

        private CertificatesOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _sessionMock = new Mock<ISessionService>();
            _userServiceMock = new Mock<IUserService>();
            _blobServiceMock = new Mock<IBlobService>();
            _asposeLicenseServiceMock = new Mock<IAsposeLicenseService>();
            _digitalCertificatesWebConfig = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                MasterPassword = "master-password"
            };           

            _sut = new CertificatesOrchestrator(
                _mediatorMock.Object,
                _sessionMock.Object,
                _userServiceMock.Object,
                _blobServiceMock.Object,
                _asposeLicenseServiceMock.Object,
                _digitalCertificatesWebConfig);
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
        public async Task GetDownloadCertificateViewModelAsync_ReturnsNull_When_MediatorReturnsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetStandardCertificateQueryResult)null);

            // Act
            var result = await _sut.GetDownloadCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetDownloadCertificateViewModelAsync_ReturnsMappedViewModel_When_MediatorReturnsResult()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var dateAwarded = new DateTime(2025, 3, 24);

            var queryResult = new GetStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                FamilyName = "Smith",
                GivenNames = "John Andrew",
                CourseName = "Software Developer",
                CourseOption = "Frontend",
                CourseLevel = 3,
                OverallGrade = "Distinction",
                DateAwarded = dateAwarded,
                CertificateReference = "CERT-12345",
                CoronationEmblem = true
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            var result = await _sut.GetDownloadCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().NotBeNull();
            result.FamilyName.Should().Be(queryResult.FamilyName);
            result.GivenNames.Should().Be(queryResult.GivenNames);
            result.StandardName.Should().Be(queryResult.CourseName);
            result.OptionName.Should().Be(queryResult.CourseOption);
            result.Level.Should().Be(queryResult.CourseLevel.ToString());
            result.Result.Should().Be(queryResult.OverallGrade);
            result.DateAwarded.Should().Be(queryResult.DateAwarded);
            result.CertificateNumber.Should().Be(queryResult.CertificateReference);
            result.CoronationEmblem.Should().Be(queryResult.CoronationEmblem);
        }

        [Test]
        public async Task GenerateCertificateAsync_UsesStandardTemplate_WhenCoronationEmblemIsFalse()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytes();
            var model = CreateModel(coronationEmblem: false);

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync("standard-template"))
                .ReturnsAsync(templateBytes);

            // Act
            var result = await _sut.GenerateCertificateAsync(model);

            // Assert
            result.Should().NotBeNull();          

            _blobServiceMock.Verify(x => x.GetBlobBytesAsync("standard-template"), Times.Once);
            _blobServiceMock.Verify(x => x.GetBlobBytesAsync("green-standard-template"), Times.Never);
        }

        [Test]
        public async Task GenerateCertificateAsync_UsesGreenTemplate_WhenCoronationEmblemIsTrue()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytes();
            var model = CreateModel(coronationEmblem: true);

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync("green-standard-template"))
                .ReturnsAsync(templateBytes);

            // Act
            var result = await _sut.GenerateCertificateAsync(model);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            _blobServiceMock.Verify(x => x.GetBlobBytesAsync("green-standard-template"), Times.Once);
            _blobServiceMock.Verify(x => x.GetBlobBytesAsync("standard-template"), Times.Never);
        }
               
        [Test]
        public async Task GenerateCertificateAsync_FlattensForm()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytes();
            var model = CreateModel();

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync("standard-template"))
                .ReturnsAsync(templateBytes);

            // Act
            var result = await _sut.GenerateCertificateAsync(model);

            // Assert
            using var stream = new MemoryStream(result);
            using var document = new Document(stream);            

            document.Form.Count.Should().Be(0);
        }

        [Test]
        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("  ", false)]
        [TestCase("super-secret", true)]
        public async Task GenerateCertificateAsync_EncryptsPdf_WhenMasterPasswordIsConfigured(string masterPassword, bool isEncrypted)
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytes();
            var model = CreateModel();
            model.CoronationEmblem = false;

            _digitalCertificatesWebConfig.MasterPassword = masterPassword;

            templateBytes.Should().NotBeNull();
            templateBytes.Length.Should().BeGreaterThan(0);

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync(It.IsAny<string>()))
                .ReturnsAsync((string blobName) =>
                {
                    blobName.Should().Be("standard-template");
                    return templateBytes;
                });

            // Act
            var result = await _sut.GenerateCertificateAsync(model);

            using var stream = new MemoryStream(result);
            using var document = new Document(stream);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            document.IsEncrypted.Equals(isEncrypted);
        }

        [Test]
        public async Task GenerateCertificateAsync_ThrowsException_WhenTemplateFieldIsMissing()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytesWithoutAwardedOn();
            var model = CreateModel();

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.StandardTemplateBlobName))
                .ReturnsAsync(templateBytes);

            // Act
            Func<Task> act = async () => await _sut.GenerateCertificateAsync(model);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Message.Should().Be(
                "The PDF template is missing required field(s): Awarded on");
        }

        [Test]
        public async Task GenerateCertificateAsync_ThrowsException_WhenRequiredFieldNamesDoNotMatchTemplate()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytesWithOnlyFullName();
            var model = CreateModel();

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.StandardTemplateBlobName))
                .ReturnsAsync(templateBytes);

            // Act
            Func<Task> act = async () => await _sut.GenerateCertificateAsync(model);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidOperationException>();
            exception.Which.Message.Should().Be(
                "The PDF template is missing required field(s): Passed info, Achieved grade, Awarded on");
        }

        private byte[] CreatePdfTemplateBytesWithOnlyFullName()
        {
            using var document = new Document();
            var page = document.Pages.Add();

            AddTextBoxField(document, page, "Full Name", 100, 700, 300, 730);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private DownloadCertificateViewModel CreateModel(bool coronationEmblem = false)
        {
            return new DownloadCertificateViewModel
            {
                FamilyName = "Smith",
                GivenNames = "John Andrew",
                StandardName = "Software Developer",
                OptionName = "Frontend",
                Level = "3",
                Result = "Distinction",
                DateAwarded = new DateTime(2025, 3, 24),
                CertificateNumber = "CERT-12345",
                CoronationEmblem = coronationEmblem
            };
        }

        [Test]
        public async Task GenerateCertificateAsync_ThrowsException_WhenTemplateFieldIsMissing_ForGreenTemplate()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytesWithoutAwardedOn();
            var model = CreateModel(coronationEmblem: true);

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.GreenStandardTemplateBlobName))
                .ReturnsAsync(templateBytes);

            // Act
            Func<Task> act = async () => await _sut.GenerateCertificateAsync(model);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidOperationException>();

            exception.Which.Message.Should().Be(
                "The PDF template is missing required field(s): Awarded on");

            _blobServiceMock.Verify(
                x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.GreenStandardTemplateBlobName),
                Times.Once);

            _blobServiceMock.Verify(
                x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.StandardTemplateBlobName),
                Times.Never);
        }

        [Test]
        public async Task GenerateCertificateAsync_ThrowsException_WhenRequiredFieldNamesDoNotMatchTemplate_ForGreenTemplate()
        {
            // Arrange
            var templateBytes = CreatePdfTemplateBytesWithOnlyFullName();
            var model = CreateModel(coronationEmblem: true);

            _blobServiceMock
                .Setup(x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.GreenStandardTemplateBlobName))
                .ReturnsAsync(templateBytes);

            // Act
            Func<Task> act = async () => await _sut.GenerateCertificateAsync(model);

            // Assert
            var exception = await act.Should().ThrowAsync<InvalidOperationException>();

            exception.Which.Message.Should().Be(
                "The PDF template is missing required field(s): Passed info, Achieved grade, Awarded on");

            _blobServiceMock.Verify(
                x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.GreenStandardTemplateBlobName),
                Times.Once);

            _blobServiceMock.Verify(
                x => x.GetBlobBytesAsync(_digitalCertificatesWebConfig.StandardTemplateBlobName),
                Times.Never);
        }

        private byte[] CreatePdfTemplateBytes()
        {
            var document = new Document();
            var page = document.Pages.Add();

            AddTextBoxField(document, page, "Full Name", 100, 700, 300, 730);
            AddTextBoxField(document, page, "Passed info", 100, 650, 300, 690);
            AddTextBoxField(document, page, "Achieved grade", 100, 600, 300, 630);
            AddTextBoxField(document, page, "Awarded on", 100, 550, 300, 580);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private byte[] CreatePdfTemplateBytesWithoutAwardedOn()
        {
            using var document = new Document();
            var page = document.Pages.Add();

            AddTextBoxField(document, page, "Full Name", 100, 700, 300, 730);
            AddTextBoxField(document, page, "Passed info", 100, 650, 300, 690);
            AddTextBoxField(document, page, "Achieved grade", 100, 600, 300, 630);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private static void AddTextBoxField(Aspose.Pdf.Document document, Aspose.Pdf.Page page, string fieldName, int llx, int lly, int urx, int ury)
        {
            var field = new Aspose.Pdf.Forms.TextBoxField(page, new Aspose.Pdf.Rectangle(llx, lly, urx, ury))
            {
                PartialName = fieldName
            };

            document.Form.Add(field);
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
    }
}
