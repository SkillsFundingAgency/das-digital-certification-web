using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Services.SessionStorage;
using SFA.DAS.DigitalCertificates.Web.Services.User;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services.SessionStorage
{
    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<IUserService> _userServicMock;
        private Mock<ICacheStorageService> _cacheStorageMock;
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private SessionStorageService _sut;

        [SetUp]
        public void Setup()
        {
            _userServicMock = new Mock<IUserService>();
            _cacheStorageMock = new Mock<ICacheStorageService>();
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();

            _sut = new SessionStorageService(_userServicMock.Object, _cacheStorageMock.Object, _outerApiMock.Object);
        }
    }
}