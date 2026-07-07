using FluentValidation;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public static class ValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string?> MustNotContainHtmlTags<T>(this IRuleBuilder<T, string?> ruleBuilder, string message) =>
            ruleBuilder.Matches(@"^[^<>]*$").WithMessage(message);
    }
}
