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
using SFA.DAS.DigitalCertificates.Web.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.DigitalCertificates.Web.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class CertificateSharingControllerTests
    {
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ISharingOrchestrator> _sharingOrchestratorMock;
        private Mock<ISessionService> _sessionServiceMock;
        private CertificateSharingController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _sharingOrchestratorMock = new Mock<ISharingOrchestrator>();
            _sessionServiceMock = new Mock<ISessionService>();

            _sut = new CertificateSharingController(
                _contextAccessorMock.Object,
                _sharingOrchestratorMock.Object,
                _sessionServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
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
            result!.RouteName.Should().Be(CertificateSharingController.CertificateSharingLinkRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _sharingOrchestratorMock.Verify(s => s.CreateSharing(certificateId), Times.Once);
            _sessionServiceMock.Verify(s => s.ClearShareEmailAsync(), Times.Once);
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
                FormattedExpiry = DateTime.UtcNow.AddMinutes(5).ToString()
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
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
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
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
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
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
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
            result!.RouteName.Should().Be(CertificateSharingController.CertificateSharingLinkRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
            result.RouteValues["sharingId"].Should().Be(sharingId);
            result.RouteValues["emailAddress"].Should().Be(string.Empty);

            _sessionServiceMock.Verify(s => s.SetShareEmailAsync(string.Empty), Times.Never);
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
            result!.RouteName.Should().Be(CertificateSharingController.ConfirmShareByEmailRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
            result.RouteValues["sharingId"].Should().Be(sharingId);

            _sessionServiceMock.Verify(s => s.SetShareEmailAsync(model.EmailAddress), Times.Once);
        }

        [Test]
        public async Task ConfirmShareByEmail_Returns_Redirect_When_Model_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var email = "test@example.com";

            _sharingOrchestratorMock
                .Setup(s => s.GetConfirmShareByEmail(certificateId, sharingId, It.IsAny<string>()))
                .ReturnsAsync((ConfirmShareByEmailViewModel)null!);

            _sessionServiceMock
                .Setup(s => s.GetShareEmailAsync())
                .ReturnsAsync(email);

            // Act
            var result = await _sut.ConfirmShareByEmail(certificateId, sharingId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);

            _sessionServiceMock.Verify(s => s.GetShareEmailAsync(), Times.Once);
        }

        [Test]
        public async Task ConfirmShareByEmail_Returns_View_When_Model_Found()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var email = "user@example.com";

            var model = new ConfirmShareByEmailViewModel
            {
                CertificateId = certificateId,
                SharingId = sharingId,
                CourseName = "Course",
                SharingNumber = 1,
                EmailAddress = email
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetConfirmShareByEmail(certificateId, sharingId, It.IsAny<string>()))
                .ReturnsAsync(model);

            _sessionServiceMock
                .Setup(s => s.GetShareEmailAsync())
                .ReturnsAsync(email);

            // Act
            var result = await _sut.ConfirmShareByEmail(certificateId, sharingId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);

            _sessionServiceMock.Verify(s => s.GetShareEmailAsync(), Times.Once);
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
            result!.RouteName.Should().Be(CertificateSharingController.EmailSentRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
            result.RouteValues["sharingId"].Should().Be(sharingId);
            result.RouteValues["sharingEmailId"].Should().Be(sharingEmailId);

            _sessionServiceMock.Verify(s => s.ClearShareEmailAsync(), Times.Once);
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
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
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

        [Test]
        public async Task DeleteSharing_Returns_View_When_Model_Valid()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            var model = new CertificateSharingLinkViewModel
            {
                CertificateId = certificateId,
                SharingId = sharingId,
                SharingNumber = 5,
                CourseName = "Course",
                CertificateType = Domain.Models.CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync(model);

            // Act
            var result = await _sut.DeleteSharing(certificateId, sharingId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().BeEquivalentTo(model);
            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
        }

        [Test]
        public async Task DeleteSharing_Redirects_To_CreateCertificateSharing_When_Model_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync((CertificateSharingLinkViewModel)null!);

            // Act
            var result = await _sut.DeleteSharing(certificateId, sharingId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
            result.RouteValues.Should().ContainKey("certificateId");
            result.RouteValues["certificateId"].Should().Be(certificateId);
            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
        }

        [Test]
        public async Task DeleteSharingPost_Deletes_And_Redirects_When_Model_Found()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            var model = new CertificateSharingLinkViewModel
            {
                CertificateId = certificateId,
                SharingId = sharingId,
                SharingNumber = 7,
                CourseName = "Course",
                CertificateType = Domain.Models.CertificateType.Standard
            };

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync(model);

            _sharingOrchestratorMock
                .Setup(s => s.DeleteSharing(certificateId, sharingId))
                .Returns(Task.CompletedTask);

            _sut.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await _sut.DeleteSharingPost(certificateId, sharingId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);

            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
            _sharingOrchestratorMock.Verify(s => s.DeleteSharing(certificateId, sharingId), Times.Once);

            var title = _sut.TempData[TempDataDictionaryExtensions.FlashMessageTitleTempDataKey]?.ToString();
            title.Should().Be($"Sharing link {model.SharingNumber} deleted");
        }

        [Test]
        public async Task DeleteSharingPost_Redirects_To_CreateCertificateSharing_When_Model_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();

            _sharingOrchestratorMock
                .Setup(s => s.GetSharingById(certificateId, sharingId))
                .ReturnsAsync((CertificateSharingLinkViewModel)null!);

            // Act
            var result = await _sut.DeleteSharingPost(certificateId, sharingId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificateSharingController.CreateCertificateSharingRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);

            _sharingOrchestratorMock.Verify(s => s.GetSharingById(certificateId, sharingId), Times.Once);
            _sharingOrchestratorMock.Verify(s => s.DeleteSharing(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }    
    }
}
