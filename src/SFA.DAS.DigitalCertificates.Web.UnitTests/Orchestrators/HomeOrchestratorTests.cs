using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class HomeOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;

        private HomeOrchestrator _sut;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();

            _httpContext = new DefaultHttpContext();
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);

            _sut = new HomeOrchestrator(
                _mediatorMock.Object, 
                _contextAccessorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }
    }
}
