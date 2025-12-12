using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificates;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetCertificates
{
    public class GetCertificatesQueryResultTests
    {
        [Test]
        public void When_SourceIsNull_Then_ResultIsNull()
        {
            // Arrange
            CertificatesResponse? source = null;

            // Act
            GetCertificatesQueryResult? result = source;

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_SourceHasAuthorisation_Then_MapsAuthorisation()
        {
            // Arrange
            var source = new CertificatesResponse
            {
                Authorisation = new UlnAuthorisation
                {
                    Uln = "123456",
                    AuthorisedAt = DateTime.Now,
                    AuthorisationId = Guid.NewGuid()
                }
            };

            // Act
            GetCertificatesQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.Authorisation.Should().NotBeNull();
            result.Authorisation!.Uln.Should().Be(source.Authorisation.Uln);
            result.Authorisation.AuthorisedAt.Should().Be(source.Authorisation.AuthorisedAt);
            result.Authorisation.AuthorisationId.Should().Be(source.Authorisation.AuthorisationId);
        }

        [Test]
        public void When_SourceHasNullCertificates_Then_ResultCertificatesIsNull()
        {
            // Arrange
            var source = new CertificatesResponse
            {
                Certificates = null
            };

            // Act
            GetCertificatesQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.Certificates.Should().BeNull();
        }

        [Test]
        public void When_SourceHasEmptyCertificates_Then_ResultCertificatesIsEmpty()
        {
            // Arrange
            var source = new CertificatesResponse
            {
                Certificates = new List<Certificate>()
            };

            // Act
            GetCertificatesQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.Certificates.Should().NotBeNull();
            result.Certificates.Should().BeEmpty();
        }

        [Test]
        public void When_SourceHasMultipleCertificates_Then_AllNonNullAreMapped()
        {
            // Arrange
            var standard = new Certificate { CertificateId = Guid.NewGuid(), CertificateType = "Standard", CourseName = "Bricklayer", CourseLevel = "1", DateAwarded = DateTime.Now };
            var framework = new Certificate { CertificateId = Guid.NewGuid(), CertificateType = "Framework", CourseName = "Plumber", CourseLevel = "Advanced", DateAwarded = DateTime.Now };

            var source = new CertificatesResponse
            {
                Certificates = new List<Certificate> { standard, framework }
            };

            // Act
            GetCertificatesQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.Certificates.Should().HaveCount(2);

            result.Certificates![0].CertificateId.Should().Be(standard.CertificateId);
            result.Certificates![0].CertificateType.Should().Be(Domain.Models.CertificateType.Standard);

            result.Certificates![1].CertificateId.Should().Be(framework.CertificateId);
            result.Certificates![1].CertificateType.Should().Be(Domain.Models.CertificateType.Framework);
        }
    }
}
