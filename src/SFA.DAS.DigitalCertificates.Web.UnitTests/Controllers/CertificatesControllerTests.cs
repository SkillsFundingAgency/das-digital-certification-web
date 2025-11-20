using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class CertificatesControllerTests
    {
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private CertificatesController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _sut = new CertificatesController(_contextAccessorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public void CertificatesList_Returns_View()
        {
            // Act
            var result = _sut.CertificatesList() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Certificate_Returns_View()
        {
            // Act
            var result = _sut.Certificate() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }
    }
}
