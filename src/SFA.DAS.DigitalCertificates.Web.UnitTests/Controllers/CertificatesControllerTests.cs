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
            // Act
            var result = await _sut.CertificatesList() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Certificate_Returns_View()
        {
            // Act
            var result = _sut.CertificateStandard(Guid.NewGuid()) as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task CreateCertificateSharing_Get_Returns_View_With_Model()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var model = new CertificateSharingViewModel
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
    }
}