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
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("authorise")]
    public class AuthoriseController : BaseController
    {
        #region Routes
        public const string NeedMoreInformationRouteGet = nameof(NeedMoreInformationRouteGet);
        public const string NeedMoreInformationContinueRoutePost = nameof(NeedMoreInformationContinueRoutePost);
        public const string EnterYourUlnRouteGet = nameof(EnterYourUlnRouteGet);
        public const string EnterYourUlnRoutePost = nameof(EnterYourUlnRoutePost);
        public const string KnowYourUlnRouteGet = nameof(KnowYourUlnRouteGet);
        public const string KnowYourUlnRoutePost = nameof(KnowYourUlnRoutePost);
        public const string ChangeCourseRouteGet = nameof(ChangeCourseRouteGet);
        public const string SelectCourseRouteGet = nameof(SelectCourseRouteGet);
        public const string SelectCourseRoutePost = nameof(SelectCourseRoutePost);
        public const string SelectProviderRouteGet = nameof(SelectProviderRouteGet);
        public const string SelectProviderRoutePost = nameof(SelectProviderRoutePost);
        public const string CheckAnswersRouteGet = nameof(CheckAnswersRouteGet);
        public const string CheckAnswersRoutePost = nameof(CheckAnswersRoutePost);
        public const string KnowYearRouteGet = nameof(KnowYearRouteGet);
        public const string KnowYearRoutePost = nameof(KnowYearRoutePost);
        public const string CannotMatchRouteGet = nameof(CannotMatchRouteGet);
        public const string LockedOutRouteGet = nameof(LockedOutRouteGet);
        public const string NotFoundRouteGet = nameof(NotFoundRouteGet);
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
                return await RedirectToNotFoundAsync();
            }

            return View();
        }

        [HttpPost("need-more-information", Name = NeedMoreInformationContinueRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> NeedMoreInformationContinue()
        {
            await _sessionService.ClearAuthorisationAnswersAsync();

            var distinctUlnCount =
                await _authoriseOrchestrator.GetDistinctUlnCountAsync();

            switch (distinctUlnCount)
            {
                case 0:
                    return await RedirectToNotFoundAsync();

                case 1:
                    return RedirectToRoute(KnowYourUlnRouteGet);

                default:
                    return RedirectToRoute(EnterYourUlnRouteGet);
            }
        }

        [HttpGet("enter-your-uln", Name = EnterYourUlnRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> EnterYourUln()
        {
            var model = await _authoriseOrchestrator.GetEnterYourUlnViewModelAsync();
            return View(model);
        }

        [HttpPost("enter-your-uln", Name = EnterYourUlnRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> EnterYourUln(EnterYourUlnViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateEnterYourUlnViewModel(model, ModelState))
            {
                return RedirectToRoute(EnterYourUlnRouteGet);
            }

            var updatedModel =
                await _authoriseOrchestrator.SaveEnterYourUlnAsync(model);

            return await RedirectAfterUlnUpdateAsync(updatedModel, hasUln: true);
        }

        [HttpGet("know-your-uln", Name = KnowYourUlnRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYourUln()
        {
            var distinctUlnCount =
                await _authoriseOrchestrator.GetDistinctUlnCountAsync();

            switch (distinctUlnCount)
            {
                case 0:
                    return await RedirectToNotFoundAsync();

                case 1:
                    var model =
                        await _authoriseOrchestrator.GetKnowYourUlnViewModelAsync();

                    return View(model);

                default:
                    return RedirectToRoute(EnterYourUlnRouteGet);
            }
        }

        [HttpPost("know-your-uln", Name = KnowYourUlnRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYourUln(KnowYourUlnViewModel model)
        {
            var distinctUlnCount =
                await _authoriseOrchestrator.GetDistinctUlnCountAsync();

            switch (distinctUlnCount)
            {
                case 0:
                    return await RedirectToNotFoundAsync();

                case 1:
                    break;

                default:
                    return RedirectToRoute(EnterYourUlnRouteGet);
            }

            if (!await _authoriseOrchestrator.ValidateKnowYourUlnViewModel(model, ModelState))
            {
                return RedirectToRoute(KnowYourUlnRouteGet);
            }

            var updatedModel =
                await _authoriseOrchestrator.SaveKnowYourUlnAsync(model);

            return await RedirectAfterUlnUpdateAsync(
                updatedModel,
                hasUln: updatedModel.KnowUln == true);
        }

        private async Task<IActionResult> RedirectAfterUlnUpdateAsync(
            AuthoriseViewModelBase model,
            bool hasUln)
        {
            var route = SelectCourseRouteGet;

            if (model.IsReturningToCheck)
            {
                if (!model.IsShortJourney)
                {
                    route = CheckAnswersRouteGet;
                }
                else if (hasUln)
                {
                    var matchOutcome =
                        await _authoriseOrchestrator.GetUlnMatchOutcomeAsync();

                    switch (matchOutcome)
                    {
                        case MatchOutcome.SingleMatch:
                            route = CheckAnswersRouteGet;
                            break;

                        case MatchOutcome.NoMatch:
                        case MatchOutcome.MultipleMatches:
                            route = SelectCourseRouteGet;
                            break;

                        case MatchOutcome.NoData:
                        default:
                            return await RedirectToNotFoundAsync();
                    }
                }
            }

            return RedirectToRoute(route);
        }

        [HttpGet("change-course", Name = ChangeCourseRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> ChangeCourse()
        {
            await _authoriseOrchestrator.PrepareChangeCourseAsync();

            return RedirectToRoute(SelectCourseRouteGet);
        }

        [HttpGet("select-course", Name = SelectCourseRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectCourse()
        {
            var model = await _authoriseOrchestrator.GetSelectCourseViewModelAsync();
            if (model == null || model.Courses == null || !model.Courses.Any())
            {
                return await RedirectToNotFoundAsync();
            }

            return View(model);
        }

        [HttpPost("select-course", Name = SelectCourseRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectCourse(SelectCourseViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateSelectCourseViewModel(model, ModelState))
            {
                return RedirectToRoute(SelectCourseRouteGet);
            }

            var updatedModel =
                await _authoriseOrchestrator.SaveSelectedCourseAsync(model);

            var returningOnLongJourney =
                updatedModel.IsReturningToCheck && !updatedModel.IsShortJourney;

            var matchOutcome =
                await _authoriseOrchestrator.GetCourseMatchOutcomeAsync();

            switch (matchOutcome)
            {
                case MatchOutcome.NoMatch when returningOnLongJourney:
                case MatchOutcome.MultipleMatches when returningOnLongJourney:
                case MatchOutcome.SingleMatch:
                    return RedirectToRoute(CheckAnswersRouteGet);

                case MatchOutcome.NoMatch:
                case MatchOutcome.MultipleMatches:
                    return RedirectToRoute(KnowYearRouteGet);

                case MatchOutcome.NoData:
                default:
                    return await RedirectToNotFoundAsync();
            }
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

        [HttpGet("select-provider", Name = SelectProviderRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectProvider()
        {
            var model = await _authoriseOrchestrator.GetSelectProviderViewModelAsync();
            if (model == null || model.Providers == null || !model.Providers.Any())
            {
                return await RedirectToNotFoundAsync();
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
                case MatchOutcome.MultipleMatches:
                    var isSingle = await _authoriseOrchestrator.HasSingleOwnedCertificateAsync();
                    if (isSingle)
                    {
                        TempData.AddFlashMessage("We've matched your information to this course.", string.Empty, TempDataDictionaryExtensions.FlashMessageLevel.Success);
                    }
                    else
                    {
                        TempData.AddFlashMessage("We've matched your information to these courses.", string.Empty, TempDataDictionaryExtensions.FlashMessageLevel.Success);
                    }

                    return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
                
                case MatchOutcome.Locked:
                    return await RedirectToCannotMatchAsync();
                
                case MatchOutcome.MandatoryUlnMissing:
                    return await RedirectToNotFoundAsync();

                default:
                    var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
                    return HandleNoMatch(answers.Uln);
            }
        }

        private async Task<IActionResult> RedirectToCannotMatchAsync()
        {
            await _authoriseOrchestrator.CreateUserActionForCannotMatchAsync(ActionType.NotMatched);
            await _sessionService.ClearAuthorisationAnswersAsync();

            return RedirectToRoute(CannotMatchRouteGet);
        }

        private async Task<IActionResult> RedirectToNotFoundAsync()
        {
            await _authoriseOrchestrator.CreateUserActionForCannotMatchAsync(ActionType.NotFound);
            await _sessionService.ClearAuthorisationAnswersAsync();

            return RedirectToRoute(NotFoundRouteGet);
        }

        private IActionResult HandleNoMatch(long? uln)
        {
            const string title = "We cannot match your information to any results.";

            var detail = uln.HasValue
                ? "Check your answers, you might need to search for the correct information in your documents or emails. If you need to make changes we can try to match your results, or you can submit again."
                : "Ask your training provider to give you your unique learner number so we can match you to your results. Or you may find your ULN on your exam certificates or results slips.";

            TempData.AddFlashMessageWithDetail(
                title,
                detail,
                TempDataDictionaryExtensions.FlashMessageLevel.Warning);

            return RedirectToRoute(CheckAnswersRouteGet);
        }

        [HttpGet("cannot-match", Name = CannotMatchRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> CannotMatch()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), "Home");
            }

            if (User?.Identity?.IsAuthenticated == true)
            {
                var ulnAuthorisation = await _sessionService.GetUlnAuthorisationAsync();
                if (ulnAuthorisation != null)
                {
                    return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
                }
            }

            var reference = await _authoriseOrchestrator.GetLatestUserActionReferenceAsync(ActionType.NotMatched);
            if (string.IsNullOrWhiteSpace(reference))
            {
                reference = await _authoriseOrchestrator.CreateUserActionForCannotMatchAsync(ActionType.NotMatched);
            }
            var model = new CannotMatchViewModel { ReferenceNumber = reference };
            return View("ShutterPage", model);
        }

        [HttpGet("not-found", Name = NotFoundRouteGet)]
        [AllowAnonymous]
        public async Task<IActionResult> NotFoundPage()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(HomeController.AccessDenied), "Home");
            }

            if (User?.Identity?.IsAuthenticated == true)
            {
                var ulnAuthorisation = await _sessionService.GetUlnAuthorisationAsync();
                if (ulnAuthorisation != null)
                {
                    return RedirectToRoute(CertificatesController.CertificatesListRouteGet);
                }
            }

            var reference = await _authoriseOrchestrator.GetLatestUserActionReferenceAsync(ActionType.NotFound);
            var model = new CannotMatchViewModel { ReferenceNumber = reference };
            return View("ShutterPage", model);
        }
    }
}
