using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.DigitalCertificates.Web.Models.Stub;
using SFA.DAS.GovUK.Auth.Exceptions;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    [TestFixture]
    public class StubControllerTests
    {
        private Mock<IConfiguration> _configMock;
        private Mock<IStubAuthenticationService> _stubAuthServiceMock;
        private Mock<IValidator<SignInStubViewModel>> _validatorMock;
        private StubController _sut;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _configMock = new Mock<IConfiguration>();
            _stubAuthServiceMock = new Mock<IStubAuthenticationService>();
            _validatorMock = new Mock<IValidator<SignInStubViewModel>>();

            _sut = new StubController(_configMock.Object, _stubAuthServiceMock.Object, _validatorMock.Object);

            _httpContext = new DefaultHttpContext();

            var urlHelperFactoryMock = new Mock<Microsoft.AspNetCore.Mvc.Routing.IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(Mock.Of<IUrlHelper>());
            _sut.Url = urlHelperFactoryMock.Object.GetUrlHelper(_sut.ControllerContext);

            _sut.ControllerContext = new ControllerContext { HttpContext = _httpContext };
        }


        [TearDown]
        public void TearDown() => _sut.Dispose();

        [Test]
        public void SignInStub_Returns_View_With_Configured_Defaults()
        {
            // Arrange
            _configMock.Setup(c => c["StubId"]).Returns("stub-id");
            _configMock.Setup(c => c["StubEmail"]).Returns("stub@example.com");
            _configMock.Setup(c => c["StubPhone"]).Returns("0123456789");

            // Act
            var result = _sut.SignInStub("/return") as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.ViewName.Should().Be("SignInStub");

            var model = result.Model.As<SignInStubViewModel>();
            model.Id.Should().Be("stub-id");
            model.Email.Should().Be("stub@example.com");
            model.Phone.Should().Be("0123456789");
            model.ReturnUrl.Should().Be("/return");
        }

        [Test]
        public async Task SignInStubPost_When_Validation_Fails_Redirects_To_SignInStub()
        {
            // Arrange
            var model = new SignInStubViewModel { ReturnUrl = "/back" };
            _validatorMock
                .Setup(v => v.ValidateAsync(model, default))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Email", "Required") }));

            // Act
            var result = await _sut.SignInStubPost(model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be("SignIn-Stub");
            result.RouteValues["ReturnUrl"].Should().Be("/back");

            _stubAuthServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SignInStubPost_When_Valid_Calls_StubService_And_SignsIn()
        {
            // Arrange
            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "user@example.com",
                Phone = "07777",
                UserFile = CreateFakeFormFile(),
                ReturnUrl = "/next"
            };

            _validatorMock.Setup(v => v.ValidateAsync(model, default))
                          .ReturnsAsync(new ValidationResult()); // valid

            var govUkUser = new GovUkUser();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("test", "1") }));

            _stubAuthServiceMock.Setup(s => s.GetStubVerifyGovUkUser(model.UserFile))
                                .ReturnsAsync(govUkUser);
            _stubAuthServiceMock.Setup(s => s.GetStubSignInClaims(It.IsAny<StubAuthUserDetails>()))
                                .ReturnsAsync(principal);

            var authServiceMock = new Mock<IAuthenticationService>();
            var signInCalled = false;

            authServiceMock
                .Setup(a => a.SignInAsync(
                        It.IsAny<HttpContext>(),
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        It.IsAny<AuthenticationProperties>()))
                .Callback(() => signInCalled = true)
                .Returns(Task.CompletedTask);

            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(authServiceMock.Object);
            _httpContext.RequestServices = sp.Object;

            // Act
            var result = await _sut.SignInStubPost(model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be("SignedIn-stub");
            result.RouteValues["ReturnUrl"].Should().Be("/next");
            signInCalled.Should().BeTrue();

            _stubAuthServiceMock.Verify(s => s.GetStubVerifyGovUkUser(model.UserFile), Times.Once);
            _stubAuthServiceMock.Verify(s => s.GetStubSignInClaims(It.IsAny<StubAuthUserDetails>()), Times.Once);
        }

        [Test]
        public async Task SignInStubPost_When_StubVerifyException_Adds_ModelError_And_Redirects_Back()
        {
            // Arrange
            var model = new SignInStubViewModel
            {
                UserFile = CreateFakeFormFile(),
                ReturnUrl = "/error"
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(model, default))
                .ReturnsAsync(new ValidationResult());

            _stubAuthServiceMock
                .Setup(s => s.GetStubVerifyGovUkUser(It.IsAny<IFormFile>()))
                .ThrowsAsync(new StubVerifyException("invalid user file"));

            // Act
            var result = await _sut.SignInStubPost(model) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be("SignIn-Stub");
            result.RouteValues["ReturnUrl"].Should().Be("/error");

            _sut.ModelState[nameof(model.UserFile)]!.Errors.Should()
                .Contain(e => e.ErrorMessage == "invalid user file");
        }

        [Test]
        public void SignedInStub_Returns_View_With_ReturnUrl_Model()
        {
            // Act
            var result = _sut.SignedInStub("/home") as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result!.Model.Should().Be("/home");
        }

        private static IFormFile CreateFakeFormFile(string fileName = "user.json", string content = "{}")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new System.IO.MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName);
        }
    }
}
