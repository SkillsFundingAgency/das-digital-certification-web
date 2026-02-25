using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;

        public CertificatesOrchestrator(IMediator mediator, ISessionService sessionService, IUserService userService)
            : base(mediator)
        {
            _sessionService = sessionService;
            _userService = userService;
        }

        public async Task<CertificatesListViewModel> GetCertificatesListViewModel()
        {
            return new CertificatesListViewModel
            {
                Certificates = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier())
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

            var owned = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());

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

            var owned = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        public async Task<string?> CreateOrReuseUserActionForCertificate(Guid certificateId)
        {
            var userId = _userService.GetUserId();
            if (userId == null)
                return null;

            var userDetails = await _sessionService.GetUserDetailsAsync();

            var familyName = userDetails?.FamilyName ?? string.Empty;
            var givenNames = userDetails?.GivenNames ?? string.Empty;

            var owned = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());
            var ownedCertificate = owned?.FirstOrDefault(c => c.CertificateId == certificateId);

            var certificateType = ownedCertificate?.CertificateType;
            var courseName = ownedCertificate?.CourseName ?? string.Empty;

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

            return result?.ActionCode ?? string.Empty;
        }

        public async Task<string?> CreateOrReuseUserActionForNonSpecific()
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
                var owned = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());
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
    }
}
