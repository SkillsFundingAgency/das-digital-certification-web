using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Forms;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Constants;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;
        private readonly IBlobService _blob;
        private readonly IAsposeLicenseService _asposeLicenseService;   
        private readonly IDownloadCertificateService _downloadCertificateService;
        private const string Level = "Level";
        private const string FullName = "Full Name";
        private const string PassedInfo = "Passed info";
        private const string AchievedGrade = "Achieved grade";
        private const string AwardedOn = "Awarded on";
        private const string DateFormat = "d MMMM yyyy";
        private const string CertificateNumber = "Certificate no";
        private readonly IValidator<SelectAddressViewModel> _selectAddressValidator;
        private readonly IValidator<AddAddressManualViewModel> _addAddressValidator;

        public CertificatesOrchestrator(IMediator mediator, 
            IHttpContextAccessor httpContextAccessor, 
            ISessionService sessionService, 
            IUserService userService,
            IValidator<SelectAddressViewModel> selectAddressValidator,
            IValidator<AddAddressManualViewModel> addAddressValidator,
            IBlobService blob,
            IAsposeLicenseService apposeLicenseService,
            DigitalCertificatesWebConfiguration digitalCertificatesPdfConfiguration,
            IDownloadCertificateService downloadCertificateService)
            : base(mediator, httpContextAccessor)
        {
            _sessionService = sessionService;
            _userService = userService;
            _blob = blob;
            _asposeLicenseService = apposeLicenseService;
            _digitalCertificatesWebConfiguration = digitalCertificatesPdfConfiguration;
            _downloadCertificateService = downloadCertificateService;
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

        public async Task<DownloadCertificateViewModel?> GetDownloadCertificateViewModelAsync(Guid certificateId)
        {
            var result = await Mediator.Send(new GetStandardCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var model = result.ToDownloadCertificateRequest(certificateId);

            return _downloadCertificateService.CreateDownloadCertificateViewModel(model);
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

            var (printStatus, printDate, printMessage) = MapPrintStatus(result.DeliveryInformation);
            viewModel.PrintStatus = printStatus;
            viewModel.PrintStatusDate = printDate;
            viewModel.PrintStatusMessage = printMessage;
            viewModel.PrintStatusDisplay = printStatus == Enums.PrintStatus.Requested ? "Print requested" : printStatus.ToString();
            viewModel.ShowPrintHeader = printStatus != Enums.PrintStatus.None && printStatus != Enums.PrintStatus.Submitted;
            viewModel.PrintStatusCssClass = CssClassForStatus(printStatus);
            viewModel.ShowRequestPrint = printStatus == Enums.PrintStatus.Submitted && viewModel.PrintRequestedAt == null;

            var owned = await _sessionService.GetOwnedCertificatesAsync();

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        public async Task<DownloadCertificateViewModel?> GetDownloadFrameworkCertificateViewModelAsync(Guid certificateId)
        {
            var result = await Mediator.Send(new GetFrameworkCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var model = result.ToDownloadCertificateRequest(certificateId);

            return _downloadCertificateService.CreateDownloadCertificateViewModel(model);
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
                FrameworkCertificateNumber = result.FrameworkCertificateNumber,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                ProviderName = result.ProviderName,
                EmployerName = result.EmployerName,
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
            viewModel.PrintStatusDisplay = printStatus == Enums.PrintStatus.Requested ? "Print requested" : printStatus.ToString();
            viewModel.PrintStatusCssClass = CssClassForStatus(printStatus);
            viewModel.ShowPrintHeader = printStatus != Enums.PrintStatus.None && printStatus != Enums.PrintStatus.Submitted;

            var owned = await _sessionService.GetOwnedCertificatesAsync();

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        [ExcludeFromCodeCoverage]
        public async Task<byte[]> GenerateCertificateAsync(DownloadCertificateViewModel model)
        {
            byte[]? templateBytes = null;
            var fullName = $"{model.GivenNames} {model.FamilyName}";

            if(fullName.Length >= _digitalCertificatesWebConfiguration.MaxFullNameLengthOnOneLine)
            {
                fullName = $"{model.GivenNames}\n{model.FamilyName}";
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {                             
                [FullName] = fullName,
                [AwardedOn] = $"{model.DateAwarded:dd} {model.DateAwarded.ToString("MMMM", CultureInfo.InvariantCulture).ToUpperInvariant()} {model.DateAwarded:yyyy}",
                [CertificateNumber] = model.CertificateNumber
            };

            if (model.CertificateType == CertificateType.Standard)
            {
                templateBytes = model.CoronationEmblem ? await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.ContainerName, _digitalCertificatesWebConfiguration.GreenStandardTemplateBlobName)
                                                        : await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.ContainerName, _digitalCertificatesWebConfiguration.StandardTemplateBlobName);
                values.Add(AchievedGrade, model.OverallGrade?.ToUpper() ?? string.Empty);
                values.Add(PassedInfo, string.Join(Environment.NewLine,
                    new[]
                    {
                model.CourseName.ToUpper(),
                model.CourseOption?.ToUpper(),
                $"{Level} {model.CourseLevel.ToUpper()}"
                    }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            else if (model.CertificateType == CertificateType.Framework)
            {
                templateBytes = await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.ContainerName, _digitalCertificatesWebConfiguration.FrameworkTemplateBlobName);
                values.Add(PassedInfo, string.Join(Environment.NewLine,
                        new[]
                        {
                model.CourseName.ToUpper(),
                model.CourseOption?.ToUpper(),
                $"{model.CourseLevel.ToUpper()}  {Level}"
                        }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }

            if (templateBytes is null)
            {
                throw new InvalidOperationException("Template bytes were not loaded.");
            }
                       
            var output = await CreatePDFMemoryStream(templateBytes, values);
            return output.ToArray();
        }

        [ExcludeFromCodeCoverage]
        private async Task<MemoryStream> CreatePDFMemoryStream(byte[] templateBytes, Dictionary<string, string> values)
        {
            await _asposeLicenseService.GetAsposeLicense();

            var templateStream = new MemoryStream(templateBytes);
            var document = new Document(templateStream);
            var fields = document.Form.Fields.Cast<Field>().ToList();

            ValidateRequiredFields(fields, values.Keys);

            foreach (var kv in values)
            {
                var field = FindField(fields, kv.Key);

                if (field == null)
                {
                    continue;
                }

                if (field is TextBoxField textBox)
                {
                    textBox.Value = kv.Value ?? string.Empty;
                    textBox.ReadOnly = true;
                }
                else
                {
                    field.Value = kv.Value ?? string.Empty;
                }
            }

            document.Form.Flatten();
            var output = new MemoryStream();
            document.Encrypt(
                userPassword: "",
                ownerPassword: _digitalCertificatesWebConfiguration.MasterPassword,
                permissions: Permissions.PrintDocument,
                cryptoAlgorithm: CryptoAlgorithm.AESx128);

            document.Save(output);
            return output;
        }

        private static void ValidateRequiredFields(IEnumerable<Field> fields, IEnumerable<string> requiredFieldNames)
        {
            var missingFields = requiredFieldNames
                .Where(required => FindField(fields, required) is null)
                .ToList();

            if (missingFields.Count != 0)
            {
                throw new InvalidOperationException(
                    $"The PDF template is missing required field(s): {string.Join(", ", missingFields)}");
            }
        }

        private static Field? FindField(IEnumerable<Field> fields, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return fields.FirstOrDefault(f =>
                string.Equals(f.PartialName, key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.FullName, key, StringComparison.OrdinalIgnoreCase));
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

            var familyName = GetUserSurname();
            var givenNames = GetUserGivenNames();

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

            var familyName = GetUserSurname();
            var givenNames = GetUserGivenNames();

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

            var viewModel = new SelectAddressViewModel
            {
                CertificateId = certificateId,
                CourseName = ownedCertificate.CourseName,
                GivenNames = GetUserGivenNames(),
                FamilyName = GetUserSurname(),
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

            var viewModel = new AddAddressManualViewModel
            {
                CertificateId = certificateId,
                CourseName = ownedCertificate.CourseName,
                GivenNames = GetUserGivenNames(),
                FamilyName = GetUserSurname()
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

            var vm = new CheckAndSubmitViewModel
            {
                CertificateId = certificateId,
                CourseName = ownedCertificate.CourseName,
                GivenNames = GetUserGivenNames(),
                FamilyName = GetUserSurname()
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
            string email = GetUserEmail();
            string userName = GetUserDisplayName();

            var templateId = GetTemplateId(_digitalCertificatesWebConfiguration, NotificationTemplateNames.PrintRequest);

            var deliveryAddress = await _sessionService.GetDeliveryAddressAsync();

            var req = new CreatePrintRequest
            {
                Address = new CreatePrintAddressRequest
                {
                    ContactName = userName,
                    ContactOrganisation = deliveryAddress?.Organisation,
                    ContactAddLine1 = deliveryAddress?.AddressLine1,
                    ContactAddLine2 = deliveryAddress?.AddressLine2,
                    ContactAddLine3 = deliveryAddress?.TownOrCity,
                    ContactAddLine4 = deliveryAddress?.County,
                    ContactPostCode = deliveryAddress?.Postcode ?? string.Empty
                },
                Email = new CreatePrintEmailRequest
                {
                    EmailAddress = email,
                    UserName = userName,
                    LinkDomain = _digitalCertificatesWebConfiguration.ServiceBaseUrl,
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
            var cutoverDate = _digitalCertificatesWebConfiguration.CutoverDate;

            if (deliveryInformation == null || !deliveryInformation.Any())
            {
                return (Enums.PrintStatus.None, null, null);
            }

            // If a cutover date is configured, ensure we have relevant events after it
            if (cutoverDate.HasValue)
            {
                var relevantEvents = deliveryInformation.Where(d =>
                    string.Equals(d.Status, DeliveryInformationStatuses.Submitted, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.Status, DeliveryInformationStatuses.Reprint, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d.Status, DeliveryInformationStatuses.Printed, StringComparison.OrdinalIgnoreCase));

                var hasRelevantAfterCutover = relevantEvents.Any() && relevantEvents.All(d => d.EventTime > cutoverDate.Value);

                if (!hasRelevantAfterCutover)
                {
                    return (Enums.PrintStatus.None, null, null);
                }
            }

            var ordered = deliveryInformation
                .OrderByDescending(e => e.EventTime)
                .ToList();

            var latest = ordered.First();
            var dt = latest.EventTime;

            switch (latest.Status)
            {
                case var s when string.Equals(s, DeliveryInformationStatuses.Delivered, StringComparison.OrdinalIgnoreCase):
                    {
                        var msg = $"A certificate was delivered on {dt:dd MMMM yyyy}.";
                        return (Enums.PrintStatus.Delivered, dt, msg);
                    }

                case var s when string.Equals(s, DeliveryInformationStatuses.Printed, StringComparison.OrdinalIgnoreCase):
                    {
                        var msg = $"A certificate was printed on {dt:dd MMMM yyyy}. It can take up to 3 weeks to be delivered.";
                        return (Enums.PrintStatus.Printed, dt, msg);
                    }

                case var s when string.Equals(s, DeliveryInformationStatuses.Submitted, StringComparison.OrdinalIgnoreCase):
                    {
                        return (Enums.PrintStatus.Submitted, null, null);
                    }

                case var s when string.Equals(s, DeliveryInformationStatuses.SentToPrinter, StringComparison.OrdinalIgnoreCase)
                           || string.Equals(s, DeliveryInformationStatuses.Reprint, StringComparison.OrdinalIgnoreCase)
                           || string.Equals(s, DeliveryInformationStatuses.PrintRequested, StringComparison.OrdinalIgnoreCase):
                    {
                        var msg = $"A certificate was requested on {dt:dd MMMM yyyy}. It can take up to 3 weeks to be delivered.";
                        return (Enums.PrintStatus.Requested, dt, msg);
                    }

                default:
                    return (Enums.PrintStatus.None, null, null);
            }
        }

        private string CssClassForStatus(Enums.PrintStatus status)
        {
            return status switch
            {
                Enums.PrintStatus.Delivered => "status-tag status-tag--delivered",
                Enums.PrintStatus.Printed => "status-tag status-tag--requested",
                Enums.PrintStatus.Requested => "status-tag status-tag--requested",
                _ => "status-tag status-tag--neutral",
            };
        }
    }
}

