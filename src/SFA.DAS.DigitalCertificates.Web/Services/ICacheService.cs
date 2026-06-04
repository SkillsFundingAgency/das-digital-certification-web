using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Domain.Models;
namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ICacheService
    {
        Task<User?> GetUserAsync(string govUkIdentifier);
        Task<MatchesAndMasks?> GetOrCreateMatchesAsync(string govUkIdentifier, Guid userId);
        Task<int> GetMatchFailCountAsync(string govUkIdentifier);
        Task<int> IncrementMatchFailCountAsync(string govUkIdentifier);
        Task ClearUser(string govUkIdentifier);
        Task ClearMatchFailCountAsync(string govUkIdentifier);
        Task ClearMatches(string govUkIdentifier);
    }
}