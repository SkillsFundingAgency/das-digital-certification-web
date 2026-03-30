using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;
using MediatR;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly IValidator<SelectAddressViewModel> _selectAddressValidator;
        private readonly IValidator<AddAddressManualViewModel> _addAddressValidator;

        public CertificatesOrchestrator(IMediator mediator, ISessionService sessionService, IUserService userService,
            IValidator<SelectAddressViewModel> selectAddressValidator,
            IValidator<AddAddressManualViewModel> addAddressValidator)
            : base(mediator)
        {
            _sessionService = sessionService;
            _userService = userService;
            _selectAddressValidator = selectAddressValidator;
            _addAddressValidator = addAddressValidator;
        }

        public async Task<CertificatesListViewModel> GetCertificatesListViewModel()
        {
            return new CertificatesListViewModel
            {
                Certificates = await _sessionService.GetOwnedCertificatesAsync()
            };
        }

        public async Task<CertificateStandardViewModel?> GetCertificateStandardViewModel(Guid certificateId)
        {
            var result = await Mediator.Send(new GetStandardCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var viewModel = new CertificateStandardViewModel
            {
                CertificateId = certificateId,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                Uln = result.Uln,
                CertificateType = Enum.TryParse<CertificateType>(result.CertificateType, out var parsed) ? parsed : CertificateType.Unknown,
                CertificateReference = result.CertificateReference,
                CourseCode = result.CourseCode,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                OverallGrade = result.OverallGrade,
                ProviderName = result.ProviderName,
                Ukprn = result.Ukprn,
                EmployerName = result.EmployerName,
                AssessorName = result.AssessorName,
                StartDate = result.StartDate,
                PrintRequestedAt = result.PrintRequestedAt,
                PrintRequestedBy = result.PrintRequestedBy
            };

            var owned = await _sessionService.GetOwnedCertificatesAsync();

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        public async Task<CertificateFrameworkViewModel?> GetCertificateFrameworkViewModel(Guid certificateId)
        {
            var result = await Mediator.Send(new GetFrameworkCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var viewModel = new CertificateFrameworkViewModel
            {
                CertificateId = certificateId,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                Uln = result.Uln,
                CertificateType = Enum.TryParse<CertificateType>(result.CertificateType, out var parsed) ? parsed : CertificateType.Unknown,
                CertificateReference = result.CertificateReference,
                FrameworkCertificateNumber = result.FrameworkCertificateNumber,
                CourseCode = result.CourseCode,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                OverallGrade = result.OverallGrade,
                ProviderName = result.ProviderName,
                Ukprn = result.Ukprn,
                EmployerName = result.EmployerName,
                AssessorName = result.AssessorName,
                StartDate = result.StartDate,
                PrintRequestedAt = result.PrintRequestedAt,
                PrintRequestedBy = result.PrintRequestedBy,
                QualificationsAndAwardingBodies = result.QualificationsAndAwardingBodies,
                DeliveryInformation = result.DeliveryInformation
            };

            var owned = await _sessionService.GetOwnedCertificatesAsync();

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        public async Task<CreateUserActionForCertificateResult> CreateUserActionForCertificate(Guid certificateId)
        {
            var userId = _userService.GetUserId();
            if (userId == null)
                return new CreateUserActionForCertificateResult();

            var owned = await _sessionService.GetOwnedCertificatesAsync();
            var ownedCertificate = owned?.FirstOrDefault(c => c.CertificateId == certificateId);

            if (ownedCertificate == null)
                return new CreateUserActionForCertificateResult();

            var userDetails = await _sessionService.GetUserDetailsAsync();

            var familyName = userDetails?.FamilyName ?? string.Empty;
            var givenNames = userDetails?.GivenNames ?? string.Empty;

            var certificateType = ownedCertificate.CertificateType;
            var courseName = ownedCertificate.CourseName ?? string.Empty;

            var result = await Mediator.Send(new CreateUserActionCommand
            {
                UserId = userId.Value,
                ActionType = ActionType.Help,
                FamilyName = familyName,
                GivenNames = givenNames,
                CertificateId = certificateId,
                CertificateType = certificateType,
                CourseName = courseName
            });

            if (result != null && !string.IsNullOrEmpty(result.ActionCode))
            {
                await _sessionService.SetContactReferenceAsync(result.ActionCode);
            }

            return new CreateUserActionForCertificateResult
            {
                ReferenceNumber = result?.ActionCode ?? string.Empty,
                CertificateType = certificateType
            };
        }

        public async Task<string?> CreateUserActionForNonSpecific()
        {
            var userId = _userService.GetUserId();
            if (userId == null)
                return null;

            var userDetails = await _sessionService.GetUserDetailsAsync();

            var familyName = userDetails?.FamilyName ?? string.Empty;
            var givenNames = userDetails?.GivenNames ?? string.Empty;

            var result = await Mediator.Send(new CreateUserActionCommand
            {
                UserId = userId.Value,
                ActionType = ActionType.Contact,
                FamilyName = familyName,
                GivenNames = givenNames
            });

            if (result != null && !string.IsNullOrEmpty(result.ActionCode))
            {
                await _sessionService.SetContactReferenceAsync(result.ActionCode);
            }

            return result?.ActionCode ?? string.Empty;
        }

        public async Task<ContactUsViewModel?> GetContactUsViewModel(string referenceNumber, Guid? certificateId)
        {
            if (string.IsNullOrEmpty(referenceNumber))
                return null;

            CertificateType certificateType = CertificateType.Unknown;

            if (certificateId != null)
            {
                var owned = await _sessionService.GetOwnedCertificatesAsync();
                var ownedCertificate = owned?.FirstOrDefault(c => c.CertificateId == certificateId);
                certificateType = ownedCertificate?.CertificateType ?? CertificateType.Unknown;
            }

            var model = new ContactUsViewModel
            {
                ReferenceNumber = referenceNumber,
                CertificateId = certificateId,
                CertificateType = certificateType
            };

            return model;
        }

        public async Task<SelectAddressViewModel?> GetSelectAddressViewModel(Guid certificateId, string? searchTerm = null)
        {
            var owned = await _sessionService.GetOwnedCertificatesAsync();
            var ownedCertificate = owned?.FirstOrDefault(c => c.CertificateId == certificateId);

            if (ownedCertificate == null)
            {
                return null;
            }

            var userDetails = await _sessionService.GetUserDetailsAsync();

            var viewModel = new SelectAddressViewModel
            {
                CertificateId = certificateId,
                CourseName = ownedCertificate.CourseName,
                GivenNames = userDetails?.GivenNames,
                FamilyName = userDetails?.FamilyName,
                SearchTerm = searchTerm
            };

            return viewModel;
        }

        public async Task<AddAddressManualViewModel?> GetAddAddressViewModel(Guid certificateId)
        {
            var owned = await _sessionService.GetOwnedCertificatesAsync();
            var ownedCertificate = owned?.FirstOrDefault(c => c.CertificateId == certificateId);

            if (ownedCertificate == null)
            {
                return null;
            }

            var userDetails = await _sessionService.GetUserDetailsAsync();

            var viewModel = new AddAddressManualViewModel
            {
                CertificateId = certificateId,
                CourseName = ownedCertificate.CourseName,
                GivenNames = userDetails?.GivenNames,
                FamilyName = userDetails?.FamilyName
            };

            var address = await _sessionService.GetDeliveryAddressAsync();
            if (address != null)
            {
                viewModel.Organisation = address.Organisation;
                viewModel.AddressLine1 = address.AddressLine1;
                viewModel.AddressLine2 = address.AddressLine2;
                viewModel.TownOrCity = address.TownOrCity;
                viewModel.County = address.County;
                viewModel.Postcode = address.Postcode;
            }

            return viewModel;
        }

        public async Task<CheckAndSubmitViewModel?> GetCheckAndSubmitViewModel(Guid certificateId, string defaultBackRoute)
        {
            var owned = await _sessionService.GetOwnedCertificatesAsync();
            var ownedCertificate = owned?.FirstOrDefault(c => c.CertificateId == certificateId);

            if (ownedCertificate == null)
            {
                return null;
            }

            var userDetails = await _sessionService.GetUserDetailsAsync();

            var vm = new CheckAndSubmitViewModel
            {
                CertificateId = certificateId,
                CourseName = ownedCertificate.CourseName,
                GivenNames = userDetails?.GivenNames,
                FamilyName = userDetails?.FamilyName
            };

            var address = await _sessionService.GetDeliveryAddressAsync();
            if (address != null)
            {
                vm.BackRoute = address.BackRoute;
                vm.Organisation = address.Organisation;
                vm.AddressLine1 = address.AddressLine1;
                vm.AddressLine2 = address.AddressLine2;
                vm.TownOrCity = address.TownOrCity;
                vm.County = address.County;
                vm.Postcode = address.Postcode;
            }

            vm.BackRoute = string.IsNullOrWhiteSpace(vm.BackRoute) ? defaultBackRoute : vm.BackRoute;

            return vm;
        }

        public async Task<bool> ValidateSelectAddressViewModel(SelectAddressViewModel viewModel, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_selectAddressValidator, viewModel, modelState);
        }

        public async Task<bool> ValidateAddAddressManualViewModel(AddAddressManualViewModel viewModel, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_addAddressValidator, viewModel, modelState);
        }

        public async Task<bool> StoreDeliveryAddressFromLocationAsync(Guid certificateId, string selectedName, string backRoute)
        {
            if (string.IsNullOrWhiteSpace(selectedName)) return false;

            var locationsResult = await Mediator.Send(new GetLocationsQuery { SearchTerm = selectedName });

            var matchLocation = locationsResult?.Locations?.FirstOrDefault(location => string.Equals(location.Name?.Trim(), selectedName?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (matchLocation == null) return false;
            var addr = new CheckAndSubmitViewModel
            {
                CertificateId = certificateId,
                Organisation = matchLocation.Organisation,
                AddressLine1 = matchLocation.AddressLine1,
                AddressLine2 = matchLocation.AddressLine2,
                TownOrCity = matchLocation.PostTown,
                County = matchLocation.County,
                Postcode = matchLocation.Postcode,
                BackRoute = backRoute
            };

            await _sessionService.SetDeliveryAddressAsync(addr);

            return true;
        }
    }
}
