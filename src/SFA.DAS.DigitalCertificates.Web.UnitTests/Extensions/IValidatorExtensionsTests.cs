using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Extensions
{
    [TestFixture]
    public class IValidatorExtensionsTests
    {
        [Test]
        public async Task ModelStateIsValid_Adds_Errors_When_Validation_Fails()
        {
            // Arrange
            var model = new TestModel { Name = string.Empty };
            var modelState = new ModelStateDictionary();

            var failures = new[] { new ValidationFailure(nameof(TestModel.Name), "Name is required") };
            var validator = new Mock<IValidator<TestModel>>();
            validator.Setup(v => v.ValidateAsync(model, default)).ReturnsAsync(new ValidationResult(failures));

            // Act
            var isValid = await validator.Object.ModelStateIsValid(model, modelState);

            // Assert
            isValid.Should().BeFalse();
            modelState.IsValid.Should().BeFalse();
            modelState[nameof(TestModel.Name)]!.Errors.Should()
                .Contain(e => e.ErrorMessage == "Name is required");
        }

        [Test]
        public async Task ModelStateIsValid_Returns_True_When_No_Errors()
        {
            // Arrange
            var model = new TestModel { Name = "John" };
            var modelState = new ModelStateDictionary();

            var validator = new Mock<IValidator<TestModel>>();
            validator.Setup(v => v.ValidateAsync(model, default))
                     .ReturnsAsync(new ValidationResult()); // valid

            // Act
            var isValid = await validator.Object.ModelStateIsValid(model, modelState);

            // Assert
            isValid.Should().BeTrue();
            modelState.IsValid.Should().BeTrue();
        }

        public class TestModel
        {
            public string Name { get; set; }
        }
    }
}
