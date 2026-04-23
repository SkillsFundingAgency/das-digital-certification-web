using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Validators;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Validators
{
    public class AddAddressManualViewModelValidatorTests
    {
        private Mock<ILocationsOrchestrator> _locationsOrchestrator;
        private AddAddressManualViewModelValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _locationsOrchestrator = new Mock<ILocationsOrchestrator>();
            _validator = new AddAddressManualViewModelValidator(_locationsOrchestrator.Object);
        }

        [Test]
        public async Task Validate_Fails_When_AddressLine1Missing()
        {
            // Arrange
            var model = new AddAddressManualViewModel { TownOrCity = "Town", Postcode = "AB1 2CD" };

            // Act
            var result = await _validator.ValidateAsync(model, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == AddAddressManualViewModelValidator.Address1RequiredError);
        }

        [Test]
        public async Task Validate_Fails_When_PostcodeNotFound()
        {
            // Arrange
            var model = new AddAddressManualViewModel { AddressLine1 = "Line1", TownOrCity = "Town", Postcode = "ZZ1 1ZZ" };
            _locationsOrchestrator.Setup(x => x.GetLocations("ZZ11ZZ")).ReturnsAsync(new GetLocationsQueryResult());

            // Act
            var result = await _validator.ValidateAsync(model, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == AddAddressManualViewModelValidator.PostcodeInvalidError || e.ErrorMessage == AddAddressManualViewModelValidator.PostcodeRequiredError);
        }

        [Test]
        public async Task Validate_Passes_When_AllValidAnd_PostcodeMatches()
        {
            // Arrange
            var model = new AddAddressManualViewModel { AddressLine1 = "Line1", TownOrCity = "Town", Postcode = "AB1 2BD" };
            _locationsOrchestrator.Setup(x => x.GetLocations("AB12BD")).ReturnsAsync(new GetLocationsQueryResult
            {
                Locations = new[] { new LocationResult { Name = "Addr", Postcode = "AB1 2BD" } }
            });

            // Act
            var result = await _validator.ValidateAsync(model, CancellationToken.None);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
