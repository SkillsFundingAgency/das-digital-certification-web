using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Enums;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Enums
{
    [TestFixture]
    public class GradeExtensionsTests
    {
        [TestCase(null, Grade.Unknown)]
        [TestCase("", Grade.Unknown)]
        [TestCase("DISTINCTION", Grade.Distinction)]
        [TestCase("Distinction", Grade.Distinction)]
        [TestCase("MERIT", Grade.Merit)]
        [TestCase("Credit", Grade.Credit)]
        [TestCase("NO GRADE AWARDED", Grade.NoGradeAwarded)]
        [TestCase("Pass", Grade.Pass)]
        [TestCase("pass with excellence", Grade.PassWithExcellence)]
        [TestCase("UNKNOWN_VAL", Grade.Unknown)]
        public void ParseFromApi_Maps_Values(string? input, Grade expected)
        {
            var result = GradeExtensions.ParseFromApi(input);
            result.Should().Be(expected);
        }

        [Test]
        public void ToBannerDisplay_Returns_Expected_Text()
        {
            Grade.Credit.ToBannerDisplay().Should().Be("You have passed your apprenticeship with credit");
            Grade.Distinction.ToBannerDisplay().Should().Be("You have passed your apprenticeship with distinction");
            Grade.Merit.ToBannerDisplay().Should().Be("You have passed your apprenticeship with merit");
            Grade.Outstanding.ToBannerDisplay().Should().Be("You have passed your apprenticeship with outstanding");
            Grade.Pass.ToBannerDisplay().Should().Be("You have passed your apprenticeship");
            Grade.PassWithExcellence.ToBannerDisplay().Should().Be("You have passed your apprenticeship with excellence");
            Grade.NoGradeAwarded.ToBannerDisplay().Should().Be("You have passed your apprenticeship");
            Grade.Unknown.ToBannerDisplay().Should().Be("You have passed your apprenticeship");
        }

        [Test]
        public void ToResultDisplay_Returns_Expected_Text()
        {
            Grade.Credit.ToResultDisplay().Should().Be("Credit");
            Grade.Distinction.ToResultDisplay().Should().Be("Distinction");
            Grade.Merit.ToResultDisplay().Should().Be("Merit");
            Grade.Outstanding.ToResultDisplay().Should().Be("Outstanding");
            Grade.Pass.ToResultDisplay().Should().Be("Pass");
            Grade.PassWithExcellence.ToResultDisplay().Should().Be("Pass with excellence");
            Grade.NoGradeAwarded.ToResultDisplay().Should().Be("No grade awarded");
            Grade.Unknown.ToResultDisplay().Should().Be(string.Empty);
        }

        [Test]
        public void IsAvailable_Behaves_As_Expected()
        {
            Grade.Unknown.IsAvailable().Should().BeFalse();
            Grade.NoGradeAwarded.IsAvailable().Should().BeFalse();
            Grade.Pass.IsAvailable().Should().BeFalse();

            Grade.Credit.IsAvailable().Should().BeTrue();
            Grade.Distinction.IsAvailable().Should().BeTrue();
            Grade.Merit.IsAvailable().Should().BeTrue();
            Grade.PassWithExcellence.IsAvailable().Should().BeTrue();
            Grade.Outstanding.IsAvailable().Should().BeTrue();
        }

        [Test]
        public void ToBannerDisplay_Returns_NonEmpty_For_Grades()
        {
            Grade.Unknown.ToBannerDisplay().Should().NotBeNullOrEmpty();
            Grade.NoGradeAwarded.ToBannerDisplay().Should().NotBeNullOrEmpty();
            Grade.Pass.ToBannerDisplay().Should().NotBeNullOrEmpty();
            Grade.Credit.ToBannerDisplay().Should().NotBeNullOrEmpty();
        }
    }
}
