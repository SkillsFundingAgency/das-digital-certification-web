using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class SharedCertificatesControllerTests
    {
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ICertificatesOrchestrator> _certificatesOrchestratorMock;
        private Mock<ISharingOrchestrator> _sharingOrchestratorMock;
        private SharedCertificatesController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _certificatesOrchestratorMock = new Mock<ICertificatesOrchestrator>();
            _sharingOrchestratorMock = new Mock<ISharingOrchestrator>();

            _sut = new SharedCertificatesController(
                _contextAccessorMock.Object,
                _certificatesOrchestratorMock.Object,
                _sharingOrchestratorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task CheckQualification_Redirects_To_Expired_When_SharingInfo_Null()
        {
            // Arrange
            var code = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetCheckQualificationViewModel(code))
                .ReturnsAsync((CheckQualificationViewModel)null!);

            // Act
            var result = await _sut.CheckQualification(code) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(SharedCertificatesController.CheckQualificationExpiredRouteGet);
        }

        [Test]
        public async Task CheckQualification_Returns_View_When_SharingInfo_Found()
        {
            // Arrange
            var code = Guid.NewGuid();

            var model = new CheckQualificationViewModel
            {
                Code = code,
                FormattedExpiry = DateTime.UtcNow.ToString(),
                CertificateId = Guid.NewGuid(),
                CertificateType = CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetCheckQualificationViewModel(code))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.CheckQualification(code) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
        }

        [Test]
        public void CheckQualificationExpired_Returns_View()
        {
            // Act
            var result = _sut.CheckQualificationExpired() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CheckQualificationPost_Redirects_To_Expired_When_SharingInfo_Null()
        {
            // Arrange
            var code = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetCheckQualificationViewModelAndRecordAccess(code))
                .ReturnsAsync((CheckQualificationViewModel)null!);

            // Act
            var result = await _sut.CheckQualificationPost(code) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(SharedCertificatesController.CheckQualificationExpiredRouteGet);
        }

        [Test]
        public async Task CheckQualificationPost_Redirects_To_SharedStandard_When_CertificateType_Standard()
        {
            // Arrange
            var code = Guid.NewGuid();

            var sharingInfo = new CheckQualificationViewModel
            {
                Code = code,
                CertificateType = CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetCheckQualificationViewModelAndRecordAccess(code))
                .ReturnsAsync(sharingInfo);

            // Act
            var result = await _sut.CheckQualificationPost(code) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(SharedCertificatesController.SharedCertificateStandardRouteGet);
            result.RouteValues.Should().ContainKey("sharingLinkCode");
            result.RouteValues["sharingLinkCode"].Should().Be(code);
        }

        [Test]
        public async Task CheckQualificationPost_Redirects_To_SharedFramework_When_CertificateType_Framework()
        {
            // Arrange
            var code = Guid.NewGuid();

            var sharingInfo = new CheckQualificationViewModel
            {
                Code = code,
                CertificateType = CertificateType.Framework
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetCheckQualificationViewModelAndRecordAccess(code))
                .ReturnsAsync(sharingInfo);

            // Act
            var result = await _sut.CheckQualificationPost(code) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(SharedCertificatesController.SharedCertificateFrameworkRouteGet);
            result.RouteValues.Should().ContainKey("sharingLinkCode");
            result.RouteValues["sharingLinkCode"].Should().Be(code);
        }

        [Test]
        public async Task SharedCertificateStandard_Returns_View_When_Model_Found()
        {
            // Arrange
            var code = Guid.NewGuid();
            var model = new SharedCertificateStandardViewModel
            {
                GivenNames = "Given",
                FamilyName = "Family",
                CourseName = "Course",
                FormattedExpiry = DateTime.UtcNow.ToString()
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetSharedStandardCertificateViewModel(code))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.SharedCertificateStandard(code) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
        }

        [Test]
        public async Task SharedCertificateStandard_Redirects_To_Expired_When_Model_Null()
        {
            // Arrange
            var code = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharedStandardCertificateViewModel(code))
                .ReturnsAsync((SharedCertificateStandardViewModel)null!);

            // Act
            var result = await _sut.SharedCertificateStandard(code) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(SharedCertificatesController.CheckQualificationExpiredRouteGet);
        }

        [Test]
        public async Task SharedCertificateFramework_Returns_View_When_Model_Found()
        {
            // Arrange
            var code = Guid.NewGuid();
            var model = new SharedCertificateFrameworkViewModel
            {
                SharingLinkCode = code,
                GivenNames = "Given",
                FamilyName = "Family",
                CourseName = "Course",
                FormattedExpiry = DateTime.UtcNow.ToString()
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetSharedFrameworkCertificateViewModel(code))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.SharedCertificateFramework(code) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
        }

        [Test]
        public async Task SharedCertificateFramework_Redirects_To_Expired_When_Model_Null()
        {
            // Arrange
            var code = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharedFrameworkCertificateViewModel(code))
                .ReturnsAsync((SharedCertificateFrameworkViewModel)null!);

            // Act
            var result = await _sut.SharedCertificateFramework(code) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(SharedCertificatesController.CheckQualificationExpiredRouteGet);
        }

        [Test]
        public async Task DownloadSharedCertificateFrameworkPdf_Throws_InvalidOperationException_When_Model_Is_Null()
        {
            // Arrange
            var sharingLinkCode = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(x => x.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode))
                .ReturnsAsync((DownloadCertificateViewModel)null!);

            // Act
            Func<Task> act = async () => await _sut.DownloadSharedCertificateFrameworkPdf(sharingLinkCode);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(CertificatesController.PdfCertificateCannotBeProduced);

            _sharingOrchestratorMock.Verify(
                x => x.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(It.IsAny<DownloadCertificateViewModel>()),
                Times.Never);
        }

        [Test]
        [TestCase("TestGivenName", "Test", "678123", "TestGivenName_Test_CertificateNumber678123.pdf")]
        [TestCase("John Paul", "Smith", "678123", "John_Paul_Smith_CertificateNumber678123.pdf")]
        [TestCase("John-Paul", "O'Connor", "678123", "John_Paul_O_Connor_CertificateNumber678123.pdf")]
        [TestCase(" John   Paul ", " O'Connor-Smith ", "678123", "John_Paul_O_Connor_Smith_CertificateNumber678123.pdf")]
        [TestCase("Anne.Marie", "Van der Berg", "678123", "Anne_Marie_Van_der_Berg_CertificateNumber678123.pdf")]
        [TestCase("John-Michael", "Smith-Jones", "ABC-123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("John@@Michael", "Smith!!Jones", "ABC###123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("John___Michael", "Smith___Jones", "ABC___123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("John.Michael", "Smith,Jones", "ABC/123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("123", "456", "789", "123_456_CertificateNumber789.pdf")]
        [TestCase("John", "Smith", "ABC-123-XYZ", "John_Smith_CertificateNumberABC_123_XYZ.pdf")]
        [TestCase("!!!John!!!", "@@Smith@@", "##ABC123##", "John_Smith_CertificateNumberABC123.pdf")]
        [TestCase("John & Michael", "Smith & Jones", "ABC & 123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("Jöhn", "Smíth", "ÄBC123", "J_hn_Sm_th_CertificateNumberBC123.pdf")]
        [TestCase("李", "王", "证书123", "__CertificateNumber123.pdf")]
        [TestCase("!!!", "@@@", "###", "__CertificateNumber.pdf")]
        public async Task DownloadSharedCertificateFrameworkPdf_Returns_Pdf_File_When_Model_Found(
            string givenNames,
            string familyName,
            string certificateNumber,
            string expectedFileName)
        {
            // Arrange
            var sharingLinkCode = Guid.NewGuid();
            var pdfBytes = new byte[] { 1, 2, 3, 4 };

            var model = new DownloadCertificateViewModel
            {
                CertificateNumber = certificateNumber,
                FamilyName = familyName,
                GivenNames = givenNames,
                CourseOption = "Software developer",
                CourseLevel = "3",
                OverallGrade = "Pass",
                CourseName = "Test",
                CoronationEmblem = false,
                DateAwarded = DateTime.UtcNow,
                CertificateType = CertificateType.Framework
            };

            _sharingOrchestratorMock
                .Setup(x => x.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode))
                .ReturnsAsync(model);

            _certificatesOrchestratorMock
                .Setup(x => x.GenerateCertificateAsync(model))
                .ReturnsAsync(pdfBytes);

            // Act
            var result = await _sut.DownloadSharedCertificateFrameworkPdf(sharingLinkCode);

            // Assert
            result.Should().BeOfType<FileContentResult>();

            var fileResult = result as FileContentResult;
            fileResult.Should().NotBeNull();
            fileResult!.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be(expectedFileName);
            fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);

            _sharingOrchestratorMock.Verify(
                x => x.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(model),
                Times.Once);
        }

        [Test]
        public async Task DownloadSharedCertificateFrameworkPdf_Throws_InvalidOperationException_When_Pdf_Cannot_Be_Produced()
        {
            // Arrange
            var sharingLinkCode = Guid.NewGuid();

            var model = new DownloadCertificateViewModel
            {
                CertificateNumber = "678123",
                FamilyName = "Test",
                GivenNames = "TestGivenName",
                CourseOption = "Software developer",
                CourseLevel = "3",
                OverallGrade = "Pass",
                CourseName = "Test",
                CoronationEmblem = false,
                DateAwarded = DateTime.UtcNow,
                CertificateType = CertificateType.Framework
            };

            _sharingOrchestratorMock
                .Setup(x => x.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode))
                .ReturnsAsync(model);

            _certificatesOrchestratorMock
                .Setup(x => x.GenerateCertificateAsync(model))
                .ReturnsAsync((byte[])null!);

            // Act
            Func<Task> act = async () => await _sut.DownloadSharedCertificateFrameworkPdf(sharingLinkCode);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(CertificatesController.PdfCertificateCannotBeProduced);

            _sharingOrchestratorMock.Verify(
                x => x.GetDownloadSharedFrameworkCertificateViewModelAsync(sharingLinkCode),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(model),
                Times.Once);
        }

        [Test]
        public async Task DownloadSharedCertificateStandardPdf_Throws_InvalidOperationException_When_Model_Is_Null()
        {
            // Arrange
            var sharingLinkCode = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(x => x.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode))
                .ReturnsAsync((DownloadCertificateViewModel)null!);

            // Act
            Func<Task> act = async () => await _sut.DownloadSharedCertificateStandardPdf(sharingLinkCode);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(CertificatesController.PdfCertificateCannotBeProduced);

            _sharingOrchestratorMock.Verify(
                x => x.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(It.IsAny<DownloadCertificateViewModel>()),
                Times.Never);
        }

        [TestCase("TestGivenName", "Test", "678123", "TestGivenName_Test_CertificateNumber678123.pdf")]
        [TestCase("John Paul", "Smith", "678123", "John_Paul_Smith_CertificateNumber678123.pdf")]
        [TestCase("John-Paul", "O'Connor", "678123", "John_Paul_O_Connor_CertificateNumber678123.pdf")]
        [TestCase(" John   Paul ", " O'Connor-Smith ", "678123", "John_Paul_O_Connor_Smith_CertificateNumber678123.pdf")]
        [TestCase("Anne.Marie", "Van der Berg", "678123", "Anne_Marie_Van_der_Berg_CertificateNumber678123.pdf")]
        [TestCase("John-Michael", "Smith-Jones", "ABC-123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("John@@Michael", "Smith!!Jones", "ABC###123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("John___Michael", "Smith___Jones", "ABC___123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("John.Michael", "Smith,Jones", "ABC/123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("123", "456", "789", "123_456_CertificateNumber789.pdf")]
        [TestCase("John", "Smith", "ABC-123-XYZ", "John_Smith_CertificateNumberABC_123_XYZ.pdf")]
        [TestCase("!!!John!!!", "@@Smith@@", "##ABC123##", "John_Smith_CertificateNumberABC123.pdf")]
        [TestCase("John & Michael", "Smith & Jones", "ABC & 123", "John_Michael_Smith_Jones_CertificateNumberABC_123.pdf")]
        [TestCase("Jöhn", "Smíth", "ÄBC123", "J_hn_Sm_th_CertificateNumberBC123.pdf")]
        [TestCase("李", "王", "证书123", "__CertificateNumber123.pdf")]
        [TestCase("!!!", "@@@", "###", "__CertificateNumber.pdf")]
        public async Task DownloadSharedCertificateStandardPdf_Returns_Pdf_File_When_Model_Found(
        string givenNames,
        string familyName,
        string certificateNumber,
        string expectedFileName)
        {
            // Arrange
            var sharingLinkCode = Guid.NewGuid();
            var pdfBytes = new byte[] { 1, 2, 3, 4 };

            var model = new DownloadCertificateViewModel
            {
                CertificateNumber = certificateNumber,
                FamilyName = familyName,
                GivenNames = givenNames,
                CourseOption = "Software developer",
                CourseLevel = "3",
                OverallGrade = "Pass",
                CourseName = "Test",
                CoronationEmblem = false,
                DateAwarded = DateTime.UtcNow,
                CertificateType = CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(x => x.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode))
                .ReturnsAsync(model);

            _certificatesOrchestratorMock
                .Setup(x => x.GenerateCertificateAsync(model))
                .ReturnsAsync(pdfBytes);

            // Act
            var result = await _sut.DownloadSharedCertificateStandardPdf(sharingLinkCode);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;

            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be(expectedFileName);
            fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);

            _sharingOrchestratorMock.Verify(
                x => x.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(model),
                Times.Once);
        }

        [Test]
        public async Task DownloadSharedCertificateStandardPdf_Throws_InvalidOperationException_When_Pdf_Cannot_Be_Produced()
        {
            // Arrange
            var sharingLinkCode = Guid.NewGuid();

            var model = new DownloadCertificateViewModel
            {
                CertificateNumber = "678123",
                FamilyName = "Test",
                GivenNames = "TestGivenName",
                CourseOption = "Software developer",
                CourseLevel = "3",
                OverallGrade = "Pass",
                CourseName = "Test",
                CoronationEmblem = false,
                DateAwarded = DateTime.UtcNow,
                CertificateType = CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(x => x.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode))
                .ReturnsAsync(model);

            _certificatesOrchestratorMock
                .Setup(x => x.GenerateCertificateAsync(model))
                .ReturnsAsync((byte[])null!);

            // Act
            Func<Task> act = async () => await _sut.DownloadSharedCertificateStandardPdf(sharingLinkCode);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(CertificatesController.PdfCertificateCannotBeProduced);

            _sharingOrchestratorMock.Verify(
                x => x.GetDownloadSharedStandardCertificateViewModelAsync(sharingLinkCode),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(model),
                Times.Once);
        }
    }
}
