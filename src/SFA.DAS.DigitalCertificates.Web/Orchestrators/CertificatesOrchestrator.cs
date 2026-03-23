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
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Constants;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly DigitalCertificatesWebConfiguration _configuration;
        private readonly IValidator<SelectAddressViewModel> _selectAddressValidator;
        private readonly IValidator<AddAddressManualViewModel> _addAddressValidator;

        public CertificatesOrchestrator(IMediator mediator, ISessionService sessionService, IUserService userService,
            IValidator<SelectAddressViewModel> selectAddressValidator,
            IValidator<AddAddressManualViewModel> addAddressValidator,
            DigitalCertificatesWebConfiguration configuration)
            : base(mediator)
        {
            _sessionService = sessionService;
            _userService = userService;
            _selectAddressValidator = selectAddressValidator;
            _addAddressValidator = addAddressValidator;
            _configuration = configuration;
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

            var (printStatus, printDate, printMessage) = MapPrintStatus(null);
            viewModel.PrintStatus = printStatus;
            viewModel.PrintStatusDate = printDate;
            viewModel.PrintStatusMessage = printMessage;
            viewModel.ShowPrintHeader = printStatus != Enums.PrintStatus.None && printStatus != Enums.PrintStatus.Submitted;
            viewModel.ShowRequestPrint = printStatus == Enums.PrintStatus.Submitted && viewModel.PrintRequestedAt == null;

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

            var (printStatus, printDate, printMessage) = MapPrintStatus(result.DeliveryInformation);
            viewModel.PrintStatus = printStatus;
            viewModel.PrintStatusDate = printDate;
            viewModel.PrintStatusMessage = printMessage;
            viewModel.ShowPrintHeader = printStatus != Enums.PrintStatus.None && printStatus != Enums.PrintStatus.Submitted;
            viewModel.ShowRequestPrint = printStatus == Enums.PrintStatus.Submitted && viewModel.PrintRequestedAt == null;

            var owned = await _sessionService.GetOwnedCertificatesAsync();

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        public async Task<CreateUserActionForCertificateResult> CreateUserActionForCertificate(Guid certificateId, ActionType actionType)
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
                ActionType = actionType,
                FamilyName = familyName,
                GivenNames = givenNames,
                CertificateId = certificateId,
                CertificateType = certificateType,
                CourseName = courseName
            });

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

        public async Task CreatePrintRequest(Guid certificateId)
        {
            var userDetails = await _sessionService.GetUserDetailsAsync();
            string email = userDetails?.Email ?? string.Empty;
            string userName = userDetails?.FullName ?? string.Empty;

            var templateId = GetTemplateId(_configuration, NotificationTemplateNames.PrintRequest);

            var deliveryAddress = await _sessionService.GetDeliveryAddressAsync();

            var req = new CreatePrintRequest
            {
                Address = new PrintAddressDto
                {
                    ContactName = userName,
                    ContactOrganisation = deliveryAddress?.Organisation,
                    ContactAddLine1 = deliveryAddress?.AddressLine1,
                    ContactAddLine2 = deliveryAddress?.AddressLine2,
                    ContactAddLine3 = deliveryAddress?.TownOrCity,
                    ContactAddLine4 = deliveryAddress?.County,
                    ContactPostCode = deliveryAddress?.Postcode ?? string.Empty
                },
                Email = new PrintEmailDto
                {
                    EmailAddress = email,
                    UserName = userName,
                    LinkDomain = _configuration.ServiceBaseUrl,
                    TemplateId = templateId ?? string.Empty
                }
            };

            await Mediator.Send(new CreatePrintRequestCommand
            {
                CertificateId = certificateId,
                Request = req
            });
        }

        public async Task<PrintRequestConfirmationViewModel> GetPrintRequestConfirmationViewModel(Guid certificateId)
        {
            var ownedCert = await _sessionService.GetOwnedCertificatesAsync();
            var cert = ownedCert?.FirstOrDefault(c => c.CertificateId == certificateId);
            var courseName = cert?.CourseName ?? string.Empty;

            var vm = new PrintRequestConfirmationViewModel
            {
                CertificateId = certificateId,
                CourseName = courseName
            };

            return vm;
        }

        private (Enums.PrintStatus status, DateTime? date, string? message) MapPrintStatus(List<DeliveryInformationResponse>? deliveryInformation)
        {
            if (deliveryInformation == null || !deliveryInformation.Any())
            {
                return (Enums.PrintStatus.None, null, null);
            }

            var ordered = deliveryInformation
                .OrderByDescending(e => e.EventTime ?? DateTime.MinValue)
                .ToList();

            // Determine status from the latest event first
            var latest = ordered.First();
            var dt = latest.EventTime;

            if (latest.Status?.Equals(DeliveryInformationStatuses.Delivered, StringComparison.OrdinalIgnoreCase) == true)
            {
                var msg = dt != null
                    ? $"A certificate was delivered on {dt:dd MMMM yyyy}."
                    : "A certificate was delivered.";

                return (Enums.PrintStatus.Delivered, dt, msg);
            }

            if (latest.Status?.Equals(DeliveryInformationStatuses.Printed, StringComparison.OrdinalIgnoreCase) == true)
            {
                var msg = dt != null
                    ? $"You requested a certificate on {dt:dd MMMM yyyy}. It can take up to 3 weeks to be delivered."
                    : "You requested a certificate. It can take up to 3 weeks to be delivered.";

                return (Enums.PrintStatus.Printed, dt, msg);
            }

            if (latest.Status?.Equals(DeliveryInformationStatuses.SentToPrinter, StringComparison.OrdinalIgnoreCase) == true
                || latest.Status?.Equals(DeliveryInformationStatuses.Reprint, StringComparison.OrdinalIgnoreCase) == true)
            {
                var msg = dt != null
                    ? $"You requested a certificate on {dt:dd MMMM yyyy}. It can take up to 3 weeks to be delivered."
                    : "You requested a certificate. It can take up to 3 weeks to be delivered.";

                return (Enums.PrintStatus.Requested, dt, msg);
            }

            if (latest.Status?.Equals(DeliveryInformationStatuses.Submitted, StringComparison.OrdinalIgnoreCase) == true)
            {
                return (Enums.PrintStatus.Submitted, null, null);
            }

            return (Enums.PrintStatus.None, null, null);
        }
    }
}

