using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Models;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Models
{
    public class ErrorViewModelTests
    {
        private ErrorViewModel _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _sut = new ErrorViewModel
            {
                ProblemReference = "problem-123"
            };
        }

        [Test]
        public void SupportEmailSubject_ShouldMatchAcceptanceCriteria()
        {
            _sut.SupportEmailSubject.Should().Be(
                "Support with Apprenticeship certificates, reference: problem-123");
        }

        [Test]
        public void SupportEmailUrl_ShouldContainEmailAddress()
        {
            _sut.SupportEmailUrl.Should().StartWith(
                "mailto:helpdesk@manage-apprenticeships.service.gov.uk?subject=");
        }

        [Test]
        public void SupportEmailUrl_ShouldContainEncodedSubject()
        {
            var encodedSubject = _sut.SupportEmailUrl
                .Split("?subject=")[1];

            var decodedSubject = Uri.UnescapeDataString(encodedSubject);

            decodedSubject.Should().Be(_sut.SupportEmailSubject);
        }
    }
}