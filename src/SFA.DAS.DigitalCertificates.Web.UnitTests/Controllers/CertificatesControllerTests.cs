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

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class CertificatesControllerTests
    {
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ICertificatesOrchestrator> _certificatesOrchestratorMock;
        private Mock<ISessionService> _sessionServiceMock;
        private CertificatesController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _certificatesOrchestratorMock = new Mock<ICertificatesOrchestrator>();
            _sessionServiceMock = new Mock<ISessionService>();

            _sut = new CertificatesController(
                _contextAccessorMock.Object,
                _certificatesOrchestratorMock.Object,
                _sessionServiceMock.Object);
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
            _sessionServiceMock.Verify(s => s.ClearContactReferenceAsync(), Times.Once);
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
            _sessionServiceMock.Verify(s => s.ClearContactReferenceAsync(), Times.Once);
        }
        [Test]
        public async Task DownloadCertificateStandardPdf_Throws_InvalidOperationException_When_Model_Is_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _certificatesOrchestratorMock
                .Setup(x => x.GetDownloadCertificateViewModelAsync(certificateId))
                .ReturnsAsync((DownloadCertificateViewModel)null!);

            // Act
            Func<Task> act = async () => await _sut.DownloadCertificateStandardPdf(certificateId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage(CertificatesController.PdfCertificateCannotBeProduced);

            _certificatesOrchestratorMock.Verify(
                x => x.GetDownloadCertificateViewModelAsync(certificateId),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(It.IsAny<DownloadCertificateViewModel>()),
                Times.Never);
        }

        [Test]
        public async Task DownloadCertificateStandardPdf_Returns_Pdf_File_When_Model_Found()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var pdfBytes = new byte[] { 1, 2, 3, 4 };

            var model = new DownloadCertificateViewModel
            {
                CertificateNumber = "678123",
                FamilyName = "Test",
                GivenNames = "Test Given Name",
                CourseOption = "Software developer",
                CourseLevel = "3",
                OverallGrade = "Pass",
                CourseName = "Test",
                CoronationEmblem = false,
                DateAwarded = DateTime.UtcNow,
            };

            _certificatesOrchestratorMock
                .Setup(x => x.GetDownloadCertificateViewModelAsync(certificateId))
                .ReturnsAsync(model);

            _certificatesOrchestratorMock
                .Setup(x => x.GenerateCertificateAsync(model))
                .ReturnsAsync(pdfBytes);

            // Act
            var result = await _sut.DownloadCertificateStandardPdf(certificateId);

            // Assert
            result.Should().BeOfType<FileContentResult>();

            var fileResult = result as FileContentResult;
            fileResult.Should().NotBeNull();
            fileResult!.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be("CertificateNumber678123.pdf");
            fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);

            _certificatesOrchestratorMock.Verify(
                x => x.GetDownloadCertificateViewModelAsync(certificateId),
                Times.Once);

            _certificatesOrchestratorMock.Verify(
                x => x.GenerateCertificateAsync(model),
                Times.Once);
        }

        [Test]
        public void DownloadCertificateStandardPdf_Throws_InvalidOperationException_When_Pdf_Cannot_Be_Produced()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var model = new DownloadCertificateViewModel
            {
                CertificateNumber = "678123",
                FamilyName = "Test",
                GivenNames = "Test Given Name",
                CourseOption = "Software developer",
                CourseLevel = "3",
                OverallGrade = "Pass",
                CourseName = "Test",
                CoronationEmblem = false,
                DateAwarded = DateTime.UtcNow,
            };

            _certificatesOrchestratorMock
                .Setup(x => x.GetDownloadCertificateViewModelAsync(certificateId))
                .ReturnsAsync(model);

            _certificatesOrchestratorMock
                .Setup(x => x.GenerateCertificateAsync(model))
                .ReturnsAsync((byte[])null!);

            // Act
            var result = async () => await _sut.DownloadCertificateStandardPdf(certificateId);

            // Assert
            Assert.That(result, Throws.TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("PDF certificate cannot be produced"));

            _certificatesOrchestratorMock.Verify(x => x.GetDownloadCertificateViewModelAsync(certificateId), Times.Once);

            _certificatesOrchestratorMock.Verify(x => x.GenerateCertificateAsync(model), Times.Once);
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
            _sessionServiceMock.Verify(s => s.ClearContactReferenceAsync(), Times.Once);
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
        public async Task SelectAddress_Get_ReturnsView_When_ModelExists()
        {
            var certId = Guid.NewGuid();
            var vm = new SelectAddressViewModel { CertificateId = certId };

            _certificatesOrchestratorMock.Setup(x => x.GetSelectAddressViewModel(certId, null)).ReturnsAsync(vm);

            var result = await _sut.SelectAddress(certId);

            result.Should().BeOfType<ViewResult>();
            var view = (ViewResult)result;
            view.Model.Should().BeSameAs(vm);
            _sessionServiceMock.Verify(x => x.ClearDeliveryAddressAsync(), Times.Once);
        }

        [Test]
        public async Task SelectAddress_Get_RedirectsToStandard_When_NoModel()
        {
            var certId = Guid.NewGuid();
            _certificatesOrchestratorMock.Setup(x => x.GetSelectAddressViewModel(certId, null)).ReturnsAsync((SelectAddressViewModel)null);

            var result = await _sut.SelectAddress(certId);

            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            redirect.RouteValues.Should().ContainKey("certificateId");
            _sessionServiceMock.Verify(x => x.ClearDeliveryAddressAsync(), Times.Never);
        }

        [Test]
        public async Task AddAddress_Get_ReturnsView_When_ModelExists()
        {
            var certId = Guid.NewGuid();
            var vm = new AddAddressManualViewModel { CertificateId = certId };

            _certificatesOrchestratorMock.Setup(x => x.GetAddAddressViewModel(certId)).ReturnsAsync(vm);

            var result = await _sut.AddAddress(certId);

            result.Should().BeOfType<ViewResult>();
            var view = (ViewResult)result;
            view.Model.Should().BeSameAs(vm);
        }

        [Test]
        public async Task AddAddress_Get_RedirectsToStandard_When_NoModel()
        {
            var certId = Guid.NewGuid();
            _certificatesOrchestratorMock.Setup(x => x.GetAddAddressViewModel(certId)).ReturnsAsync((AddAddressManualViewModel)null);

            var result = await _sut.AddAddress(certId);

            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            redirect.RouteValues.Should().ContainKey("certificateId");
            _sessionServiceMock.Verify(x => x.ClearDeliveryAddressAsync(), Times.Never);
        }

        [Test]
        public async Task SelectAddressPost_When_ValidationFails_RedirectsToSelectAddressGet_And_SetsCertificateId()
        {
            var certId = Guid.NewGuid();
            var model = new SelectAddressViewModel { SearchTerm = "x" };

            _certificatesOrchestratorMock.Setup(x => x.ValidateSelectAddressViewModel(model, It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            var result = await _sut.SelectAddressPost(certId, model);

            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.SelectAddressRouteGet);
        }

        [Test]
        public async Task AddAddressPost_When_ValidationFails_RedirectsToAddAddressGet_And_SetsCertificateId()
        {
            var certId = Guid.NewGuid();
            var model = new AddAddressManualViewModel { AddressLine1 = "" };

            _certificatesOrchestratorMock.Setup(x => x.ValidateAddAddressManualViewModel(model, It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            var result = await _sut.AddAddressPost(certId, model);

            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.AddAddressRouteGet);
        }

        [Test]
        public async Task SelectAddressPost_When_ValidationSucceeds_RedirectsToCheckAndSubmit_And_SetsCertificateId()
        {
            var certId = Guid.NewGuid();
            var model = new SelectAddressViewModel { SearchTerm = "x" };

            _certificatesOrchestratorMock.Setup(x => x.ValidateSelectAddressViewModel(model, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _certificatesOrchestratorMock.Setup(x => x.StoreDeliveryAddressFromLocationAsync(certId, It.IsAny<string>(), CertificatesController.SelectAddressRouteGet)).ReturnsAsync(true);

            var result = await _sut.SelectAddressPost(certId, model);

            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CheckAndSubmitRouteGet);
        }

        [Test]
        public async Task SelectAddressPost_When_StoreFails_RedirectsToSelectAddress_And_SetsCertificateId()
        {
            var certId = Guid.NewGuid();
            var model = new SelectAddressViewModel { SearchTerm = "x" };

            _certificatesOrchestratorMock.Setup(x => x.ValidateSelectAddressViewModel(model, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _certificatesOrchestratorMock.Setup(x => x.StoreDeliveryAddressFromLocationAsync(certId, It.IsAny<string>(), CertificatesController.SelectAddressRouteGet)).ReturnsAsync(false);

            var result = await _sut.SelectAddressPost(certId, model);

            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.SelectAddressRouteGet);
        }

        [Test]
        public async Task AddAddressPost_When_ValidationSucceeds_RedirectsToCheckAndSubmit_And_SetsCertificateId()
        {
            var certId = Guid.NewGuid();
            var model = new AddAddressManualViewModel { AddressLine1 = "1 Test St" };

            _certificatesOrchestratorMock.Setup(x => x.ValidateAddAddressManualViewModel(model, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);

            var result = await _sut.AddAddressPost(certId, model);

            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CheckAndSubmitRouteGet);
        }

        [Test]
        public async Task CheckAndSubmit_Get_ReturnsView_When_ModelExists()
        {
            var certId = Guid.NewGuid();
            var vm = new CheckAndSubmitViewModel { CertificateId = certId };

            _certificatesOrchestratorMock.Setup(x => x.GetCheckAndSubmitViewModel(certId, It.IsAny<string>())).ReturnsAsync(vm);

            var result = await _sut.CheckAndSubmit(certId);

            result.Should().BeOfType<ViewResult>();
            var view = (ViewResult)result;
            view.Model.Should().BeSameAs(vm);
        }

        [Test]
        public async Task CheckAndSubmit_Get_RedirectsToStandard_When_NoModel()
        {
            var certId = Guid.NewGuid();
            _certificatesOrchestratorMock.Setup(x => x.GetCheckAndSubmitViewModel(certId, It.IsAny<string>())).ReturnsAsync((CheckAndSubmitViewModel)null);

            var result = await _sut.CheckAndSubmit(certId);

            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            redirect.RouteValues.Should().ContainKey("certificateId");
        }

        [Test]
        public async Task CheckAndSubmitPost_RedirectsToPrintRequestConfirmation()
        {
            var certId = Guid.NewGuid();
            _certificatesOrchestratorMock.Setup(p => p.CreatePrintRequest(certId)).Returns(Task.CompletedTask);

            var result = await _sut.CheckAndSubmitPost(certId);

            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.PrintRequestConfirmationRouteGet);
            redirect.RouteValues.Should().ContainKey("certificateId");
        }


        public async Task ContactUsForCertificateCreate_Redirects_To_ContactUs_When_ReferenceNumber_Returned()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var referenceNumber = "REF-123";

            _certificatesOrchestratorMock
                .Setup(o => o.CreateUserActionForCertificate(certificateId, It.IsAny<ActionType>()))

                .ReturnsAsync(new CreateUserActionForCertificateResult
                {
                    ReferenceNumber = referenceNumber,
                    CertificateType = CertificateType.Standard
                });

            // Act
            var result = await _sut.ContactUsForCertificateCreate(certificateId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.ContactUsForCertificateRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
        }

        [Test]
        public async Task ContactUsForCertificateCreate_Redirects_To_Standard_When_ReferenceNumber_Empty_And_Type_Standard()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _certificatesOrchestratorMock
                .Setup(o => o.CreateUserActionForCertificate(certificateId, It.IsAny<ActionType>()))

                .ReturnsAsync(new CreateUserActionForCertificateResult
                {
                    ReferenceNumber = null,
                    CertificateType = CertificateType.Standard
                });

            // Act
            var result = await _sut.ContactUsForCertificateCreate(certificateId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
        }

        [Test]
        public async Task ContactUsForCertificateCreate_Redirects_To_Framework_When_ReferenceNumber_Empty_And_Type_Framework()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _certificatesOrchestratorMock
                .Setup(o => o.CreateUserActionForCertificate(certificateId, It.IsAny<ActionType>()))

                .ReturnsAsync(new CreateUserActionForCertificateResult
                {
                    ReferenceNumber = null,
                    CertificateType = CertificateType.Framework
                });

            // Act
            var result = await _sut.ContactUsForCertificateCreate(certificateId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CertificateFrameworkRouteGet);
            result.RouteValues["certificateId"].Should().Be(certificateId);
        }

        [Test]
        public async Task ContactUsForCertificateCreate_Redirects_To_CertificatesList_When_ReferenceNumber_Empty_And_Type_Unknown()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _certificatesOrchestratorMock
                .Setup(o => o.CreateUserActionForCertificate(certificateId, It.IsAny<ActionType>()))

                .ReturnsAsync(new CreateUserActionForCertificateResult
                {
                    ReferenceNumber = null,
                    CertificateType = CertificateType.Unknown
                });

            // Act
            var result = await _sut.ContactUsForCertificateCreate(certificateId) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(CertificatesController.CertificatesListRouteGet);
        }

        [Test]
        public void LinkAccessed_WhenAccessHistoryIsNull_ReturnsFalse()
        {
            // Arrange           
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory =null
            };

            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.False(result);
        }

        [Test]
        public void LinkAccessed_WhenAccessHistoryIsEmpty_ReturnsFalse()
        {
            // Arrange            
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory = new()
            };


            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.False(result);
        }

        [Test]
        public void LinkAccessed_WhenAccessHistoryContainsDirectLink_ReturnsTrue()
        {
            // Arrange
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory = new() { new() { AccessType = AccessType.DirectLink } }
            };
           
            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.True(result);
        }

        [Test]
        public void LinkAccessed_WhenAccessHistoryContainsEmailLink_ReturnsTrue()
        {
            // Arrange
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory = new() { new() { AccessType = AccessType.EmailLink } }
            };

            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.True(result);
        }
       
        [Test]
        public void LinkAccessed_WhenAccessHistoryContainsMixedAccessTypes_ReturnsTrue()
        {
            // Arrange
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory = new()
            {
                new() { AccessType = AccessType.EmailLink },
                new() { AccessType = AccessType.DirectLink },
                new() { AccessType = AccessType.Created },
                new() { AccessType = AccessType.EmailSent }
            }
            };

            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.True(result);
        }

        [Test]
        public void LinkAccessed_ReturnsTrueWhenAccessHistoryContainsBothDirectAndEmailLinks()
        {
            // Arrange
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory = new List<SharingAccessHistoryItem>
                {
                    new SharingAccessHistoryItem { AccessType = AccessType.DirectLink },
                    new SharingAccessHistoryItem { AccessType = AccessType.EmailLink }
                }
            };

            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void LinkAccessed_ReturnsFalseWhenAccessHistoryIsEmpty()
        {
            // Arrange
            var viewModel = new CertificateSharingLinkViewModel
            {
                CourseName = "Course",
                AccessHistory = new List<SharingAccessHistoryItem>()
            };

            // Act
            var result = viewModel.LinkAccessed;

            // Assert
            Assert.That(result, Is.False);
        }       
    }
}
