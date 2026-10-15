using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly IHttpContextAccessor? _contextAccessor;
        public BaseController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public new HttpContext HttpContext =>
            _contextAccessor?.HttpContext
            ?? throw new InvalidOperationException("No HttpContext available.");
    }
}
