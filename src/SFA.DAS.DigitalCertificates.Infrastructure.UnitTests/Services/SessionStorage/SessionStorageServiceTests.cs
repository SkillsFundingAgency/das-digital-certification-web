using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;

namespace SFA.DAS.DigitalCertificates.Infrastructure.UnitTests.Services.SessionStorage
{



    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<ISession> _sessionMock;
        private SessionStorageService _sut;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionMock = new Mock<ISession>();

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            _sut = new SessionStorageService(_httpContextAccessorMock.Object);
        }
    }
}