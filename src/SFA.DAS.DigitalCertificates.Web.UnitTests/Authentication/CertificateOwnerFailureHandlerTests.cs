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
    public class CertificateOwnerFailureHandlerTests
    {
        private Mock<LinkGenerator> _linkGeneratorMock;
        private CertificateOwnerFailureHandler _sut;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _linkGeneratorMock = new Mock<LinkGenerator>();
            _sut = new CertificateOwnerFailureHandler(_linkGeneratorMock.Object);
            _httpContext = new DefaultHttpContext();
        }

        [Test]
        public async Task When_RequirementInPolicy_And_FailureIsCertificateOwner_And_PathMatches_Then_Redirects_AndReturnsTrue()
        {
            // Arrange
            _httpContext.Request.Path = $"/{CertificatesController.BaseRoute}/123";

            _linkGeneratorMock
                .Setup(l => l.GetPathByAddress(
                    _httpContext,
                    CertificatesController.CertificatesListRouteGet,
                    It.IsAny<RouteValueDictionary>(),
                    null,
                    null,
                    default,
                    null
                ))
                .Returns("/certificates");

            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(certificateOwnerFailure: true);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeTrue();
            _httpContext.Response.Headers.Location.Should().Contain("/certificates");
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status302Found);
        }


        [Test]
        public async Task When_RequirementInPolicy_And_FailureIsCertificateOwner_But_NoRouteReturned_Then_ReturnsFalse()
        {
            // Arrange
            _httpContext.Request.Path = $"/{CertificatesController.BaseRoute}/123";

            _linkGeneratorMock
                .Setup(l => l.GetPathByAddress(
                    _httpContext,
                    CertificatesController.CertificatesListRouteGet,
                    It.IsAny<RouteValueDictionary>(),
                    null,
                    null,
                    default,
                    null
                ))
                .Returns((string)null);

            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(certificateOwnerFailure: true);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeFalse();
            _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        public async Task When_RequirementInPolicy_And_FailureIsNotCertificateOwner_Then_ReturnsFalse()
        {
            // Arrange
            _httpContext.Request.Path = $"/{CertificatesController.BaseRoute}/123";

            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(certificateOwnerFailure: false);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeFalse();
        }

        [Test]
        public async Task When_RequirementNotInPolicy_Then_ReturnsFalse()
        {
            // Arrange
            _httpContext.Request.Path = $"/{CertificatesController.BaseRoute}/123";

            var policy = CreatePolicyWithRequirement(includeRequirement: false);
            var result = CreateFailure(certificateOwnerFailure: true);

            // Act
            var handled = await _sut.HandleFailureAsync(_httpContext, policy, result);

            // Assert
            handled.Should().BeFalse();
        }

        [Test]
        public async Task When_PathDoesNotStartWithCertificatesBaseRoute_Then_ReturnsFalse()
        {
            // Arrange
            _httpContext.Request.Path = "/somewhere-else";

            var policy = CreatePolicyWithRequirement(includeRequirement: true);
            var result = CreateFailure(certificateOwnerFailure: true);

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
                    new[] { new CertificateOwnerRequirement() },
                    new List<string>());
            }

            // must contain at least one requirement - use a different one instead
            return new AuthorizationPolicy(
                new[] { new DenyAnonymousAuthorizationRequirement() },
                new List<string>());
        }


        private static PolicyAuthorizationResult CreateFailure(bool certificateOwnerFailure)
        {
            if (!certificateOwnerFailure)
            {
                return PolicyAuthorizationResult.Forbid();
            }

            var failure = AuthorizationFailure.Failed(
                new[]
                {
                    new AuthorizationFailureReason(null, DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner)
                });

            return PolicyAuthorizationResult.Forbid(failure);
        }
    }
}
