using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Web.Services;
using System.Linq;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Enums;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("authorise")]
    public class AuthoriseController : BaseController
    {
        #region Routes
        public const string NeedMoreInformationRouteGet = nameof(NeedMoreInformationRouteGet);
        public const string NeedMoreInformationContinueRoutePost = nameof(NeedMoreInformationContinueRoutePost);
        public const string KnowYourUlnRouteGet = nameof(KnowYourUlnRouteGet);
        public const string KnowYourUlnRoutePost = nameof(KnowYourUlnRoutePost);
        public const string SelectCourseRouteGet = nameof(SelectCourseRouteGet);
        public const string SelectCourseRoutePost = nameof(SelectCourseRoutePost);
        public const string SelectProviderRouteGet = nameof(SelectProviderRouteGet);
        public const string SelectProviderRoutePost = nameof(SelectProviderRoutePost);
        public const string CheckAnswersRouteGet = nameof(CheckAnswersRouteGet);
        public const string CheckAnswersRoutePost = nameof(CheckAnswersRoutePost);
        public const string KnowYearRouteGet = nameof(KnowYearRouteGet);
        public const string KnowYearRoutePost = nameof(KnowYearRoutePost);
        public const string CannotMatchRouteGet = nameof(CannotMatchRouteGet);
        #endregion

        private readonly ISessionService _sessionService;
        private readonly IAuthoriseOrchestrator _authoriseOrchestrator;

        public AuthoriseController(IHttpContextAccessor httpContextAccessor, ISessionService sessionService, IAuthoriseOrchestrator authoriseOrchestrator)
            : base(httpContextAccessor)
        {
            _sessionService = sessionService;
            _authoriseOrchestrator = authoriseOrchestrator;
        }

        [HttpGet("need-more-information", Name = NeedMoreInformationRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> NeedMoreInformation()
        {
            var hasMatches = await _authoriseOrchestrator.PrepareNeedMoreInformationAsync();

            if (!hasMatches)
            {
                return await RedirectToCannotMatchAsync();
            }

            return View();
        }

        [HttpPost("need-more-information", Name = NeedMoreInformationContinueRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> NeedMoreInformationContinue()
        {
            await _sessionService.ClearAuthorisationAnswersAsync();

            return RedirectToRoute(KnowYourUlnRouteGet);
        }

        [HttpGet("know-your-uln", Name = KnowYourUlnRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYourUln()
        {
            var model = await _authoriseOrchestrator.GetKnowYourUlnViewModelAsync();
            return View(model);
        }

        [HttpPost("know-your-uln", Name = KnowYourUlnRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYourUln(KnowYourUlnViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateKnowYourUlnViewModel(model, ModelState))
            {
                return RedirectToRoute(KnowYourUlnRouteGet);
            }
            var updatedModel = await _authoriseOrchestrator.SaveKnowYourUlnAsync(model);

            var matchOutcome = await _authoriseOrchestrator.GetUlnMatchOutcomeAsync(updatedModel);

            if (model?.IsReturningToCheck == true &&
                model.IsShortJourney != true &&
                matchOutcome == MatchOutcome.NoMatch)
            {
                return RedirectToRoute(CheckAnswersRouteGet);
            }

            switch (matchOutcome)
            {
                case MatchOutcome.NoData:
                case MatchOutcome.NoMatch:
                    return RedirectToRoute(SelectCourseRouteGet);
                case MatchOutcome.MultipleMatches:
                case MatchOutcome.SingleMatch:
                    return RedirectToRoute(CheckAnswersRouteGet);
                default:
                    return await RedirectToCannotMatchAsync();
            }
        }

        [HttpGet("select-course", Name = SelectCourseRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectCourse()
        {
            var model = await _authoriseOrchestrator.GetSelectCourseViewModelAsync();
            if (model == null || model.Courses == null || !model.Courses.Any())
            {
                return await RedirectToCannotMatchAsync();
            }

            return View(model);
        }

        [HttpPost("select-course", Name = SelectCourseRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectCourse(SelectCourseViewModel model)
        {
            if (string.Equals(model.SelectedCourseCode?.Trim(), SelectCourseViewModel.UnknownCourseSentinel, System.StringComparison.OrdinalIgnoreCase))
            {
                model.SelectedCourseUnknown = true;
                model.SelectedCourseCode = null;
            }
            if (!await _authoriseOrchestrator.ValidateSelectCourseViewModel(model, ModelState))
            {
                return RedirectToRoute(SelectCourseRouteGet);
            }

            var updatedModel = await _authoriseOrchestrator.SaveSelectedCourseAsync(model);
            var matchOutcome = await _authoriseOrchestrator.GetCourseMatchOutcomeAsync(updatedModel);

            if (model?.IsReturningToCheck == true &&
                model.IsShortJourney != true &&
                matchOutcome == MatchOutcome.NoMatch)
            {
                return RedirectToRoute(CheckAnswersRouteGet);
            }

            switch (matchOutcome)
            {
                case MatchOutcome.NoData:
                    return await RedirectToCannotMatchAsync();
                case MatchOutcome.NoMatch:
                    return RedirectToRoute(KnowYearRouteGet);
                case MatchOutcome.MultipleMatches:
                case MatchOutcome.SingleMatch:
                    return RedirectToRoute(CheckAnswersRouteGet);
                default:
                    return RedirectToRoute(CannotMatchRouteGet);
            }
        }

        [HttpGet("select-provider", Name = SelectProviderRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectProvider()
        {
            var model = await _authoriseOrchestrator.GetSelectProviderViewModelAsync();
            if (model == null || model.Providers == null || !model.Providers.Any())
            {
                return await RedirectToCannotMatchAsync();
            }

            return View(model);
        }

        [HttpPost("select-provider", Name = SelectProviderRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectProvider(SelectProviderViewModel model)
        {
            if (string.Equals(model.SelectedProviderName?.Trim(), SelectProviderViewModel.UnknownProviderSentinel, System.StringComparison.OrdinalIgnoreCase))
            {
                model.SelectedProviderUnknown = true;
                model.SelectedProviderName = null;
            }

            if (!await _authoriseOrchestrator.ValidateSelectProviderViewModel(model, ModelState))
            {
                return RedirectToRoute(SelectProviderRouteGet);
            }

            await _authoriseOrchestrator.SaveSelectedProviderAsync(model);

            return RedirectToRoute(CheckAnswersRouteGet);
        }

        [HttpGet("check-answers", Name = CheckAnswersRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> CheckAnswers()
        {
            var model = await _authoriseOrchestrator.GetCheckAnswersViewModelAsync();

            if (model == null)
            {
                return RedirectToRoute(NeedMoreInformationRouteGet);
            }

            model.BackLinkRouteName = model.IsShortJourney ? SelectCourseRouteGet : SelectProviderRouteGet;

            return View(model);
        }

        [HttpPost("check-answers", Name = CheckAnswersRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> CheckAnswersPost()
        {
            var outcome = await _authoriseOrchestrator.SubmitCheckAnswersAsync();

            switch (outcome)
            {
                case MatchOutcome.SingleMatch:
                    return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
                case MatchOutcome.MultipleMatches:
                    return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
                case MatchOutcome.Locked:
                    return await RedirectToCannotMatchAsync();
                case MatchOutcome.NoMatch:
                default:
                    return HandleNoMatch();
            }
        }

        private async Task<IActionResult> RedirectToCannotMatchAsync()
        {
            await _sessionService.ClearAuthorisationAnswersAsync();
            return RedirectToRoute(CannotMatchRouteGet);
        }

        private IActionResult HandleNoMatch()
        {
            TempData.AddFlashMessageWithDetail(
                "We cannot match your information to any results.",
                "Check your answers. If you need to make changes we can try to match your results, or you can submit again.",
                TempDataDictionaryExtensions.FlashMessageLevel.Warning);

            return RedirectToRoute(CheckAnswersRouteGet);
        }

        [HttpGet("cannot-match", Name = CannotMatchRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public IActionResult CannotMatch()
        {
            return View();
        }

        [HttpGet("know-year", Name = KnowYearRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYear()
        {
            var model = await _authoriseOrchestrator.GetKnowYearViewModelAsync();

            if (model == null)
            {
                return RedirectToRoute(NeedMoreInformationRouteGet);
            }

            return View(model);
        }

        [HttpPost("know-year", Name = KnowYearRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYear(KnowYearViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateKnowYearViewModel(model, ModelState))
            {
                return RedirectToRoute(KnowYearRouteGet);
            }

            var updatedModel = await _authoriseOrchestrator.SaveKnowYearAsync(model);

            return RedirectToRoute(
             updatedModel?.IsReturningToCheck == true && updatedModel.ProviderSelected
                 ? CheckAnswersRouteGet
                 : SelectProviderRouteGet
         );
        }
    }
}
