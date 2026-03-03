using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    public class CertificatesControllerAddressTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private Mock<ICertificatesOrchestrator> _certificatesOrchestrator;
        private Mock<ISharingOrchestrator> _sharingOrchestrator;
        private Mock<SFA.DAS.DigitalCertificates.Web.Services.ISessionService> _sessionService;
        private CertificatesController _sut;

        [SetUp]
        public void SetUp()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            _certificatesOrchestrator = new Mock<ICertificatesOrchestrator>();
            _sharingOrchestrator = new Mock<ISharingOrchestrator>();
            _sessionService = new Mock<SFA.DAS.DigitalCertificates.Web.Services.ISessionService>();

            _sut = new CertificatesController(_httpContextAccessor.Object, _certificatesOrchestrator.Object, _sharingOrchestrator.Object, _sessionService.Object);
        }

        [Test]
        public async Task SelectAddress_Get_ReturnsView_When_ModelExists()
        {
            // Arrange
            var certId = Guid.NewGuid();
            var vm = new SelectAddressViewModel { CertificateId = certId };

            _certificatesOrchestrator.Setup(x => x.GetSelectAddressViewModel(certId, null)).ReturnsAsync(vm);

            // Act
            var result = await _sut.SelectAddress(certId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var view = (ViewResult)result;
            view.Model.Should().BeSameAs(vm);
        }

        [Test]
        public async Task SelectAddress_Get_RedirectsToStandard_When_NoModel()
        {
            // Arrange
            var certId = Guid.NewGuid();
            _certificatesOrchestrator.Setup(x => x.GetSelectAddressViewModel(certId, null)).ReturnsAsync((SelectAddressViewModel)null);

            // Act
            var result = await _sut.SelectAddress(certId);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            redirect.RouteValues.Should().ContainKey("certificateId");
        }

        [Test]
        public async Task AddAddress_Get_ReturnsView_When_ModelExists()
        {
            // Arrange
            var certId = Guid.NewGuid();
            var vm = new AddAddressManualViewModel { CertificateId = certId };

            _certificatesOrchestrator.Setup(x => x.GetAddAddressViewModel(certId)).ReturnsAsync(vm);

            // Act
            var result = await _sut.AddAddress(certId);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var view = (ViewResult)result;
            view.Model.Should().BeSameAs(vm);
        }

        [Test]
        public async Task AddAddress_Get_RedirectsToStandard_When_NoModel()
        {
            // Arrange
            var certId = Guid.NewGuid();
            _certificatesOrchestrator.Setup(x => x.GetAddAddressViewModel(certId)).ReturnsAsync((AddAddressManualViewModel)null);

            // Act
            var result = await _sut.AddAddress(certId);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.CertificateStandardRouteGet);
            redirect.RouteValues.Should().ContainKey("certificateId");
        }

        [Test]
        public async Task SelectAddressPost_When_ValidationFails_RedirectsToSelectAddressGet_And_SetsCertificateId()
        {
            // Arrange
            var certId = Guid.NewGuid();
            var model = new SelectAddressViewModel { SearchTerm = "x" };

            _certificatesOrchestrator.Setup(x => x.ValidateSelectAddressViewModel(model, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.SelectAddressPost(certId, model);

            // Assert
            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.SelectAddressRouteGet);
        }

        [Test]
        public async Task AddAddressPost_When_ValidationFails_RedirectsToAddAddressGet_And_SetsCertificateId()
        {
            // Arrange
            var certId = Guid.NewGuid();
            var model = new AddAddressManualViewModel { AddressLine1 = "" };

            _certificatesOrchestrator.Setup(x => x.ValidateAddAddressManualViewModel(model, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.AddAddressPost(certId, model);

            // Assert
            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.AddAddressRouteGet);
        }

       // TODO: Success path not implemented yet as validation is currently the only logic in the post actions, but will be added when further processing is implemented
        [Test]
        public async Task SelectAddressPost_When_ValidationSucceeds_RedirectsToSelectAddressGet_And_SetsCertificateId()
        {
            // Arrange
            var certId = Guid.NewGuid();
            var model = new SelectAddressViewModel { SearchTerm = "x" };

            _certificatesOrchestrator.Setup(x => x.ValidateSelectAddressViewModel(model, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>())).ReturnsAsync(true);

            // Act
            var result = await _sut.SelectAddressPost(certId, model);

            // Assert
            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.SelectAddressRouteGet);
        }

       // TODO: Success path not implemented yet as validation is currently the only logic in the post actions, but will be added when further processing is implemented
        [Test]
        public async Task AddAddressPost_When_ValidationSucceeds_RedirectsToAddAddressGet_And_SetsCertificateId()
        {
            // Arrange
            var certId = Guid.NewGuid();
            var model = new AddAddressManualViewModel { AddressLine1 = "1 Test St" };

            _certificatesOrchestrator.Setup(x => x.ValidateAddAddressManualViewModel(model, It.IsAny<Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary>())).ReturnsAsync(true);

            // Act
            var result = await _sut.AddAddressPost(certId, model);

            // Assert
            model.CertificateId.Should().Be(certId);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)result;
            redirect.RouteName.Should().Be(CertificatesController.AddAddressRouteGet);
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }
    }
}
