using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    public class KnowYourUlnViewModelValidatorTests
    {
        private KnowYourUlnViewModelValidator _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _sut = new KnowYourUlnViewModelValidator();
        }

        [Test]
        public async Task Requires_An_Answer_To_KnowUln()
        {
            var model = new KnowYourUlnViewModel
            {
                KnowUln = null
            };

            var result = await _sut.ValidateAsync(model);

            var error = result.Errors.Should().ContainSingle().Which;

            error.PropertyName.Should().Be(nameof(model.KnowUln));
            error.ErrorMessage.Should()
                .Be("Select 'Yes' if you know your unique learner number");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task Requires_Uln_When_Yes_Selected(string? uln)
        {
            var model = new KnowYourUlnViewModel
            {
                KnowUln = true,
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
        public async Task Rejects_Invalid_Uln_When_Yes_Selected(string uln)
        {
            var model = new KnowYourUlnViewModel
            {
                KnowUln = true,
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
        public async Task Accepts_Valid_Uln_When_Yes_Selected(string uln)
        {
            var model = new KnowYourUlnViewModel
            {
                KnowUln = true,
                Uln = uln
            };

            var result = await _sut.ValidateAsync(model);

            result.IsValid.Should().BeTrue();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not a ULN")]
        public async Task Ignores_Uln_When_No_Selected(string? uln)
        {
            var model = new KnowYourUlnViewModel
            {
                KnowUln = false,
                Uln = uln
            };

            var result = await _sut.ValidateAsync(model);

            result.IsValid.Should().BeTrue();
        }
    }
}