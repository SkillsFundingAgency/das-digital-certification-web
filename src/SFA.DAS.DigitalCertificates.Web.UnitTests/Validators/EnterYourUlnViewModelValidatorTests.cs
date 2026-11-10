using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    public class EnterYourUlnViewModelValidatorTests
    {
        private EnterYourUlnViewModelValidator _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _sut = new EnterYourUlnViewModelValidator();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task Requires_Uln(string? uln)
        {
            var model = new EnterYourUlnViewModel
            {
                Uln = uln
            };

            var result = await _sut.ValidateAsync(model);

            var error = result.Errors.Should().ContainSingle().Which;

            error.PropertyName.Should().Be(nameof(model.Uln));
            error.ErrorMessage.Should().Be("Enter your unique learner number");
        }

        [TestCase("123456789")]
        [TestCase("12345678901")]
        [TestCase("-123456789")]
        [TestCase("+123456789")]
        [TestCase("12345a7890")]
        [TestCase("12345.7890")]
        [TestCase("12345 7890")]
        [TestCase("١٢٣٤٥٦٧٨٩٠")]
        public async Task Rejects_Invalid_Uln(string uln)
        {
            var model = new EnterYourUlnViewModel
            {
                Uln = uln
            };

            var result = await _sut.ValidateAsync(model);

            var error = result.Errors.Should().ContainSingle().Which;

            error.PropertyName.Should().Be(nameof(model.Uln));
            error.ErrorMessage.Should().Be(
                "Enter a unique learner number that is 10 digits and contains only numbers");
        }

        [TestCase("1234567890")]
        [TestCase("0123456789")]
        [TestCase(" 1234567890 ")]
        public async Task Accepts_Valid_Uln(string uln)
        {
            var model = new EnterYourUlnViewModel
            {
                Uln = uln
            };

            var result = await _sut.ValidateAsync(model);

            result.IsValid.Should().BeTrue();
        }
    }
}