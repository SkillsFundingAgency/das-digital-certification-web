using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Exceptions;
using SFA.DAS.DigitalCertificates.Web.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Home;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IHomeOrchestrator> _orchestratorMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IGovUkAuthenticationService> _govUkAuthServiceMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<ISessionService> _sessionServiceMock;
        private HomeController _sut;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _orchestratorMock = new Mock<IHomeOrchestrator>();
            _configMock = new Mock<IConfiguration>();
            _govUkAuthServiceMock = new Mock<IGovUkAuthenticationService>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _sessionServiceMock = new Mock<ISessionService>();

            _httpContext = new DefaultHttpContext();
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);
            _sessionServiceMock.Setup(s => s.SetUserDetailsAsync(It.IsAny<UserDetails>())).Returns(Task.CompletedTask);

            _sut = new HomeController(
                _orchestratorMock.Object,
                _configMock.Object,
                _govUkAuthServiceMock.Object,
                _contextAccessorMock.Object,
                _loggerMock.Object,
                _sessionServiceMock.Object);
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();

        [Test]
        public void Index_ShouldReturnView_WhenRunningLocallyOrDev()
        {
            _configMock.Setup(c => c["EnvironmentName"]).Returns("LOCAL");
            var localResult = _sut.Index() as ViewResult;
            localResult.Should().NotBeNull();

            _configMock.Setup(c => c["EnvironmentName"]).Returns("DEV");
            var devResult = _sut.Index() as ViewResult;
            devResult.Should().NotBeNull();
        }

        [Test]
        public void Index_ShouldRedirect_WhenRunningInProd()
        {
            // Arrange
            _configMock.Setup(c => c["EnvironmentName"]).Returns("PROD");

            // Act
            var result = _sut.Index();

            // Assert
            var redirect = result as RedirectToRouteResult;
            redirect.Should().NotBeNull();
            redirect!.RouteName.Should().Be(HomeController.CheckRouteGet);
        }

        [Test]
        public void Check_ShouldReturnView()
        {
            var result = _sut.Check() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void Locked_ShouldReturnView()
        {
            var result = _sut.Locked() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void Cookies_ShouldReturnView()
        {
            var result = _sut.Cookies() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void CookieDetails_ShouldReturnView()
        {
            var result = _sut.CookieDetails() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public async Task Verified_Should_CreateUser_And_Redirect_To_CertificatesList()
        {
            // Arrange
            var govUkUser = new GovUkUser
            {
                Sub = "sub-123",
                Email = "user@example.com",
                PhoneNumber = "07123",
                CoreIdentityJwt = new GovUkCoreIdentityJwt
                {
                    Sub = "sub-123",
                    Vot = "P2",
                    Vc = new GovUkCoreIdentityCredential
                    {
                        CredentialSubject = new GovUkCredentialSubject
                        {
                            BirthDates = new List<GovUkBirthDateEntry>
                            {
                                new GovUkBirthDateEntry
                                {
                                    Value = "1990-01-01",
                                    ValidUntilRaw = "2025-01-01"
                                }
                            },
                            Names = new List<GovUkName>
                            {
                                new GovUkName
                                {
                                    ValidFromRaw = "2020-01-01",
                                    ValidUntilRaw = "2022-01-01",
                                    NameParts = new List<GovUkNamePart>
                                    {
                                        new GovUkNamePart
                                        {
                                            Type = "GivenName",
                                            Value = "John"
                                        },
                                        new GovUkNamePart
                                        {
                                            Type = "FamilyName",
                                            Value = "Smith"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            _govUkAuthServiceMock
                .Setup(s => s.GetAccountDetails(It.IsAny<string>()))
                .ReturnsAsync(govUkUser);

            // Mock IAuthenticationService so GetTokenAsync works
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(
                    new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(), "Cookies")));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            _contextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = await _sut.Verified();

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect!.RouteName.Should().Be(CertificatesController.CertificatesListRouteGet);

            _orchestratorMock.Verify(o => o.CreateOrUpdateUser(It.Is<CreateOrUpdateUserModel>(m =>
                m.GovUkIdentifier == "sub-123" &&
                m.EmailAddress == "user@example.com" &&
                m.PhoneNumber == "07123" &&
                m.Names.Count == 1 &&
                m.Names[0].FamilyName == "Smith" &&
                m.Names[0].GivenNames == "John" &&
                m.DateOfBirth.HasValue &&
                m.DateOfBirth.Value.Year == 1990)), Times.Once);
        }

        [Test]
        public void Verified_ShouldThrow_When_AccountDetails_Are_Null()
        {
            // Arrange
            _govUkAuthServiceMock
                .Setup(s => s.GetAccountDetails(It.IsAny<string>()))
                .ReturnsAsync((GovUkUser)null);

            // Act
            Func<Task> act = _sut.Verified;

            // Assert
            act.Should().ThrowAsync<VerifyException>()
                .WithMessage("Unable to load verify details");
        }


        [Test]
        public void AccessDenied_ShouldReturnView()
        {
            var result = _sut.AccessDenied() as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void Error_ShouldLogError_And_ReturnView()
        {
            // Arrange
            var errorMessage = "Test error message";
            var httpContext = new DefaultHttpContext { TraceIdentifier = "TestTraceIdentifier" };
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);

            // Act
            var result = _sut.Error(errorMessage) as ViewResult;

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(errorMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            result.Should().NotBeNull();
            var model = result.Model as ErrorViewModel;
            model.Should().NotBeNull();
            model!.RequestId.Should().Be("TestTraceIdentifier");
            model.ErrorMessage.Should().Be(errorMessage);
        }
    }
}
