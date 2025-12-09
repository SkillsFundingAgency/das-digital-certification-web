using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateCertificateSharing;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateSharingDetails;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificateSharingOrchestrator : BaseOrchestrator, ICertificateSharingOrchestrator
    {
        private readonly IUserService _userService;
        private readonly ISessionStorageService _sessionStorageService;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;

        public CertificateSharingOrchestrator(IMediator mediator, IUserService userService, ISessionStorageService sessionStorageService, DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration)
          : base(mediator)
        {
            _userService = userService;
            _sessionStorageService = sessionStorageService;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
        }

        public async Task<CertificateSharingViewModel> GetCertificateSharings(Guid certificateId)
        {
            var userId = _userService.GetUserId()!.Value;

            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new GetCertificateSharingDetailsQuery
            {
                UserId = userId,
                CertificateId = certificateId,
                Limit = _digitalCertificatesWebConfiguration.SharingListLimit
            });

            if (response == null)
            {
                return new CertificateSharingViewModel
                {
                    CertificateId = certificateId,
                    CourseName = certificateData.CourseName,
                    CertificateType = certificateData.CertificateType,
                    Sharings = new List<CertificateSharingItemViewModel>()
                };
            }

            return new CertificateSharingViewModel
            {
                CertificateId = response.CertificateId,
                CourseName = response.CourseName,
                CertificateType = certificateData.CertificateType,
                Sharings = response.Sharings.Select(s => new CertificateSharingItemViewModel
                {
                    SharingId = s.SharingId,
                    SharingNumber = s.SharingNumber,
                    CreatedAt = s.CreatedAt,
                    ExpiryTime = s.ExpiryTime
                }).ToList()
            };
        }

        public async Task<Guid> CreateCertificateSharing(Guid certificateId)
        {
            var userId = _userService.GetUserId()!.Value;

            var certificateData = await GetCertificateFromSessionAsync(certificateId);

            if (certificateData == null)
            {
                throw new InvalidOperationException($"Certificate {certificateId} not found for authenticated user");
            }

            var response = await Mediator.Send(new CreateCertificateSharingCommand
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = certificateData.CertificateType.ToString(),
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
    }
}
