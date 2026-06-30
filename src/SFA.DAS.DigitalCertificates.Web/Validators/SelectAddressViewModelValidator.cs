using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using System.Linq;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class SelectAddressViewModelValidator : AbstractValidator<SelectAddressViewModel>
    {
        public const string EnterAddressErrorMessage = "Enter the first 3 letters of an address or postcode and select a location";
        public const string SelectValidAddressErrorMessage = "Select a valid address";
        public const string SearchTermInvalidCharsError = "Search term contains invalid characters";

        private readonly ILocationsOrchestrator _locationsOrchestrator;

        public SelectAddressViewModelValidator(ILocationsOrchestrator locationsOrchestrator)
        {
            _locationsOrchestrator = locationsOrchestrator;

            RuleFor(x => x.SearchTerm)
                .MustNotContainHtmlTags(SearchTermInvalidCharsError)
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

            RuleFor(x => x).CustomAsync(async (model, context, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(model.SearchTerm) || model.SearchTerm.Length < 3)
                {
                    context.AddFailure(nameof(model.SearchTerm), EnterAddressErrorMessage);
                    return;
                }

                if (model.SearchTerm.Contains('<') || model.SearchTerm.Contains('>'))
                {
                    return;
                }

                var searchResult = await _locationsOrchestrator.GetLocations(model.SearchTerm ?? string.Empty);
                if (searchResult == null || searchResult.Locations == null || !searchResult.Locations.Any())
                {
                    context.AddFailure(nameof(model.SearchTerm), SelectValidAddressErrorMessage);
                    return;
                }

                var termMatches = searchResult.Locations.Any(location => string.Equals(location.Name?.Trim(), model.SearchTerm?.Trim(), System.StringComparison.OrdinalIgnoreCase));
                if (!termMatches)
                {
                    context.AddFailure(nameof(model.SearchTerm), SelectValidAddressErrorMessage);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(model.SelectedAddress))
                {
                    if (!string.Equals(model.SearchTerm?.Trim(), model.SelectedAddress?.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    {
                        context.AddFailure(nameof(model.SelectedAddress), SelectValidAddressErrorMessage);
                    }
                }
            });
        }
    }
}
