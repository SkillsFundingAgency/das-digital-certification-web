using System.Collections.Generic;
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
    public class UlnAuthorisedFailureHandlerTests
    {
        private Mock<LinkGenerator> _linkGeneratorMock;
        private DefaultHttpContext _httpContext;
        private UlnAuthorisedFailureHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _linkGeneratorMock = new Mock<LinkGenerator>();
            _httpContext = new DefaultHttpContext();
            _sut = new UlnAuthorisedFailureHandler(_linkGeneratorMock.Object);
        }

        [Test]
        public async Task When_RequirementNotInPolicy_Then_ReturnsFalse()
        {
            // Arrange
            var policy = CreatePolicyWithRequirement(includeRequirement: false);
            var result = CreateFailure(ulnFailure: true);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeFalse();
        }

        [Test]
        public async Task When_RequirementInPolicy_But_NotFailed_Then_ReturnsFalse()
        {
            // Arrange
            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(ulnFailure: false);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeFalse();
        }

        [Test]
        public async Task When_RequirementInPolicy_And_Failed_And_RouteGenerated_Then_Redirects_AndReturnsTrue()
        {
            // Arrange
            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(ulnFailure: true);

            _httpContext.Request.Path = "/some-path";

            _linkGeneratorMock
                .Setup(l => l.GetPathByAddress(
                    _httpContext,
                    AuthoriseController.AuthoriseStartRouteGet,
                    It.IsAny<RouteValueDictionary>(),
                    null,
                    null,
                    default,
                    null))
                .Returns("/start-authorise");

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeTrue();
            _httpContext.Response.Headers.Location.Should().Contain("/start-authorise");
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }

        [Test]
        public async Task When_RequirementInPolicy_And_Failed_But_RouteIsNull_Then_ReturnsFalse()
        {
            // Arrange
            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(ulnFailure: true);

            _linkGeneratorMock
                .Setup(l => l.GetPathByAddress(
                    _httpContext,
                    AuthoriseController.AuthoriseStartRouteGet,
                    It.IsAny<RouteValueDictionary>(),
                    null,
                    null,
                    default,
                    null))
                .Returns((string)null);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeFalse();
        }

        private static AuthorizationPolicy CreatePolicyWithRequirement(bool includeRequirement)
        {
            if (includeRequirement)
            {
                return new AuthorizationPolicy(
                    new[] { new UlnAuthorisedRequirement() },
                    new List<string>());
            }

            // policy must contain at least one requirement
            return new AuthorizationPolicy(
                new[] { new DenyAnonymousAuthorizationRequirement() },
                new List<string>());
        }

        private static PolicyAuthorizationResult CreateFailure(bool ulnFailure)
        {
            if (!ulnFailure)
            {
                return PolicyAuthorizationResult.Forbid();
            }

            var failure = AuthorizationFailure.Failed(
                new[]
                {
                    new AuthorizationFailureReason(
                        null,
                        DigitalCertificatesAuthorizationFailureMessages.NotUlnAuthorized)
                });

            return PolicyAuthorizationResult.Forbid(failure);
        }
    }
}
