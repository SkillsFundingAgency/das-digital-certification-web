using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using SFA.DAS.DigitalCertificates.Web.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment webHostEnvironment)
        {
            _next = next;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            const string dasCdn = "das-at-frnt-end.azureedge.net " +
                                  "das-test-frnt-end.azureedge.net " +
                                  "das-test2-frnt-end.azureedge.net " +
                                  "das-pp-frnt-end.azureedge.net " +
                                  "das-prd-frnt-end.azureedge.net " +
                                  "das-mo-frnt-end.azureedge.net " +
                                  "das-demo-frnt-end.azureedge.net";

            context.Response.Headers.AddIfNotPresent("x-frame-options", new StringValues("DENY"));
            context.Response.Headers.AddIfNotPresent("x-content-type-options", new StringValues("nosniff"));
            context.Response.Headers.AddIfNotPresent("X-Permitted-Cross-Domain-Policies", new StringValues("none"));
            context.Response.Headers.AddIfNotPresent("x-xss-protection", new StringValues("0"));

            var connectSrc =
                "'self' https://www.google-analytics.com " +
                "https://region1.analytics.google.com https://*.applicationinsights.azure.com " +
                "https://js.monitor.azure.com https://*.zendesk.com https://*.zdassets.com " +
                "wss://*.zopim.com https://*.rcrsv.io";

            if (_webHostEnvironment.IsDevelopment())
            {
                connectSrc += " http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*";
            }

            var csp =
                $"default-src 'self'; " +

                $"script-src 'self' 'unsafe-inline' 'unsafe-eval' {dasCdn} " +
                $"https://www.googletagmanager.com https://tagmanager.google.com " +
                $"https://www.google-analytics.com https://ssl.google-analytics.com " +
                $"https://js.monitor.azure.com https://*.zdassets.com https://*.zopim.com " +
                $"https://*.rcrsv.io; " +

                $"style-src 'self' 'unsafe-inline' {dasCdn} " +
                $"https://tagmanager.google.com https://fonts.googleapis.com https://*.rcrsv.io; " +

                $"font-src 'self' {dasCdn} https://fonts.gstatic.com https://*.rcrsv.io data:; " +

                $"img-src 'self' data: {dasCdn} " +
                $"https://www.googletagmanager.com https://ssl.gstatic.com " +
                $"https://www.gstatic.com https://www.google-analytics.com; " +

                $"connect-src {connectSrc}; " +

                $"media-src 'self' https://*.zdassets.com data:; " +
                $"worker-src 'self' blob:; " +

                $"frame-src https://www.googletagmanager.com https://*.zendesk.com;";

            context.Response.Headers.AddIfNotPresent(
                "Content-Security-Policy",
                new StringValues(csp));

            await _next(context);
        }
    }
}