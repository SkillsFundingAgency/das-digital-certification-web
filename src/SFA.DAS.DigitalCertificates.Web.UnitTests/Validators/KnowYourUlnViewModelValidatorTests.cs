using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    public class KnowYourUlnViewModelValidatorTests
    {
        private KnowYourUlnViewModelValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new KnowYourUlnViewModelValidator();
        }

        [Test]
        public async Task Should_have_error_when_KnowUln_not_selected()
        {
            var vm = new KnowYourUlnViewModel { KnowUln = null, Uln = null };

            var result = await _validator.ValidateAsync(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "KnowUln");
        }

        [Test]
        public async Task Should_have_error_when_KnowUln_yes_and_Uln_null()
        {
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = null };

            var result = await _validator.ValidateAsync(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Uln");
        }

        [Test]
        public async Task Should_have_error_when_KnowUln_yes_and_Uln_not_10_digits()
        {
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = 12345L };

            var result = await _validator.ValidateAsync(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Uln");
        }

        [Test]
        public async Task Should_be_valid_when_KnowUln_yes_and_Uln_10_digits()
        {
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = 1234567890L };

            var result = await _validator.ValidateAsync(vm);

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public async Task Should_be_valid_when_KnowUln_no_and_Uln_null()
        {
            var vm = new KnowYourUlnViewModel { KnowUln = false, Uln = null };

            var result = await _validator.ValidateAsync(vm);

            result.IsValid.Should().BeTrue();
        }
    }
}
