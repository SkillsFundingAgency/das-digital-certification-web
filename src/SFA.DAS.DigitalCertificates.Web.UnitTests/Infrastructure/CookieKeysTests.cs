using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Infrastructure;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Infrastructure
{
    [TestFixture]
    public class CookieKeysTests
    {
        [Test]
        public void CookieKeys_ShouldBeStaticClass()
        {
            var type = typeof(CookieKeys);            
            type.IsAbstract.Should().BeTrue();
            type.IsSealed.Should().BeTrue();
        }

        [Test]
        public void DasSeenCookieMessage_HasExpectedValue()
        {
            CookieKeys.DasSeenCookieMessage.Should().Be("DASSeenCookieMessage");
            CookieKeys.DasSeenCookieMessage.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void AnalyticsConsent_HasExpectedValue()
        {
            CookieKeys.AnalyticsConsent.Should().Be(nameof(CookieKeys.AnalyticsConsent));
            CookieKeys.AnalyticsConsent.Should().NotBeNullOrWhiteSpace();
        }
    }
}
