using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using System;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class AddAddressManualViewModelValidator : AbstractValidator<AddAddressManualViewModel>
    {
        public const string OrganisationLengthError = "Organisation must be up to 200 characters";
        public const string Address1RequiredError = "Enter address line 1";
        public const string Address1LengthError = "Address line 1 must be up to 200 characters";
        public const string Address2LengthError = "Address 2 line must be up to 200 characters";
        public const string TownRequiredError = "Enter a town or city";
        public const string TownLengthError = "Town or city must be up to 200 characters";
        public const string CountyLengthError = "County must be up to 200 characters";
        public const string PostcodeRequiredError = "Enter a postcode";
        public const string PostcodeInvalidError = "Enter a full UK postcode";

        private const string PostcodeRegex = "^(GIR ?0AA|(?:[A-PR-UWYZ][0-9]{1,2}|[A-PR-UWYZ][A-HK-Y][0-9]{1,2}|[A-PR-UWYZ][0-9][A-HJKS-UW]|[A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY]) ?[0-9][ABD-HJLNP-UW-Z]{2})$";

        private readonly ILocationsOrchestrator _locationsOrchestrator;

        public AddAddressManualViewModelValidator(ILocationsOrchestrator locationsOrchestrator)
        {
            _locationsOrchestrator = locationsOrchestrator;
            RuleFor(x => x.Organisation)
                .MaximumLength(200).WithMessage(OrganisationLengthError)
                .When(x => !string.IsNullOrWhiteSpace(x.Organisation));

            RuleFor(x => x.AddressLine1)
                .NotEmpty().WithMessage(Address1RequiredError)
                .MaximumLength(200).WithMessage(Address1LengthError);

            RuleFor(x => x.AddressLine2)
                .MaximumLength(200).WithMessage(Address2LengthError)
                .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

            RuleFor(x => x.TownOrCity)
                .NotEmpty().WithMessage(TownRequiredError)
                .MaximumLength(200).WithMessage(TownLengthError);

            RuleFor(x => x.County)
                .MaximumLength(200).WithMessage(CountyLengthError)
                .When(x => !string.IsNullOrWhiteSpace(x.County));

            RuleFor(x => x.Postcode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(PostcodeRequiredError)
                .Matches(new Regex(PostcodeRegex, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5))).WithMessage(PostcodeInvalidError)
                .MustAsync(BeAValidPostcode).WithMessage(PostcodeInvalidError);
        }

        private async Task<bool> BeAValidPostcode(string postcode, CancellationToken ct)
        {
            var normalized = Normalize(postcode);
            var result = await _locationsOrchestrator.GetLocations(normalized);

            if (result?.Locations == null)
                return false;

            return result.Locations.Any(location => Normalize(location.Postcode) == normalized);
        }

        private static string Normalize(string? value) =>
            (value ?? string.Empty)
                .Replace(" ", string.Empty)
                .ToUpperInvariant();
    }
}
