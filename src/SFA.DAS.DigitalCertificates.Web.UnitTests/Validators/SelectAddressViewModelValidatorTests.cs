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
    public class SelectAddressViewModelValidatorTests
    {
        private Mock<ILocationsOrchestrator> _locationsOrchestrator;
        private SelectAddressViewModelValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _locationsOrchestrator = new Mock<ILocationsOrchestrator>();
            _validator = new SelectAddressViewModelValidator(_locationsOrchestrator.Object);
        }

        [Test]
        public async Task Validate_Fails_When_SearchTermTooShort()
        {
            // Arrange
            var model = new SelectAddressViewModel { SearchTerm = "ab" };

            // Act
            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == SelectAddressViewModelValidator.EnterAddressErrorMessage);
        }

        [Test]
        public async Task Validate_Fails_When_NoResults()
        {
            // Arrange
            var model = new SelectAddressViewModel { SearchTerm = "abc" };
            _locationsOrchestrator.Setup(x => x.GetLocations("abc")).ReturnsAsync(new GetLocationsQueryResult());

            // Act
            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == SelectAddressViewModelValidator.SelectValidAddressErrorMessage);
        }

        [Test]
        public async Task Validate_Passes_When_MatchingResultAndSelectedAddressMatches()
        {
            // Arrange
            var model = new SelectAddressViewModel { SearchTerm = "Match Address", SelectedAddress = "Match Address" };
            _locationsOrchestrator.Setup(x => x.GetLocations("Match Address")).ReturnsAsync(new GetLocationsQueryResult
            {
                Locations = new[] { new LocationResult { Name = "Match Address", Postcode = "X" } }
            });

            // Act
            var result = await _validator.ValidateAsync(model);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
