using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Application.Commands.AuthoriseUser;
using SFA.DAS.DigitalCertificates.Application.Commands.SubmitMatch;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Constants;
using SFA.DAS.DigitalCertificates.Web.Enums;
using SFA.DAS.DigitalCertificates.Web.Exceptions;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class AuthoriseOrchestrator : BaseOrchestrator, IAuthoriseOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IGovUkAuthenticationService _govUkAuthenticationService;
        private readonly IValidator<KnowYourUlnViewModel> _knowUlnValidator;
        private readonly IValidator<KnowYearViewModel> _knowYearValidator;
        private readonly IValidator<SelectCourseViewModel> _selectCourseValidator;
        private readonly IValidator<SelectProviderViewModel> _selectProviderValidator;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;

        public AuthoriseOrchestrator(IMediator mediator, IHttpContextAccessor httpContextAccessor, ISessionService sessionService, IUserService userService, 
            ICacheService cacheService, IGovUkAuthenticationService govUkAuthenticationService,
                IValidator<KnowYourUlnViewModel> knowUlnValidator,
                IValidator<KnowYearViewModel> knowYearValidator,
                IValidator<SelectCourseViewModel> selectCourseValidator,
                IValidator<SelectProviderViewModel> selectProviderValidator,
                DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration)
            : base(mediator, httpContextAccessor)
        {
            _sessionService = sessionService;
            _userService = userService;
            _cacheService = cacheService;
            _govUkAuthenticationService = govUkAuthenticationService;
            _knowUlnValidator = knowUlnValidator;
            _knowYearValidator = knowYearValidator;
            _selectCourseValidator = selectCourseValidator;
            _selectProviderValidator = selectProviderValidator;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
        }

        public async Task<bool> PrepareNeedMoreInformationAsync()
        {
            var matches = await GetMatchesAsync();

            return matches?.Matches?.Any() == true &&
                   matches?.Masks?.Any() == true;
        }

        public async Task<bool> ValidateKnowYourUlnViewModel(KnowYourUlnViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_knowUlnValidator, viewModel, modelState);
        }

        public async Task<KnowYourUlnViewModel?> GetKnowYourUlnViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            if (answers == null) return new KnowYourUlnViewModel { IsReturningToCheck = false };

            return new KnowYourUlnViewModel
            {
                KnowUln = answers.KnowUln,
                Uln = answers.Uln,
                IsReturningToCheck = answers.IsReturningToCheck,
                IsShortJourney = answers.IsShortJourney
            };
        }

        public async Task<KnowYourUlnViewModel> SaveKnowYourUlnAsync(KnowYourUlnViewModel viewModel)
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.KnowUln = viewModel.KnowUln;
            answers.Uln = viewModel.KnowUln == true ? viewModel.Uln : null;

            await _sessionService.SetAuthorisationAnswersAsync(answers);

            viewModel.IsReturningToCheck = answers.IsReturningToCheck;
            viewModel.IsShortJourney = answers.IsShortJourney;

            return viewModel;
        }

        public async Task<bool> ValidateKnowYearViewModel(KnowYearViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_knowYearValidator, viewModel, modelState);
        }

        public async Task<KnowYearViewModel?> GetKnowYearViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            if (answers == null) return null;

            return new KnowYearViewModel
            {
                KnowYear = answers.KnowYear,
                YearCompleted = answers.YearCompleted,
                IsReturningToCheck = answers.IsReturningToCheck,
                IsShortJourney = answers.IsShortJourney,
                ProviderSelected = answers.ProviderUkprn != null
            };
        }

        public async Task<KnowYearViewModel> SaveKnowYearAsync(KnowYearViewModel viewModel)
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.KnowYear = viewModel.KnowYear;
            answers.YearCompleted = viewModel.KnowYear == true ? viewModel.YearCompleted : null;

            await _sessionService.SetAuthorisationAnswersAsync(answers);

            viewModel.IsReturningToCheck = answers.IsReturningToCheck;
            viewModel.IsShortJourney = answers.IsShortJourney;
            viewModel.ProviderSelected = answers.ProviderUkprn != null;

            return viewModel;
        }

        private async Task<MatchesAndMasks?> GetMatchesAsync()
        {
            var govUkId = _userService.GetGovUkIdentifier();
            if (string.IsNullOrWhiteSpace(govUkId)) return null;

            var userId = _userService.GetUserId();
            if (userId == null) return null;

            var matches = await _cacheService.GetMatchesAsync(govUkId);
            if(matches == null)
            {
                var govUkCredentialSubject = await GetVerifyDetails();
                if (govUkCredentialSubject != null)
                {
                    matches = await _cacheService.CreateMatchesAsync(govUkId, userId.Value, govUkCredentialSubject);
                }
            }

            return matches;
        }

        public async Task<bool> ValidateSelectCourseViewModel(SelectCourseViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_selectCourseValidator, viewModel, modelState);
        }

        public async Task<SelectCourseViewModel?> GetSelectCourseViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            var matches = await GetMatchesAsync();
            var courseOptions = MapMatchesToCourseOptions(matches);

            var model = new SelectCourseViewModel
            {
                SelectedCourseCode = answers?.CourseCode,
                SelectedCourseUnknown = answers?.CourseUnknown,
                Courses = courseOptions,
                IsReturningToCheck = answers?.IsReturningToCheck == true,
                IsShortJourney = answers?.IsShortJourney == true
            };

            return model;
        }

        public async Task<SelectCourseViewModel> SaveSelectedCourseAsync(SelectCourseViewModel viewModel)
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            if (viewModel.SelectedCourseUnknown == true)
            {
                answers.CourseUnknown = true;
                answers.CourseCode = null;
                answers.CourseName = null;
            }
            else
            {
                answers.CourseUnknown = false;
                answers.CourseCode = viewModel.SelectedCourseCode?.Trim();
            }

            var matches = await GetMatchesAsync();
            var courseOptions = MapMatchesToCourseOptions(matches);

            var selected = courseOptions.FirstOrDefault(c => c.CourseCode == answers.CourseCode);
            answers.CourseName = selected?.CourseName;
            answers.CourseLevel = selected?.CourseLevel;
            answers.CourseCertificateType = selected?.CertificateType;

            await _sessionService.SetAuthorisationAnswersAsync(answers);

            viewModel.IsReturningToCheck = answers.IsReturningToCheck;
            viewModel.IsShortJourney = answers.IsShortJourney;

            return viewModel;
        }

        private List<SelectCourseViewModel.CourseOption> MapMatchesToCourseOptions(MatchesAndMasks? matches)
        {
            var courseOptions = new List<SelectCourseViewModel.CourseOption>();
            if (matches == null) return courseOptions;

            var maskOptions = new List<SelectCourseViewModel.CourseOption>();
            if (matches.Masks != null)
            {
                maskOptions.AddRange(matches.Masks
                    .Where(m => !string.IsNullOrWhiteSpace(m.CourseCode) && !string.IsNullOrWhiteSpace(m.CourseName))
                    .Select(m => new SelectCourseViewModel.CourseOption
                    {
                        CourseCode = m.CourseCode!.Trim(),
                        CourseName = m.CourseName!.Trim(),
                        CourseLevel = m.CourseLevel,
                        CertificateType = m.CertificateType
                    }));
            }

            var matchOptions = new List<SelectCourseViewModel.CourseOption>();
            if (matches.Matches != null)
            {
                matchOptions.AddRange(matches.Matches
                    .Where(m => !string.IsNullOrWhiteSpace(m.CourseCode) && !string.IsNullOrWhiteSpace(m.CourseName))
                    .Select(m => new SelectCourseViewModel.CourseOption
                    {
                        CourseCode = m.CourseCode!.Trim(),
                        CourseName = m.CourseName!.Trim(),
                        CourseLevel = m.CourseLevel,
                        CertificateType = m.CertificateType
                    }));
            }

            courseOptions.AddRange(maskOptions);
            courseOptions.AddRange(matchOptions);

            var masksCount = maskOptions.Count;
            var minMasks = _digitalCertificatesWebConfiguration.MinimumMasksForSelection ?? 5;

            var duplicateCourseExists = courseOptions
                .GroupBy(c => (c.CourseCode ?? string.Empty).Trim() + "|" + (c.CourseName ?? string.Empty).Trim())
                .Any(g => g.Count() > 1);

            var distinctCourses = courseOptions
                .OrderBy(c => c.CourseName)
                .ThenBy(c => c.CourseCode)
                .ToList();

            return distinctCourses;
        }

        public async Task<bool> ValidateSelectProviderViewModel(SelectProviderViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_selectProviderValidator, viewModel, modelState);
        }

        public async Task<SelectProviderViewModel?> GetSelectProviderViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            var matches = await GetMatchesAsync();
            var providerOptions = MapMatchesToProviderOptions(matches);

            var model = new SelectProviderViewModel
            {
                SelectedProviderName = answers?.ProviderName,
                SelectedProviderUnknown = answers?.ProviderUnknown,
                Providers = providerOptions,
                IsReturningToCheck = answers?.IsReturningToCheck == true,
                IsShortJourney = answers?.IsShortJourney == true
            };

            return model;
        }

        public async Task<SelectProviderViewModel> SaveSelectedProviderAsync(SelectProviderViewModel viewModel)
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            viewModel.IsReturningToCheck = answers.IsReturningToCheck;
            viewModel.IsShortJourney = answers.IsShortJourney;
            if (viewModel.SelectedProviderUnknown == true)
            {
                answers.ProviderUnknown = true;
                answers.ProviderUkprn = null;
                answers.ProviderName = null;
                await _sessionService.SetAuthorisationAnswersAsync(answers);
                return viewModel;
            }

            var selectedName = viewModel.SelectedProviderName?.Trim();

            var matches = await GetMatchesAsync();
            var providerOptions = MapMatchesToProviderOptions(matches);

            var selected = providerOptions.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.ProviderName) && string.Equals(p.ProviderName?.Trim(), selectedName, System.StringComparison.OrdinalIgnoreCase)
            );

            if (selected != null)
            {
                answers.ProviderUkprn = selected.Ukprn;
                answers.ProviderName = selected.ProviderName?.Trim();
                answers.ProviderUnknown = false;
            }
            else
            {
                answers.ProviderUkprn = null;
                answers.ProviderName = null;
                answers.ProviderUnknown = false;
            }

            await _sessionService.SetAuthorisationAnswersAsync(answers);

            return viewModel;
        }

        private List<SelectProviderViewModel.ProviderOption> MapMatchesToProviderOptions(MatchesAndMasks? matches)
        {
            var providerOptions = new List<SelectProviderViewModel.ProviderOption>();
            if (matches == null) return providerOptions;

            var matchOptions = new List<SelectProviderViewModel.ProviderOption>();
            if (matches.Matches != null)
            {
                matchOptions.AddRange(matches.Matches
                    .Where(m => !string.IsNullOrWhiteSpace(m.ProviderName))
                    .Select(m => new SelectProviderViewModel.ProviderOption
                    {
                        Ukprn = m.Ukprn,
                        ProviderName = m.ProviderName!.Trim()
                    }));
            }

            var maskOptions = new List<SelectProviderViewModel.ProviderOption>();
            if (matches.Masks != null)
            {
                maskOptions.AddRange(matches.Masks
                    .Where(m => !string.IsNullOrWhiteSpace(m.ProviderName))
                    .Select(m => new SelectProviderViewModel.ProviderOption
                    {
                        Ukprn = null,
                        ProviderName = m.ProviderName!.Trim()
                    }));
            }

            providerOptions.AddRange(matchOptions);
            providerOptions.AddRange(maskOptions);

            var masksCount = maskOptions.Count;
            var minMasks = _digitalCertificatesWebConfiguration.MinimumMasksForSelection ?? 5;

            var duplicateProviderNameExists = providerOptions
                .GroupBy(p => (p.ProviderName ?? string.Empty).Trim())
                .Any(g => g.Count() > 1);

            var distinctProviders = providerOptions
                .OrderBy(p => p.ProviderName)
                .ToList();

            return distinctProviders;
        }

        public async Task<MatchOutcome> GetCourseMatchOutcomeAsync(SelectCourseViewModel viewModel)
        {
            if (viewModel == null) return MatchOutcome.NoData;

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            var matches = await GetMatchesAsync();
            if (matches == null) return MatchOutcome.NoData;

            var matchResult = FindMatch(answers, matches.Matches ?? Enumerable.Empty<Match>());

            await _sessionService.SetAuthorisationAnswersAsync(answers);

            return matchResult.Outcome;
        }

        public async Task<MatchOutcome> GetUlnMatchOutcomeAsync(KnowYourUlnViewModel viewModel)
        {
            if (viewModel == null) return MatchOutcome.NoData;

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            var matches = await GetMatchesAsync();
            if (matches == null) return MatchOutcome.NoData;

            var matchResult = FindMatch(answers, matches.Matches ?? Enumerable.Empty<Match>());

            await _sessionService.SetAuthorisationAnswersAsync(answers);

            return matchResult.Outcome;
        }

        public async Task<CheckAnswersViewModel?> GetCheckAnswersViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            if (answers == null) return null;

            var courseAnswered = answers.CourseUnknown != null || !string.IsNullOrWhiteSpace(answers.CourseCode);
            if (answers.KnowUln == null && !courseAnswered)
            {
                return null;
            }

            answers.IsReturningToCheck = true;
            await _sessionService.SetAuthorisationAnswersAsync(answers);

            var ulnDisplay = answers.KnowUln == false
                ? DisplayConstants.NotKnown
                : answers.Uln?.ToString();

            var yearDisplay = answers.KnowYear == false
                ? DisplayConstants.NotKnown
                : answers.YearCompleted?.ToString() ?? string.Empty;

            var providerDisplay = answers.ProviderUnknown == true
                ? DisplayConstants.NotKnown
                : answers.ProviderName;

            var courseDisplay = answers.CourseUnknown == true
                ? DisplayConstants.NotKnown
                : answers.CourseName;

            if (!string.IsNullOrWhiteSpace(answers.CourseLevel) && !string.IsNullOrWhiteSpace(answers.CourseName))
            {
                var levelText = string.Empty;
                if (answers.CourseCertificateType == CertificateType.Framework)
                {
                    levelText = $" ({answers.CourseLevel} Level)";
                }
                else
                {
                    levelText = $" (Level {answers.CourseLevel})";
                }

                courseDisplay = string.Concat(answers.CourseName ?? string.Empty, levelText);
            }

            return new CheckAnswersViewModel
            {
                CourseDisplay = courseDisplay,
                IsShortJourney = answers.IsShortJourney,
                UlnDisplay = ulnDisplay,
                YearDisplay = yearDisplay,
                ProviderDisplay = providerDisplay
            };
        }

        public async Task<MatchOutcome> SubmitCheckAnswersAsync()
        {
            var userId = _userService.GetUserId();
            if (userId == null) throw new InvalidOperationException("UserId is required for submiting authorisation");

            var answers = await _sessionService.GetAuthorisationAnswersAsync()
                          ?? new AuthorisationAnswers();

            if (await IsFailedMatchLimitReachedAsync())
            {
                return MatchOutcome.Locked;
            }

            var matches = await GetMatchesAsync();
            if (matches?.Matches == null || matches.Matches.Count == 0)
            {
                return MatchOutcome.NoData;
            }

            var matchResult = FindMatch(
                answers,
                matches.Matches);

            if (matchResult.Outcome == MatchOutcome.SingleMatch || matchResult.Outcome == MatchOutcome.MultipleMatches)
            {
                var matchResultMatch = matchResult.Match!;
                await Mediator.Send(new SubmitMatchCommand
                {
                    UserId = userId.Value,
                    Uln = matchResultMatch.Uln,
                    UserIdentityId = matchResultMatch.UserIdentityId,
                    CertificateType = matchResultMatch.CertificateType.ToString(),
                    CourseCode = matchResultMatch.CourseCode,
                    CourseName = matchResultMatch.CourseName,
                    CourseLevel = matchResultMatch.CourseLevel,
                    YearAwarded = matchResultMatch.DateAwarded?.Year,
                    ProviderName = matchResultMatch.ProviderName,
                    Ukprn = matchResultMatch.Ukprn.HasValue ? (int?)matchResultMatch.Ukprn.Value : null,
                    IsMatched = true,
                    IsFailed = false
                });

                await Mediator.Send(new AuthoriseUserCommand
                {
                    UserId = userId.Value,
                    Uln = matchResultMatch.Uln
                });

                return matchResult.Outcome;
            }

            var govUkId = _userService.GetGovUkIdentifier();
            var failedLimit = _digitalCertificatesWebConfiguration.FailedMatchesLimit ?? 2;
            var updatedFailedCount = await _cacheService.IncrementMatchFailCountAsync(govUkId);

            await Mediator.Send(new SubmitMatchCommand
            {
                UserId = userId.GetValueOrDefault(),
                Uln = answers.Uln,
                UserIdentityId = null,
                CertificateType = null,
                CourseCode = answers.CourseCode,
                CourseName = answers.CourseName,
                CourseLevel = null,
                YearAwarded = answers.YearCompleted,
                ProviderName = answers.ProviderName,
                Ukprn = answers.ProviderUkprn.HasValue ? (int?)answers.ProviderUkprn.Value : null,
                IsMatched = false,
                IsFailed = updatedFailedCount >= failedLimit
            });

            if (updatedFailedCount >= failedLimit)
            {
                await _cacheService.ClearUser(govUkId);
                await _cacheService.ClearMatchFailCountAsync(govUkId);
                return MatchOutcome.Locked;
            }

            return MatchOutcome.NoMatch;
        }

        private async Task<bool> IsFailedMatchLimitReachedAsync()
        {
            var govUkId = _userService.GetGovUkIdentifier();
            if (string.IsNullOrWhiteSpace(govUkId)) return false;

            var failedLimit = _digitalCertificatesWebConfiguration.FailedMatchesLimit ?? 2 ;
            var failedCount = await _cacheService.GetMatchFailCountAsync(govUkId);

            return failedCount >= failedLimit;
        }

        private static MatchResult FindMatch(AuthorisationAnswers answers, IEnumerable<Match> matches)
        {
            var selectedCourse = answers.CourseCode?.Trim();

            if (!string.IsNullOrWhiteSpace(selectedCourse) && answers.Uln != null && answers.IsShortJourney)
            {
                var exactMatches = matches
                    .Where(m =>
                        string.Equals(m.CourseCode?.Trim(), selectedCourse, StringComparison.OrdinalIgnoreCase) &&
                        m.Uln == answers.Uln.Value)
                    .ToList();

                if (exactMatches.Count > 0)
                {
                    if (exactMatches.Count == 1)
                    {
                        return MatchResult.Single(exactMatches[0]);
                    }

                    return MatchResult.Multiple(exactMatches[0]);
                }
            }

            var candidates = matches
                .Where(m =>
                    (answers.Uln != null && m.Uln == answers.Uln.Value) ||
                    (!string.IsNullOrWhiteSpace(answers.CourseCode) && string.Equals(m.CourseCode?.Trim(), answers.CourseCode.Trim(), StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!candidates.Any())
            {
                return MatchResult.None();
            }

            var hasYear = answers.YearCompleted != null;
            var hasProvider = answers.ProviderUkprn != null || !string.IsNullOrWhiteSpace(answers.ProviderName);

            if (!hasYear || !hasProvider)
            {
                return MatchResult.None();
            }

            candidates = candidates
                .Where(c =>
                    c.DateAwarded != null &&
                    Math.Abs(c.DateAwarded.Value.Year - answers.YearCompleted!.Value) <= 1)
                .ToList();

            if (answers.ProviderUkprn != null)
            {
                candidates = candidates
                    .Where(c => c.Ukprn == answers.ProviderUkprn.Value)
                    .ToList();
            }
            else if (!string.IsNullOrWhiteSpace(answers.ProviderName))
            {
                candidates = candidates
                    .Where(c => !string.IsNullOrWhiteSpace(c.ProviderName) && string.Equals(c.ProviderName.Trim(), answers.ProviderName.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (candidates.Count == 1)
            {
                return MatchResult.Single(candidates[0]);
            }

            if (candidates.Count > 1)
            {
                var firstUln = candidates[0].Uln;
                var allSameUln = candidates.All(c => c.Uln == firstUln);
                return allSameUln
                    ? MatchResult.Multiple(candidates[0])
                    : MatchResult.NoData();
            }

            return MatchResult.None();
        }

        public async Task<string?> CreateUserActionForCannotMatchAsync(ActionType actionType)
        {
            var userId = _userService.GetUserId();
            if (userId == null) throw new InvalidOperationException("UserId is required for creating user action");

            var family = GetUserSurname();
            var given = GetUserGivenNames();

            var result = await Mediator.Send(new Application.Commands.CreateUserAction.CreateUserActionCommand
            {
                UserId = userId.GetValueOrDefault(),
                ActionType = actionType,
                FamilyName = family,
                GivenNames = given
            });
            return result?.ActionCode ?? string.Empty;
        }

        public async Task<string?> GetLatestUserActionReferenceAsync(ActionType actionType)
        {
            var userId = _userService.GetUserId();
            if (userId == null) throw new InvalidOperationException("UserId is required for getting latest user action reference");

            var existing = await Mediator.Send(new GetUserActionsQuery
            {
                UserId = userId.GetValueOrDefault()
            });

            var candidates = existing?.UserActions?
                .Where(a => a.ActionType == actionType)
                .ToList();

            if (candidates == null || !candidates.Any()) return null;

            var mostRecent = candidates
                .OrderByDescending(a => a.ActionTime)
                .FirstOrDefault();

            return mostRecent?.ActionCode;
        }

        private async Task<GovUkCredentialSubject> GetVerifyDetails()
        {
            if (HttpContext != null)
            {
                var token = await HttpContext.GetTokenAsync("access_token");
                var details = await _govUkAuthenticationService.GetAccountDetails(token);

                if (details == null)
                {
                    throw new VerifyException("Unable to load verify details");
                }

                return details.CoreIdentityJwt.Vc.CredentialSubject;
            }

            return null;
        }

        private sealed class MatchResult
        {
            public MatchOutcome Outcome { get; }
            public Match? Match { get; }

            private MatchResult(MatchOutcome outcome, Match? match = null)
            {
                Outcome = outcome;
                Match = match;
            }

            public static MatchResult Single(Match match) => new MatchResult(MatchOutcome.SingleMatch, match);
            public static MatchResult Multiple(Match match) => new MatchResult(MatchOutcome.MultipleMatches, match);
            public static MatchResult None() => new MatchResult(MatchOutcome.NoMatch);
            public static MatchResult NoData() => new MatchResult(MatchOutcome.NoData);
        }
    }
}
