using System.IO;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Stub;
using SFA.DAS.DigitalCertificates.Web.Validators;
using SFA.DAS.GovUK.Auth.Configuration;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    [TestFixture]
    public class SignInStubViewModelValidatorTests
    {
        private GovUkOidcConfiguration _config;
        private SignInStubViewModelValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _config = new GovUkOidcConfiguration { RequestedUserInfoClaims = string.Empty };
            _sut = new SignInStubViewModelValidator(_config);
        }

        [Test]
        public void When_NoUserInfoClaimsRequired_ValidModel_ShouldPass()
        {
            // Arrange
            var model = new SignInStubViewModel
            {
                Id = "123",
                Email = "test@example.com",
                Phone = "07000"
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void When_NoUserInfoClaimsRequired_MissingId_ShouldFail()
        {
            var model = new SignInStubViewModel
            {
                Id = "",
                Email = "test@example.com",
                Phone = "07000"
            };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id is required");
        }

        [Test]
        public void When_NoUserInfoClaimsRequired_InvalidEmail_ShouldFail()
        {
            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "not-an-email",
                Phone = "07000"
            };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Enter a valid email address");
        }

        [Test]
        public void When_NoUserInfoClaimsRequired_MissingPhone_ShouldFail()
        {
            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "test@example.com",
                Phone = ""
            };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Phone)
                  .WithErrorMessage("Phone is required");
        }

        [Test]
        public void WhenUserInfoClaimsRequired_UserFileMissing_ShouldFail()
        {
            // Arrange
            _config.RequestedUserInfoClaims = "CoreIdentityJWT";
            _sut = new SignInStubViewModelValidator(_config);

            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "test@example.com",
                Phone = "07000",
                UserFile = null
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserFile)
                .WithErrorMessage("You must upload a JSON file with verified identity information");
        }

        [Test]
        public void WhenUserInfoClaimsRequired_EmptyFile_ShouldFail()
        {
            _config.RequestedUserInfoClaims = "CoreIdentityJWT";
            _sut = new SignInStubViewModelValidator(_config);

            var emptyFile = CreateFormFile("person.json", "");
            emptyFile.Length.Should().Be(0);

            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "test@example.com",
                Phone = "07000",
                UserFile = emptyFile
            };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.UserFile)
                .WithErrorMessage("The uploaded file must not be empty");
        }

        [Test]
        public void WhenUserInfoClaimsRequired_NonJsonFile_ShouldFail()
        {
            _config.RequestedUserInfoClaims = "CoreIdentityJWT";
            _sut = new SignInStubViewModelValidator(_config);

            var file = CreateFormFile("info.txt");

            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "test@example.com",
                Phone = "07000",
                UserFile = file
            };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.UserFile)
                .WithErrorMessage("Only JSON files can be uploaded");
        }

        [Test]
        public void WhenUserInfoClaimsRequired_ValidJsonFile_ShouldPass()
        {
            _config.RequestedUserInfoClaims = "CoreIdentityJWT";
            _sut = new SignInStubViewModelValidator(_config);

            var file = CreateFormFile("test.json", "{ }");

            var model = new SignInStubViewModel
            {
                Id = "1",
                Email = "test@example.com",
                Phone = "07000",
                UserFile = file
            };

            var result = _sut.TestValidate(model);

            result.IsValid.Should().BeTrue();
        }

        private static IFormFile CreateFormFile(string fileName, string content = "{}")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName);
        }
    }
}
