using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharings;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharingEmail;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Services;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Domain.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class SharingOrchestrator : BaseOrchestrator, ISharingOrchestrator
    {
        private readonly IUserService _userService;
        private readonly ISessionStorageService _sessionStorageService;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IValidator<ShareByEmailViewModel> _shareByEmailValidator;

        public SharingOrchestrator(IMediator mediator, IUserService userService, ISessionStorageService sessionStorageService,
            DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration,
            IDateTimeHelper dateTimeHelper,
            IValidator<ShareByEmailViewModel> shareByEmailValidator)
            : base(mediator)
        {
            _userService = userService;
            _sessionStorageService = sessionStorageService;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
            _shareByEmailValidator = shareByEmailValidator;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<CreateCertificateSharingViewModel> GetSharings(Guid certificateId)
        {
            var userId = _userService.GetUserId()!.Value;

            var certificate = await GetCertificateFromSessionAsync(certificateId);

            if (certificate == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new GetSharingsQuery
            {
                UserId = userId,
                CertificateId = certificateId,
                Limit = _digitalCertificatesWebConfiguration.SharingListLimit
            });

            if (response == null)
            {
                return new CreateCertificateSharingViewModel
                {
                    CertificateId = certificateId,
                    CourseName = certificate.CourseName,
                    CertificateType = certificate.CertificateType,
                    Sharings = new List<CreateCertificateSharingItemViewModel>()
                };
            }

            return new CreateCertificateSharingViewModel
            {
                CertificateId = response.CertificateId,
                CourseName = response.CourseName,
                CertificateType = certificate.CertificateType,
                Sharings = response.Sharings?.Select(s => new CreateCertificateSharingItemViewModel
                {
                    SharingId = s.SharingId,
                    SharingNumber = s.SharingNumber,
                    CreatedAt = s.CreatedAt,
                    ExpiryTime = s.ExpiryTime
                }).ToList()
                ?? new List<CreateCertificateSharingItemViewModel>()
            };
        }

        public async Task<Guid> CreateSharing(Guid certificateId)
        {
            var userId = _userService.GetUserId()!.Value;

            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new CreateSharingCommand
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = certificateData.CertificateType,
                CourseName = certificateData.CourseName
            });

            return response?.SharingId ?? Guid.Empty;
        }

        public async Task<CertificateSharingLinkViewModel> GetSharingById(Guid certificateId, Guid sharingId)
        {
            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new GetSharingByIdQuery
            {
                SharingId = sharingId,
                Limit = _digitalCertificatesWebConfiguration.SharingHistoryLimit
            });

            if (response == null || response.ExpiryTime <= DateTime.UtcNow)
            {
                return null!;
            }

            var viewModel = new CertificateSharingLinkViewModel
            {
                CertificateId = response.CertificateId,
                CourseName = response.CourseName,
                CertificateType = response.CertificateType,
                SharingId = response.SharingId,
                SharingNumber = response.SharingNumber,
                CreatedAt = response.CreatedAt,
                ExpiryTime = response.ExpiryTime,
                LinkCode = response.LinkCode,
                FormattedExpiry = response.ExpiryTime.ToUkExpiryDateTimeString(),
                FormattedCreated = response.CreatedAt.ToUkDateTimeString(),
                FormattedAccessTimes = (response.SharingAccess ?? new List<DateTime>()).Select(a => a.ToUkDateTimeString()).ToList()
            };

            viewModel.SecureLink = $"{_digitalCertificatesWebConfiguration?.ServiceBaseUrl}/certificates/{viewModel.LinkCode}";

            return viewModel;
        }

        public async Task<ConfirmShareByEmailViewModel?> GetConfirmShareByEmail(Guid certificateId, Guid sharingId, string emailAddress)
        {
            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new GetSharingByIdQuery
            {
                SharingId = sharingId,
                Limit = _digitalCertificatesWebConfiguration.SharingHistoryLimit
            });

            if (response == null || response.ExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return new ConfirmShareByEmailViewModel
            {
                CertificateId = response.CertificateId,
                SharingId = response.SharingId,
                CourseName = response.CourseName,
                SharingNumber = response.SharingNumber,
                EmailAddress = emailAddress,
                FormattedExpiry = response.ExpiryTime.ToUkExpiryDateTimeString()
            };
        }

        public async Task<Guid> CreateSharingEmail(Guid certificateId, Guid sharingId, string emailAddress)
        {
            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var sharingResponse = await Mediator.Send(new GetSharingByIdQuery
            {
                SharingId = sharingId,
                Limit = _digitalCertificatesWebConfiguration.SharingHistoryLimit
            });

            if (sharingResponse == null)
            {
                return Guid.Empty;
            }

            var messageText = $"This link will stop working at {sharingResponse.ExpiryTime.ToUkExpiryDateTimeString()}.";

            var userName = await GetUserDisplayNameAsync();

            var result = await Mediator.Send(new CreateSharingEmailCommand
            {
                SharingId = sharingId,
                EmailAddress = emailAddress,
                UserName = userName,
                LinkDomain = _digitalCertificatesWebConfiguration.ServiceBaseUrl,
                MessageText = messageText,
                TemplateId = _digitalCertificatesWebConfiguration.SharingEmailTemplateId
            });

            return result?.Id ?? Guid.Empty;
        }

        public async Task<EmailSentViewModel?> GetEmailSent(Guid certificateId, Guid sharingId, Guid sharingEmailId)
        {
            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new GetSharingByIdQuery
            {
                SharingId = sharingId,
                Limit = _digitalCertificatesWebConfiguration.SharingHistoryLimit
            });

            if (response == null)
            {
                return null;
            }

            var matchingEmail = response.SharingEmails?.FirstOrDefault(e => e.SharingEmailId == sharingEmailId);

            var viewModel = new EmailSentViewModel
            {
                CertificateId = response.CertificateId,
                SharingId = response.SharingId,
                SharingNumber = response.SharingNumber,
                EmailAddress = matchingEmail?.EmailAddress ?? string.Empty,
                FormattedExpiry = response.ExpiryTime.ToUkExpiryDateTimeString(),
                CourseName = response.CourseName,
            };

            var ownedCertificate = await _sessionStorageService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());

            viewModel.IsSingleCertificate = (ownedCertificate?.Count ?? 0) == 1;

            return viewModel;
        }

        private async Task<Certificate?> GetCertificateFromSessionAsync(Guid certificateId)
        {
            var govUkIdentifier = _userService.GetGovUkIdentifier();
            if (string.IsNullOrEmpty(govUkIdentifier))
            {
                return null;
            }

            var certificates = await _sessionStorageService.GetOwnedCertificatesAsync(govUkIdentifier);
            return certificates?.FirstOrDefault(c => c.CertificateId == certificateId);
        }

        private async Task<string> GetUserDisplayNameAsync()
        {
            var govUkIdentifier = _userService.GetGovUkIdentifier();
            if (string.IsNullOrEmpty(govUkIdentifier))
            {
                return string.Empty;
            }

            var user = await _sessionStorageService.GetUserAsync(govUkIdentifier);
            if (user == null)
            {
                return string.Empty;
            }

            var name = user.Names?.FirstOrDefault();
            if (name != null && !string.IsNullOrEmpty(name.GivenNames) && !string.IsNullOrEmpty(name.FamilyName))
            {
                return $"{name.GivenNames} {name.FamilyName}";
            }

            return string.Empty;
        }

        public async Task<bool> ValidateShareByEmailViewModel(ShareByEmailViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_shareByEmailValidator, viewModel, modelState);
        }
    }
}
