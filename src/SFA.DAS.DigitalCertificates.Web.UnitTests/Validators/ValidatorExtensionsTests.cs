using FluentAssertions;
using FluentValidation;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    [TestFixture]
    public class ValidatorExtensionsTests
    {
        private InlineValidator<TestModel> _validator;
        private const string ErrorMessage = "Value contains invalid characters";

        [SetUp]
        public void SetUp()
        {
            _validator = new InlineValidator<TestModel>();
            _validator.RuleFor(x => x.Value).MustNotContainHtmlTags(ErrorMessage);
        }

        [TestCase("hello")]
        [TestCase("123 Main St")]
        [TestCase("")]
        [TestCase(null)]
        public void Validate_Passes_When_ValueHasNoAngleBrackets(string value)
        {
            var result = _validator.Validate(new TestModel { Value = value });
            result.IsValid.Should().BeTrue();
        }

        [TestCase("<script>")]
        [TestCase("<")]
        [TestCase(">")]
        [TestCase("hello<world>")]
        [TestCase("<img src=x onerror=alert(1)>")]
        public void Validate_Fails_When_ValueContainsAngleBrackets(string value)
        {
            var result = _validator.Validate(new TestModel { Value = value });
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == ErrorMessage);
        }

        public class TestModel
        {
            public string Value { get; set; }
        }
    }
}
