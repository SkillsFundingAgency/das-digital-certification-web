using FluentValidation.TestHelper;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    [TestFixture]
    public class SelectProviderViewModelValidatorTests
    {
        private SelectProviderViewModelValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SelectProviderViewModelValidator();
        }

        [Test]
        public void When_NoSelection_And_NotUnknown_Should_Fail_With_SelectProviderErrorMessage()
        {
            var model = new SelectProviderViewModel { SelectedProviderName = string.Empty, SelectedProviderUnknown = false };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.SelectedProviderName)
                .WithErrorMessage(SelectProviderViewModelValidator.SelectProviderErrorMessage);
        }

        [Test]
        public void When_Unknown_Selected_Should_Pass()
        {
            var model = new SelectProviderViewModel { SelectedProviderName = null, SelectedProviderUnknown = true };

            var result = _sut.TestValidate(model);

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void When_Provider_Selected_Should_Pass()
        {
            var model = new SelectProviderViewModel { SelectedProviderName = "Provider A", SelectedProviderUnknown = false };

            var result = _sut.TestValidate(model);

            result.IsValid.Should().BeTrue();
        }
    }
}
