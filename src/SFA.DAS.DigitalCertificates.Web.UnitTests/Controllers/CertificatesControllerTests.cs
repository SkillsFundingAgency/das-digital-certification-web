using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

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
    }
}