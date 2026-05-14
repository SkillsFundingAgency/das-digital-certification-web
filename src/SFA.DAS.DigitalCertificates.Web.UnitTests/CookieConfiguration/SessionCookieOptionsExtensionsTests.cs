using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.StartupExtensions;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.CookieConfiguration
{
    [TestFixture]
    public class SessionCookieOptionsExtensionsTests
    {
        private SessionOptions _options;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddSecureSessionCookie();
            _options = services.BuildServiceProvider()
                               .GetRequiredService<IOptions<SessionOptions>>().Value;
        }

        [Test]
        public void AddSecureSessionCookie_SetsCookieName()
            => _options.Cookie.Name.Should().Be(".AspNetCore.Session");

        [Test]
        public void AddSecureSessionCookie_SetsHttpOnly()
            => _options.Cookie.HttpOnly.Should().BeTrue();

        [Test]
        public void AddSecureSessionCookie_SetsIsEssential()
            => _options.Cookie.IsEssential.Should().BeTrue();

        [Test]
        public void AddSecureSessionCookie_SetsSecurePolicyToAlways()
            => _options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);

        [Test]
        public void AddSecureSessionCookie_SetsSameSiteToStrict()
            => _options.Cookie.SameSite.Should().Be(SameSiteMode.Strict);
    }
}
