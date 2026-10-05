using FluentValidation.TestHelper;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Validators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    [TestFixture]
    public class SelectCourseViewModelValidatorTests
    {
        private SelectCourseViewModelValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SelectCourseViewModelValidator();
        }

        [Test]
        public void When_NoSelection_Must_Fail_With_SelectCourseErrorMessage()
        {
            var model = new SelectCourseViewModel { SelectedCourseCode = string.Empty };

            var result = _sut.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.SelectedCourseCode)
                .WithErrorMessage(SelectCourseViewModelValidator.SelectCourseErrorMessage);
        }

        [Test]
        public void When_Selection_Must_Pass()
        {
            var model = new SelectCourseViewModel { SelectedCourseCode = "ABC" };

            var result = _sut.TestValidate(model);

            result.IsValid.Should().BeTrue();
        }
    }
}
