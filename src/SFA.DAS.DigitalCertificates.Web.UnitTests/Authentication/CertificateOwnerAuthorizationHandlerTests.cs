using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Authentication
{
    public class CertificateOwnerAuthorizationHandlerTests
    {
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<IUserService> _userServiceMock;
        private CertificateOwnerAuthorizationHandler _sut;
        private AuthorizationHandlerContext _authContext;
        private CertificateOwnerRequirement _requirement;
        private DefaultHttpContext _httpContext;

        private Guid _standardCertificateId = Guid.NewGuid();
        private Guid _frameworkCertificateId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _userServiceMock = new Mock<IUserService>();
            _sut = new CertificateOwnerAuthorizationHandler(_cacheServiceMock.Object, _userServiceMock.Object);

            _requirement = new CertificateOwnerRequirement();
            _httpContext = new DefaultHttpContext();

            _authContext = new AuthorizationHandlerContext(
                new[] { _requirement },
                new ClaimsPrincipal(),
                _httpContext);
        }

        [Test]
        public async Task When_StandardRoute_And_StandardCertificateOwned_Then_Succeeds()
        {
            // Arrange
            SetRoute(_standardCertificateId.ToString(), CertificatesController.CertificateStandardRouteGet);

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns("gov-123");
            _cacheServiceMock
                .Setup(x => x.GetOwnedCertificatesAsync("gov-123"))
                .ReturnsAsync(new List<Certificate>
                {
                    new Certificate { CertificateId = _standardCertificateId, CertificateType = CertificateType.Standard, CourseName = "Bricklayer", CourseLevel = "1" }
                });

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasSucceeded.Should().BeTrue();
        }

        [Test]
        public async Task When_FrameworkRoute_And_FrameworkCertificateOwned_Then_Succeeds()
        {
            // Arrange
            SetRoute(_frameworkCertificateId.ToString(), CertificatesController.CertificateFrameworkRouteGet);

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns("gov-123");
            _cacheServiceMock
                .Setup(x => x.GetOwnedCertificatesAsync("gov-123"))
                .ReturnsAsync(new List<Certificate>
                {
                    new Certificate { CertificateId = _frameworkCertificateId, CertificateType = CertificateType.Framework, CourseName = "Plumber", CourseLevel = "Advanced" }
                });

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasSucceeded.Should().BeTrue();
        }

        public async Task When_RouteHasNoType_MatchingId_Succeeds()
        {
            // Arrange
            var certId = Guid.NewGuid();
            SetRoute(certId.ToString(), "SomeOtherRoute");

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns("gov-123");
            _cacheServiceMock
                .Setup(x => x.GetOwnedCertificatesAsync("gov-123"))
                .ReturnsAsync(new List<Certificate>
                {
                    new Certificate { CertificateId = certId, CertificateType = CertificateType.Framework, CourseName = "Plumber", CourseLevel = "Advanced" }
                });

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasSucceeded.Should().BeTrue();
        }

        [Test]
        public async Task When_StandardRoute_And_FrameworkCertificateOwned_Then_Fails()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            SetRoute(certificateId.ToString(), CertificatesController.CertificateStandardRouteGet);

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns("gov-123");
            _cacheServiceMock
                .Setup(x => x.GetOwnedCertificatesAsync("gov-123"))
                .ReturnsAsync(new List<Certificate>
                {
                    new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Framework, CourseName = "Plumber", CourseLevel = "Advanced"}
                });

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasFailed.Should().BeTrue();
            _authContext.FailureReasons
                .Should()
                .ContainSingle(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner);
        }

        [Test]
        public async Task When_FrameworkRoute_And_StandardCertificateOwned_Then_Fails()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            SetRoute(certificateId.ToString(), CertificatesController.CertificateFrameworkRouteGet);

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns("gov-123");
            _cacheServiceMock
                .Setup(x => x.GetOwnedCertificatesAsync("gov-123"))
                .ReturnsAsync(new List<Certificate>
                {
                    new Certificate { CertificateId = certificateId, CertificateType = CertificateType.Standard, CourseName = "Bricklayer", CourseLevel = "1" }
                });

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasFailed.Should().BeTrue();
            _authContext.FailureReasons
                .Should()
                .ContainSingle(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner);
        }

        [Test]
        public async Task When_NotOwned_Then_Fails()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            SetRoute(certificateId.ToString(), CertificatesController.CertificateStandardRouteGet);

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns("gov-123");
            _cacheServiceMock
                .Setup(x => x.GetOwnedCertificatesAsync("gov-123"))
                .ReturnsAsync(new List<Certificate>()); // empty list

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasFailed.Should().BeTrue();
            _authContext.FailureReasons
                .Should()
                .ContainSingle(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner);
        }

        [Test]
        public async Task When_CertificateId_IsInvalid_Then_Fails()
        {
            // Arrange
            SetRoute("not-a-guid", CertificatesController.CertificateStandardRouteGet);

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasFailed.Should().BeTrue();
            _authContext.FailureReasons
                .Should()
                .ContainSingle(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner);
        }

        [Test]
        public async Task When_CertificateId_Missing_Then_Fails()
        {
            // Arrange
            SetRoute(null, CertificatesController.CertificateStandardRouteGet);

            // Act
            await _sut.HandleAsync(_authContext);

            // Assert
            _authContext.HasFailed.Should().BeTrue();
            _authContext.FailureReasons
                .Should()
                .ContainSingle(r => r.Message == DigitalCertificatesAuthorizationFailureMessages.NotCertificateOwner);
        }

        private void SetRoute(string certificateId, string routeName)
        {
            var endpoint = new Endpoint(
                c => Task.CompletedTask,
                new EndpointMetadataCollection(new RouteNameMetadata(routeName)),
                "test");

            _httpContext.SetEndpoint(endpoint);
            if (certificateId != null)
            {
                _httpContext.Request.RouteValues["certificateId"] = certificateId;
            }
        }
    }
}
