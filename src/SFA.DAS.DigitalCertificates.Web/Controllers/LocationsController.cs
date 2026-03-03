using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("locations")]
    [Authorize(Policy = nameof(PolicyNames.IsVerified))]
    public class LocationsController : Controller
    {
        private readonly ILocationsOrchestrator _locationsOrchestrator;

        public LocationsController(ILocationsOrchestrator locationsOrchestrator)
        {
            _locationsOrchestrator = locationsOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string searchTerm)
        {
            var result = await _locationsOrchestrator.GetLocations(searchTerm);

            var locations = result.Locations.Select(location => new { name = location.Name });

            return Json(new { locations });
        }
    }
}
