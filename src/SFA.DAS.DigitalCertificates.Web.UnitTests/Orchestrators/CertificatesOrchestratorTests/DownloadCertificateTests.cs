using Aspose.Pdf;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators.CertificatesOrchestratorTests
{
    public class DownloadCertificateTests
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

        private static void AddTextBoxField(Document document, Page page, string fieldName, int llx, int lly, int urx, int ury)
        {
            var field = new Aspose.Pdf.Forms.TextBoxField(page, new Rectangle(llx, lly, urx, ury))
            {
                PartialName = fieldName
            };

            document.Form.Add(field);
        }
    }
}
