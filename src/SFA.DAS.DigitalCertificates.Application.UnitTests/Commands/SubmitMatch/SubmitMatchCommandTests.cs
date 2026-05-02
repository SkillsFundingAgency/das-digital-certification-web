using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.SubmitMatch;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.UnitTests.Application.Commands.SubmitMatch
{
    [TestFixture]
    public class SubmitMatchCommandTests
    {
        [Test]
        public void Implicit_Conversion_To_SubmitMatchRequest_Maps_All_Fields()
        {
            // Arrange
            var cmd = new SubmitMatchCommand
            {
                UserId = Guid.NewGuid(),
                Uln = 1234567890L,
                FamilyName = "Smith",
                DateOfBirth = new DateTime(1990,1,2),
                CertificateType = "Standard",
                CourseCode = "C1",
                CourseName = "Course One",
                CourseLevel = "3",
                DateAwarded = 2019,
                ProviderName = "Provider A",
                Ukprn = 12345,
                IsMatched = true,
                IsFailed = false
            };

            // Act
            SubmitMatchRequest req = cmd;

            // Assert
            req.Uln.Should().Be(cmd.Uln);
            req.FamilyName.Should().Be(cmd.FamilyName);
            req.DateOfBirth.Should().Be(cmd.DateOfBirth);
            req.CertificateType.Should().Be(cmd.CertificateType);
            req.CourseCode.Should().Be(cmd.CourseCode);
            req.CourseName.Should().Be(cmd.CourseName);
            req.CourseLevel.Should().Be(cmd.CourseLevel);
            req.DateAwarded.Should().Be(cmd.DateAwarded);
            req.ProviderName.Should().Be(cmd.ProviderName);
            req.Ukprn.Should().Be(cmd.Ukprn);
            req.IsMatched.Should().Be(cmd.IsMatched);
            req.IsFailed.Should().Be(cmd.IsFailed);
        }
    }
}
