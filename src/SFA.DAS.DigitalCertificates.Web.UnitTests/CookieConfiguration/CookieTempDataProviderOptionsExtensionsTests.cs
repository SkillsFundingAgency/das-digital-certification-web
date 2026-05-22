using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.StartupExtensions;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.CookieConfiguration
{
    [TestFixture]
    public class CookieTempDataProviderOptionsExtensionsTests
    {
        private CookieTempDataProviderOptions _options;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddCookieTempDataProvider();
            _options = services.BuildServiceProvider()
                               .GetRequiredService<IOptions<CookieTempDataProviderOptions>>().Value;
        }

        [Test]
        public void AddCookieTempDataProvider_SetsHttpOnly()
            => _options.Cookie.HttpOnly.Should().BeTrue();

        [Test]
        public void AddCookieTempDataProvider_SetsIsEssential()
            => _options.Cookie.IsEssential.Should().BeTrue();

        [Test]
        public void AddCookieTempDataProvider_SetsSecurePolicyToAlways()
            => _options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always);
    }
}
