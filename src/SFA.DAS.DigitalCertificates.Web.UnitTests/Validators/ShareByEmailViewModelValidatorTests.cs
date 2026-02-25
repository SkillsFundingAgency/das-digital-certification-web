using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    [TestFixture]
    public class ShareByEmailViewModelValidatorTests
    {
        private ShareByEmailViewModelValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ShareByEmailViewModelValidator();
        }

        [Test]
        public void When_Email_Is_Empty_Should_Fail_With_NoEmailErrorMessage()
        {
            // Arrange
            var model = new ShareByEmailViewModel { EmailAddress = string.Empty };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EmailAddress)
                .WithErrorMessage(ShareByEmailViewModelValidator.NoEmailErrorMessage);
        }

        [Test]
        public void When_Email_Has_Invalid_Format_Should_Fail_With_InvalidEmailErrorMessage()
        {
            // Arrange
            var model = new ShareByEmailViewModel { EmailAddress = "not-an-email" };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EmailAddress)
                .WithErrorMessage(ShareByEmailViewModelValidator.InvalidEmailErrorMessage);
        }

        [Test]
        public void When_Email_Has_Valid_Format_But_Domain_Invalid_Should_Fail_With_InvalidDomainErrorMessage()
        {
            // Arrange
            var model = new ShareByEmailViewModel { EmailAddress = "user@invalid-domain.test" };

            // Act
            var result = _sut.TestValidate(model);

            // Assert

            result.ShouldHaveValidationErrorFor(x => x.EmailAddress)
                .WithErrorMessage(ShareByEmailViewModelValidator.InvalidDomainErrorMessage);
        }
    }
}
