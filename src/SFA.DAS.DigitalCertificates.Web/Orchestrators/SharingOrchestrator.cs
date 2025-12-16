using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharings;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class SharingOrchestrator : BaseOrchestrator, ISharingOrchestrator
    {
        private readonly IUserService _userService;
        private readonly ISessionStorageService _sessionStorageService;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;

        public SharingOrchestrator(IMediator mediator, IUserService userService, ISessionStorageService sessionStorageService, DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration)
          : base(mediator)
        {
            _userService = userService;
            _sessionStorageService = sessionStorageService;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
        }

        public async Task<CreateCertificateSharingViewModel> GetSharings(Guid certificateId)
        {
            var userId = _userService.GetUserId()!.Value;

            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
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
                    CourseName = certificateData.CourseName,
                    CertificateType = certificateData.CertificateType,
                    Sharings = new List<CreateCertificateSharingItemViewModel>()
                };
            }

            return new CreateCertificateSharingViewModel
            {
                CertificateId = response.CertificateId,
                CourseName = response.CourseName,
                CertificateType = certificateData.CertificateType,
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

            if (response == null)
            {
                return null!;
            }

            var item = new CertificateSharingLinkViewModel
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

            item.SecureLink = $"{_digitalCertificatesWebConfiguration?.ServiceBaseUrl}/certificates/{item.LinkCode}";

            return item;
        }
    }
}
