using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Controllers;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Authentication
{
    public class NotUlnAuthorisedFailureHandlerTests
    {
        private Mock<LinkGenerator> _linkGeneratorMock;
        private DefaultHttpContext _httpContext;
        private NotUlnAuthorisedFailureHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _linkGeneratorMock = new Mock<LinkGenerator>();
            _httpContext = new DefaultHttpContext();
            _sut = new NotUlnAuthorisedFailureHandler(_linkGeneratorMock.Object);
        }

        [Test]
        public async Task When_RequirementNotInPolicy_Then_ReturnsFalse()
        {
            var policy = CreatePolicyWithRequirement(includeRequirement: false);
            var result = CreateFailure(notUlnFailure: true);

            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            handled.Should().BeFalse();
        }

        [Test]
        public async Task When_RequirementInPolicy_But_NotFailed_Then_ReturnsFalse()
        {
            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(notUlnFailure: false);

            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            handled.Should().BeFalse();
        }

        [Test]
        public async Task When_RequirementInPolicy_And_Failed_And_RouteGenerated_Then_Redirects_AndReturnsTrue()
        {
            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(notUlnFailure: true);

            _linkGeneratorMock
                .Setup(l => l.GetPathByAddress(
                    _httpContext,
                    CertificatesController.CertificatesListRouteGet,
                    It.IsAny<RouteValueDictionary>(),
                    null,
                    null,
                    default,
                    null))
                .Returns("/certificates");

            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            handled.Should().BeTrue();
            _httpContext.Response.Headers.Location.Should().Contain("/certificates");
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }

        [Test]
        public async Task When_RequirementInPolicy_And_Failed_But_RouteIsNull_Then_ReturnsFalse()
        {
            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(notUlnFailure: true);

            _linkGeneratorMock
                .Setup(l => l.GetPathByAddress(
                    _httpContext,
                    CertificatesController.CertificatesListRouteGet,
                    It.IsAny<RouteValueDictionary>(),
                    null,
                    null,
                    default,
                    null))
                .Returns((string)null);

            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            handled.Should().BeFalse();
        }

        private static AuthorizationPolicy CreatePolicyWithRequirement(bool includeRequirement)
        {
            if (includeRequirement)
            {
                return new AuthorizationPolicy(new[] { new NotUlnAuthorisedRequirement() }, new System.Collections.Generic.List<string>());
            }

            return new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, new System.Collections.Generic.List<string>());
        }

        private static PolicyAuthorizationResult CreateFailure(bool notUlnFailure)
        {
            if (!notUlnFailure)
            {
                return PolicyAuthorizationResult.Forbid();
            }

            var failure = AuthorizationFailure.Failed(
                new[]
                {
                    new AuthorizationFailureReason(
                        null,
                        DigitalCertificatesAuthorizationFailureMessages.UlnAuthorized)
                });

            return PolicyAuthorizationResult.Forbid(failure);
        }
    }
}
