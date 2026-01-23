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
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class CertificatesControllerTests
    {
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ICertificatesOrchestrator> _certificatesOrchestratorMock;
        private Mock<ISharingOrchestrator> _sharingOrchestratorMock;
        private CertificatesController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _certificatesOrchestratorMock = new Mock<ICertificatesOrchestrator>();
            _sharingOrchestratorMock = new Mock<ISharingOrchestrator>();
            _sut = new CertificatesController(
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
        public async Task CertificatesList_Returns_View()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var certs = new List<Certificate>
            {
                new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = "Test Course", CourseLevel = "1" }
            };

            var viewModel = new CertificatesListViewModel
            {
                Certificates = certs
            };

            _certificatesOrchestratorMock
                .Setup(x => x.GetCertificatesListViewModel())
                .ReturnsAsync(viewModel);

            // Act
            var result = await _sut.CertificatesList() as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _certificatesOrchestratorMock.Verify(x => x.GetCertificatesListViewModel(), Times.Once);
        }

        [Test]
        public async Task Certificate_Returns_View()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var model = new CertificateStandardViewModel
            {
                CertificateId = certificateId,
                CourseName = "Test Course",
                CertificateType = CertificateType.Standard,
                ShowBackLink = false
            };

            _certificatesOrchestratorMock
                .Setup(c => c.GetCertificateStandardViewModel(certificateId))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.CertificateStandard(certificateId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
            _certificatesOrchestratorMock.Verify(c => c.GetCertificateStandardViewModel(certificateId), Times.Once);
        }

        [Test]
        public async Task CertificateFramework_Returns_View()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var model = new CertificateFrameworkViewModel
            {
                CertificateId = certificateId,
                CourseName = "Test Course",
                CertificateType = Domain.Models.CertificateType.Framework,
                ShowBackLink = false
            };

            _certificatesOrchestratorMock
                .Setup(c => c.GetCertificateFrameworkViewModel(certificateId))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.CertificateFramework(certificateId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
            _certificatesOrchestratorMock.Verify(c => c.GetCertificateFrameworkViewModel(certificateId), Times.Once);
        }

        [Test]
        public async Task CreateCertificateSharing_Get_Returns_View_With_Model()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var model = new CreateCertificateSharingViewModel
            {
                CertificateId = certificateId,
                CourseName = "Test Course",
                CertificateType = Domain.Models.CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetSharings(certificateId))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.CreateCertificateSharing(certificateId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
            _sharingOrchestratorMock.Verify(s => s.GetSharings(certificateId), Times.Once);
        }

        [Test]
        public async Task CreateCertificateSharingPost_Post_Calls_Orchestrator_And_Redirects()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var returnedSharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.CreateSharing(certificateId))
                .ReturnsAsync(returnedSharingId);

            // Act
            var result = await _sut.CreateCertificateSharingPost(certificateId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CertificateSharingLinkRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _sharingOrchestratorMock.Verify(s => s.CreateSharing(certificateId), Times.Once);
        }

        [Test]
        public async Task CertificateSharingLink_Returns_View_When_Model_Valid()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            var model = new CertificateSharingLinkViewModel
            {
                CertificateId = certificateId,
                CourseName = "Course",
                CertificateType = Domain.Models.CertificateType.Standard,
                SharingId = sharingId,
                SharingNumber = 1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                LinkCode = Guid.NewGuid(),
                FormattedExpiry = DateTime.UtcNow.AddMinutes(5).ToString(),
                FormattedCreated = DateTime.UtcNow.AddMinutes(-5).ToString(),
                FormattedAccessTimes = new System.Collections.Generic.List<string>()
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.CertificateSharingLink(certificateId, sharingId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
        }

        [Test]
        public async Task CertificateSharingLink_Redirects_To_SharingList_When_Model_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync((CertificateSharingLinkViewModel)null!);

            // Act
            var result = await _sut.CertificateSharingLink(certificateId, sharingId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CreateCertificateSharingRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
        }

        [Test]

        public async Task CertificateSharingLink_Redirects_To_SharingList_When_Model_Expired()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync((CertificateSharingLinkViewModel)null!);

            // Act
            var result = await _sut.CertificateSharingLink(certificateId, sharingId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CreateCertificateSharingRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
        }

        [Test]
        public async Task CertificatesList_Returns_View_When_MultipleCertificates()
        {
            // Arrange
            var cert1 = Guid.NewGuid();
            var cert2 = Guid.NewGuid();

            var certs = new List<Certificate>
            {
                new Certificate { CertificateId = cert1, CertificateType = CertificateType.Standard, CourseName = "Course A", CourseLevel = "1" },
                new Certificate { CertificateId = cert2, CertificateType = CertificateType.Framework, CourseName = "Course B", CourseLevel = "2" }
            };

            var viewModel = new CertificatesListViewModel
            {
                Certificates = certs
            };

            _certificatesOrchestratorMock
                .Setup(x => x.GetCertificatesListViewModel())
                .ReturnsAsync(viewModel);

            // Act
            var result = await _sut.CertificatesList() as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(viewModel);
            _certificatesOrchestratorMock.Verify(x => x.GetCertificatesListViewModel(), Times.Once);
        }

        [Test]
        public async Task ShareByEmail_Redirects_To_SharingList_When_Sharing_Not_Found()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync((CertificateSharingLinkViewModel)null!);

            var model = new ShareByEmailViewModel { EmailAddress = "test@example.com" };

            // Act
            var result = await _sut.ShareByEmail(certificateId, sharingId, model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CreateCertificateSharingRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
        }

        [Test]
        public async Task ShareByEmail_Redirects_Back_When_Model_Invalid()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync(new CertificateSharingLinkViewModel { CourseName = "Course" });

            _sharingOrchestratorMock
                .Setup(s => s.ValidateShareByEmailViewModel(It.IsAny<ShareByEmailViewModel>(), It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(false);

            var model = new ShareByEmailViewModel { EmailAddress = "" };

            // Act
            var result = await _sut.ShareByEmail(certificateId, sharingId, model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CertificateSharingLinkRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
            result.RouteValues["sharingId"].Should().Be(sharingId);
            result.RouteValues["emailAddress"].Should().Be(string.Empty);
        }

        [Test]
        public async Task ShareByEmail_Redirects_To_Confirm_When_Model_Valid()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync(new CertificateSharingLinkViewModel { CourseName = "Course" });

            _sharingOrchestratorMock
                .Setup(s => s.ValidateShareByEmailViewModel(It.IsAny<ShareByEmailViewModel>(), It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>()))
                .ReturnsAsync(true);

            var model = new ShareByEmailViewModel { EmailAddress = "user@example.com" };

            // Act
            var result = await _sut.ShareByEmail(certificateId, sharingId, model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.ConfirmShareByEmailRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
            result.RouteValues["sharingId"].Should().Be(sharingId);
            result.RouteValues["emailAddress"].Should().Be(model.EmailAddress);
        }

        [Test]
        public async Task ConfirmShareByEmail_Returns_Redirect_When_Model_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetConfirmShareByEmail(certificateId, sharingId, It.IsAny<string>()))
                .ReturnsAsync((ConfirmShareByEmailViewModel)null!);

            // Act
            var result = await _sut.ConfirmShareByEmail(certificateId, sharingId, "test@example.com") as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CreateCertificateSharingRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
        }

        [Test]
        public async Task ConfirmShareByEmail_Returns_View_When_Model_Found()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            var model = new ConfirmShareByEmailViewModel
            {
                CertificateId = certificateId,
                SharingId = sharingId,
                CourseName = "Course",
                SharingNumber = 1,
                EmailAddress = "user@example.com"
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetConfirmShareByEmail(certificateId, sharingId, It.IsAny<string>()))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.ConfirmShareByEmail(certificateId, sharingId, model.EmailAddress) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
        }

        [Test]
        public async Task ConfirmShareByEmailPost_Redirects_To_EmailSent()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var sharingEmailId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.CreateSharingEmail(certificateId, sharingId, It.IsAny<string>()))
                .ReturnsAsync(sharingEmailId);

            var model = new ConfirmShareByEmailViewModel { EmailAddress = "user@example.com" };

            // Act
            var result = await _sut.ConfirmShareByEmailPost(certificateId, sharingId, model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.EmailSentRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
            result.RouteValues["sharingId"].Should().Be(sharingId);
            result.RouteValues["sharingEmailId"].Should().Be(sharingEmailId);
        }

        [Test]
        public async Task EmailSent_Redirects_To_SharingList_When_Model_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var sharingEmailId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetEmailSent(certificateId, sharingId, sharingEmailId))
                .ReturnsAsync((EmailSentViewModel)null!);

            // Act
            var result = await _sut.EmailSent(certificateId, sharingId, sharingEmailId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CreateCertificateSharingRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
        }

        [Test]
        public async Task EmailSent_Returns_View_When_Model_Found()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var sharingEmailId = Guid.NewGuid();

            var model = new EmailSentViewModel
            {
                CertificateId = certificateId,
                SharingId = sharingId,
                SharingNumber = 1,
                EmailAddress = "user@example.com",
                CourseName = "Course",
                FormattedExpiry = DateTime.UtcNow.ToString(),
                IsSingleCertificate = true
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetEmailSent(certificateId, sharingId, sharingEmailId))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.EmailSent(certificateId, sharingId, sharingEmailId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
        }
    }
}