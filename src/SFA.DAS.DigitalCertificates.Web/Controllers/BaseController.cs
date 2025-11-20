using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Web.Authorization;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public BaseController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public IHttpContextAccessor HttpContextAccessor => _contextAccessor;

        public string UserId
        {
            get
            {
                return HttpContextAccessor.HttpContext.User.FindFirst(DigitalCertificateClaimsTypes.UserId)?.Value;
            }
        }
    }
}
