using System;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class BaseOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private DefaultHttpContext _httpContext;
        private TestBaseOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(_httpContext);

            _sut = new TestBaseOrchestrator(
                _mediatorMock.Object,
                _httpContextAccessorMock.Object);
        }

        [Test]
        public void GetGovUkIdentifier_Returns_NameIdentifier_Claim()
        {
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "gov-uk-id")
            }));

            var result = _sut.GovUkIdentifier();

            Assert.That(result, Is.EqualTo("gov-uk-id"));
        }

        [Test]
        public void GetUserId_Returns_UserId_Claim()
        {
            var userId = Guid.NewGuid().ToString();

            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(DigitalCertificateClaimsTypes.UserId, userId)
            }));

            var result = _sut.UserId();

            Assert.That(result, Is.EqualTo(userId));
        }

        [Test]
        public void GetUserPhoneNumber_Returns_MobilePhone_Claim()
        {
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.MobilePhone, "07700900000")
            }));

            var result = _sut.UserPhoneNumber();

            Assert.That(result, Is.EqualTo("07700900000"));
        }

        [Test]
        public void HttpContext_Returns_Current_HttpContext()
        {
            var result = _sut.CurrentHttpContext();

            Assert.That(result, Is.SameAs(_httpContext));
        }

        [Test]
        public void Claim_Helpers_Return_Empty_String_When_HttpContext_Is_Null()
        {
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            Assert.That(_sut.GovUkIdentifier(), Is.EqualTo(string.Empty));
            Assert.That(_sut.UserId(), Is.EqualTo(string.Empty));
            Assert.That(_sut.UserPhoneNumber(), Is.EqualTo(string.Empty));
            Assert.That(_sut.UserEmail(), Is.EqualTo(string.Empty));
            Assert.That(_sut.UserGivenNames(), Is.EqualTo(string.Empty));
            Assert.That(_sut.UserSurname(), Is.EqualTo(string.Empty));
            Assert.That(_sut.UserDisplayName(), Is.EqualTo(string.Empty));
            Assert.That(_sut.CurrentHttpContext(), Is.Null);
        }

        private class TestBaseOrchestrator : BaseOrchestrator
        {
            public TestBaseOrchestrator(IMediator mediator, IHttpContextAccessor httpContextAccessor)
                : base(mediator, httpContextAccessor)
            {
            }

            public string GovUkIdentifier() => GetGovUkIdentifier();
            public string UserId() => GetUserId();
            public string UserPhoneNumber() => GetUserPhoneNumber();
            public string UserEmail() => GetUserEmail();
            public string UserGivenNames() => GetUserGivenNames();
            public string UserSurname() => GetUserSurname();
            public string UserDisplayName() => GetUserDisplayName();
            public HttpContext CurrentHttpContext() => HttpContext;
        }
    }
}