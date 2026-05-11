using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface ILocationsOrchestrator
    {
        Task<GetLocationsQueryResult> GetLocations(string searchTerm);
    }
}
