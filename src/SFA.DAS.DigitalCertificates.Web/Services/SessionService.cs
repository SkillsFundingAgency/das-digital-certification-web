using System.Threading.Tasks;
using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;
using SFA.DAS.DigitalCertificates.Domain.Models;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using System.Text.Json;
using System;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        private const string ShareEmailKey = "DigitalCertificates:ShareEmail";
        private const string OwnedCertificatesKeyPrefix = "DigitalCertificates:OwnedCertificates:";
        private const string UlnAuthorisationKeyPrefix = "DigitalCertificates:UlnAuthorisation:";
        private const string AuthorisationAnswersKeyPrefix = "DigitalCertificates:AuthorisationAnswers:";
        private const string RecordedSharingAccessKey = "DigitalCertificates:RecordedSharingAccessCodes";
        private const string DeliveryAddressKeyPrefix = "DigitalCertificates:DeliveryAddress:";
        private const string ContactReferenceKey = "DigitalCertificates:ContactReference";

        public SessionService(ISessionStorageService sessionStorageService, IMediator mediator, IUserService userService)
        {
            _sessionStorageService = sessionStorageService;
            _mediator = mediator;
            _userService = userService;
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

        public async Task<List<Certificate>?> GetOwnedCertificatesAsync()
        {
            var json = await _sessionStorageService.GetAsync(OwnedCertificatesKeyPrefix);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<Certificate>>(json);
            }
            Guid? userId = _userService.GetUserId();
            if (userId == null)
            {
                var govUkIdentifier = _userService.GetGovUkIdentifier();
                if (string.IsNullOrEmpty(govUkIdentifier)) return null;

                var user = await _mediator.Send(new GetUserQuery { GovUkIdentifier = govUkIdentifier });
                if (user == null) return null;

                userId = user.Id;
            }

            var response = await _mediator.Send(new GetCertificatesQuery { UserId = userId.Value });
            var result = response as GetCertificatesQueryResult;
            var certificates = result?.Certificates;

            if (certificates != null)
            {
                var certJson = JsonSerializer.Serialize(certificates);
                await _sessionStorageService.SetAsync(OwnedCertificatesKeyPrefix, certJson);
            }

            return certificates;
        }

        public async Task<UlnAuthorisation?> GetUlnAuthorisationAsync()
        {
            var json = await _sessionStorageService.GetAsync(UlnAuthorisationKeyPrefix);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<UlnAuthorisation>(json);
            }
            Guid? userId = _userService.GetUserId();
            if (userId == null)
            {
                var govUkIdentifier = _userService.GetGovUkIdentifier();
                if (string.IsNullOrEmpty(govUkIdentifier)) return null;

                var user = await _mediator.Send(new GetUserQuery { GovUkIdentifier = govUkIdentifier });
                if (user == null) return null;

                userId = user.Id;
            }

            var response = await _mediator.Send(new GetCertificatesQuery { UserId = userId.Value });
            var result = response as GetCertificatesQueryResult;
            var authorisation = result?.Authorisation;

            if (authorisation != null)
            {
                var authJson = JsonSerializer.Serialize(authorisation);
                await _sessionStorageService.SetAsync(UlnAuthorisationKeyPrefix, authJson);
            }

            return authorisation;
        }

        public async Task<AuthorisationAnswers?> GetAuthorisationAnswersAsync()
        {
            var json = await _sessionStorageService.GetAsync(AuthorisationAnswersKeyPrefix);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<AuthorisationAnswers>(json);
            }

            return null;
        }

        public Task SetAuthorisationAnswersAsync(AuthorisationAnswers answers)
        {
            var json = JsonSerializer.Serialize(answers);
            return _sessionStorageService.SetAsync(AuthorisationAnswersKeyPrefix, json);
        }

        public Task ClearAuthorisationAnswersAsync()
        {
            return _sessionStorageService.ClearAsync(AuthorisationAnswersKeyPrefix);
        }
        public async Task ClearSessionDataAsync()
        {
            await _sessionStorageService.ClearAsync(ShareEmailKey);
            await _sessionStorageService.ClearAsync(OwnedCertificatesKeyPrefix);
            await _sessionStorageService.ClearAsync(UlnAuthorisationKeyPrefix);
            await _sessionStorageService.ClearAsync(RecordedSharingAccessKey);
            await _sessionStorageService.ClearAsync(DeliveryAddressKeyPrefix);
            await _sessionStorageService.ClearAsync(ContactReferenceKey);
        }

        public Task SetDeliveryAddressAsync(CheckAndSubmitViewModel address)
        {
            var json = JsonSerializer.Serialize(address);
            return _sessionStorageService.SetAsync(DeliveryAddressKeyPrefix, json);
        }

        public async Task<CheckAndSubmitViewModel?> GetDeliveryAddressAsync()
        {
            var json = await _sessionStorageService.GetAsync(DeliveryAddressKeyPrefix);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<CheckAndSubmitViewModel>(json);
        }

        public Task ClearDeliveryAddressAsync()
        {
            return _sessionStorageService.ClearAsync(DeliveryAddressKeyPrefix);
        }

        public Task SetContactReferenceAsync(string referenceNumber)
        {
            return _sessionStorageService.SetAsync(ContactReferenceKey, referenceNumber);
        }

        public Task<string?> GetContactReferenceAsync()
        {
            return _sessionStorageService.GetAsync(ContactReferenceKey);
        }

        public Task ClearContactReferenceAsync()
        {
            return _sessionStorageService.ClearAsync(ContactReferenceKey);
        }

        public async Task AddRecordedSharingAccessCodeAsync(Guid code)
        {
            var list = await GetSessionStringListAsync(RecordedSharingAccessKey);
            var codeString = code.ToString();
            if (!list.Contains(codeString))
            {
                list.Add(codeString);
                var json = JsonSerializer.Serialize(list);
                await _sessionStorageService.SetAsync(RecordedSharingAccessKey, json);
            }
        }

        public async Task<bool> IsSharingAccessCodeRecordedAsync(Guid code)
        {
            var list = await GetSessionStringListAsync(RecordedSharingAccessKey);
            return list.Contains(code.ToString());
        }

        private async Task<List<string>> GetSessionStringListAsync(string key)
        {
            var json = await _sessionStorageService.GetAsync(key);
            if (string.IsNullOrEmpty(json))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

        }
    }
}
