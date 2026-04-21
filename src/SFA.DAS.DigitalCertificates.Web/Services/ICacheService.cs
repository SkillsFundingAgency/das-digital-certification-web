using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Domain.Models;
namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface ICacheService
    {
        Task<User?> GetUserAsync(string govUkIdentifier);
        Task Clear(string govUkIdentifier);
        Task<MatchesAndMasks?> GetOrCreateMatchesAsync(string govUkIdentifier, Guid userId);
        Task<int> IncrementMatchFailCountAsync(string govUkIdentifier);

        Task ClearMatches(string govUkIdentifier);
    }
}