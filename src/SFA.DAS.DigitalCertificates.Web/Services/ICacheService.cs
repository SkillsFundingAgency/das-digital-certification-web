using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.GovUK.Auth.Models;
namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ICacheService
    {
        Task<User?> GetUserAsync(string govUkIdentifier);
        Task<MatchesAndMasks?> GetMatchesAsync(string govUkIdentifier);
        Task<MatchesAndMasks?> CreateMatchesAsync(string govUkIdentifier, Guid userId, GovUkCredentialSubject govUkCredentialSubject);
        Task<int> GetMatchFailCountAsync(string govUkIdentifier);
        Task<int> IncrementMatchFailCountAsync(string govUkIdentifier);
        Task ClearUser(string govUkIdentifier);
        Task ClearMatchFailCountAsync(string govUkIdentifier);
        Task ClearMatches(string govUkIdentifier);
    }
}