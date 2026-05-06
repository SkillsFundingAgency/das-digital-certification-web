using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    [TestFixture]
    public class KnowYearViewModelValidatorTests
    {
        private KnowYearViewModelValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new KnowYearViewModelValidator();
        }

        [Test]
        public void Validate_Should_Fail_When_KnowYear_Is_Null()
        {
            var vm = new KnowYearViewModel { KnowYear = null, YearCompleted = null };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == KnowYearViewModelValidator.SelectYesIfYouKnowError);
        }

        [Test]
        public void Validate_Should_Fail_When_KnowYear_True_And_YearMissing()
        {
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = null };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == KnowYearViewModelValidator.EnterYearError);
        }

        [Test]
        public void Validate_Should_Fail_When_Year_Is_Not_4_Digits()
        {
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 123 };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == KnowYearViewModelValidator.InvalidYearFormatError);
        }

        [Test]
        public void Validate_Should_Fail_When_Year_Is_Later_Than_Current()
        {
            var nextYear = DateTime.UtcNow.Year + 1;
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = nextYear };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == KnowYearViewModelValidator.YearLaterThanCurrentError);
        }

        [Test]
        public void Validate_Should_Fail_When_Year_Is_Before_2012()
        {
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 2011 };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == KnowYearViewModelValidator.YearEarlierThan2012Error);
        }

        [Test]
        public void Validate_Should_Pass_When_KnowYear_False_Even_If_YearProvided()
        {
            var vm = new KnowYearViewModel { KnowYear = false, YearCompleted = 2000 };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void Validate_Should_Pass_When_KnowYear_True_And_Valid_Year()
        {
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 2015 };

            var result = _validator.Validate(vm);

            result.IsValid.Should().BeTrue();
        }
    }
}
