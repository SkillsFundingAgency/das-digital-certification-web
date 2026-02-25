using System.Threading.Tasks;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;
using SFA.DAS.DigitalCertificates.Domain.Models;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using System.Text.Json;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly IMediator _mediator;

        private const string UsernameKey = "DigitalCertificates:Username";
        private const string ShareEmailKey = "DigitalCertificates:ShareEmail";
        private const string OwnedCertificatesKeyPrefix = "DigitalCertificates:OwnedCertificates:";
        private const string UlnAuthorisationKeyPrefix = "DigitalCertificates:UlnAuthorisation:";

        public SessionService(ISessionStorageService sessionStorageService, IMediator mediator)
        {
            _sessionStorageService = sessionStorageService;
            _mediator = mediator;
        }

        public Task SetUsernameAsync(string username)
        {
            return _sessionStorageService.SetAsync(UsernameKey, username);
        }

        public Task<string?> GetUserNameAsync()
        {
            return _sessionStorageService.GetAsync(UsernameKey);
        }

        public Task SetShareEmailAsync(string email)
        {
            return _sessionStorageService.SetAsync(ShareEmailKey, email);
        }

        public Task<string?> GetShareEmailAsync()
        {
            return _sessionStorageService.GetAsync(ShareEmailKey);
        }

        public Task ClearShareEmailAsync()
        {
            return _sessionStorageService.ClearAsync(ShareEmailKey);
        }

        public async Task<List<Certificate>?> GetOwnedCertificatesAsync(string govUkIdentifier)
        {
            var json = await _sessionStorageService.GetAsync(OwnedCertificatesKeyPrefix + govUkIdentifier);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<Certificate>>(json);
            }

            var user = await _mediator.Send(new GetUserQuery { GovUkIdentifier = govUkIdentifier });
            if (user == null)
                return null;

            var response = await _mediator.Send(new GetCertificatesQuery { UserId = user.Id });
            var result = response as GetCertificatesQueryResult;
            var certificates = result?.Certificates;

            if (certificates != null)
            {
                var certJson = JsonSerializer.Serialize(certificates);
                await _sessionStorageService.SetAsync(OwnedCertificatesKeyPrefix + govUkIdentifier, certJson);
            }

            return certificates;
        }

        public async Task<UlnAuthorisation?> GetUlnAuthorisationAsync(string govUkIdentifier)
        {
            var json = await _sessionStorageService.GetAsync(UlnAuthorisationKeyPrefix + govUkIdentifier);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<UlnAuthorisation>(json);
            }

            var user = await _mediator.Send(new GetUserQuery { GovUkIdentifier = govUkIdentifier });
            if (user == null)
                return null;

            var response = await _mediator.Send(new GetCertificatesQuery { UserId = user.Id });
            var result = response as GetCertificatesQueryResult;
            var authorisation = result?.Authorisation;

            if (authorisation != null)
            {
                var authJson = JsonSerializer.Serialize(authorisation);
                await _sessionStorageService.SetAsync(UlnAuthorisationKeyPrefix + govUkIdentifier, authJson);
            }

            return authorisation;
        }
        public async Task ClearSessionDataAsync(string govUkIdentifier)
        {
            await _sessionStorageService.ClearAsync(ShareEmailKey);
            await _sessionStorageService.ClearAsync(UsernameKey);

            if (!string.IsNullOrEmpty(govUkIdentifier))
            {
                await _sessionStorageService.ClearAsync(OwnedCertificatesKeyPrefix + govUkIdentifier);
                await _sessionStorageService.ClearAsync(UlnAuthorisationKeyPrefix + govUkIdentifier);
            }
        }
    }
}
